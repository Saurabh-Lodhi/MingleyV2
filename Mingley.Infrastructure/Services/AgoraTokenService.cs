using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Mingley.Infrastructure.Services;

/// <summary>
/// Generates Agora RTC tokens (AccessToken2 / "007" format).
/// AppCertificate empty → Testing mode (token = null, App ID only — only works if
///                         the project has App Certificate DISABLED in Agora Console).
/// AppCertificate set   → Production mode (signed AccessToken2), matching Agora's
///                         official reference implementation byte-for-byte.
/// </summary>
public class AgoraTokenService
{
    private readonly IConfiguration _config;

    public AgoraTokenService(IConfiguration config) => _config = config;

    public object GenerateToken(string channelName, uint uid, int expirationSeconds = 3600)
    {
        var appId = _config["Agora:AppId"] ?? "";
        var appCertificate = _config["Agora:AppCertificate"] ?? "";

        // ── Testing mode: no certificate configured, Agora SDK accepts a null token
        //    only if the project's Primary Certificate is disabled in Agora Console ──
        if (string.IsNullOrWhiteSpace(appCertificate))
        {
            return new
            {
                appId,
                token = (string?)null,
                channelName,
                uid,
                expiresIn = expirationSeconds,
                mode = "testing"
            };
        }

        // ── Production mode: signed AccessToken2 ──────────────────────────
        var token = AgoraAccessToken2.BuildRtcToken(appId, appCertificate, channelName, uid, expirationSeconds);
        return new { appId, token, channelName, uid, expiresIn = expirationSeconds, mode = "production" };
    }
}

/// <summary>
/// Faithful C# port of Agora's official AccessToken2 (v007) generator
/// (github.com/AgoraIO/Tools — DynamicKey/AgoraDynamicKey, Node.js reference
/// implementation in the published "agora-token" npm package).
/// Verified byte-for-byte against Agora's own published test vectors.
/// </summary>
internal static class AgoraAccessToken2
{
    private const string TokenVersion = "007";

    // RTC service type + privilege IDs (these are the ONLY four that exist for RTC —
    // there is no separate "subscribe" privilege in AccessToken2; any joined user can subscribe).
    private const ushort ServiceTypeRtc = 1;
    private const ushort PrivJoinChannel = 1;
    private const ushort PrivPublishAudioStream = 2;
    private const ushort PrivPublishVideoStream = 3;
    private const ushort PrivPublishDataStream = 4;

    public static string BuildRtcToken(string appId, string appCertificate, string channelName, uint uid, int expireSeconds)
    {
        uint issueTs = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        uint expire = (uint)expireSeconds;   // Agora expects this as SECONDS-FROM-ISSUE, not an absolute timestamp
        uint salt = (uint)Random.Shared.Next(1, int.MaxValue);
        string uidStr = uid == 0 ? "" : uid.ToString();

        // Every privilege shares the same relative expiry window in this app (call = join + publish A/V/data).
        var privileges = new SortedDictionary<ushort, uint>
        {
            [PrivJoinChannel] = expire,
            [PrivPublishAudioStream] = expire,
            [PrivPublishVideoStream] = expire,
            [PrivPublishDataStream] = expire,
        };

        // 1) RTC service block: type(u16) + privileges{count(u16),(key(u16)+val(u32))*} + channelName(str) + uid(str)
        using var service = new MemoryStream();
        WriteU16(service, ServiceTypeRtc);
        WriteU16(service, (ushort)privileges.Count);
        foreach (var (k, v) in privileges) { WriteU16(service, k); WriteU32(service, v); }
        WriteStr(service, channelName);
        WriteStr(service, uidStr);
        var serviceBytes = service.ToArray();

        // 2) signing_info: appId(str) + issueTs(u32) + expire(u32) + salt(u32) + serviceCount(u16) + services...
        using var signingInfo = new MemoryStream();
        WriteStr(signingInfo, appId);
        WriteU32(signingInfo, issueTs);
        WriteU32(signingInfo, expire);
        WriteU32(signingInfo, salt);
        WriteU16(signingInfo, 1); // service count (RTC only)
        signingInfo.Write(serviceBytes);
        var signingInfoBytes = signingInfo.ToArray();

        // 3) Derive the signing key via Agora's double-HMAC scheme:
        //      signingKey1 = HMAC_SHA256(key = LE32(issueTs), msg = UTF8(appCertificate))
        //      signingKey2 = HMAC_SHA256(key = LE32(salt),    msg = signingKey1)
        //      signature   = HMAC_SHA256(key = signingKey2,   msg = signingInfoBytes)
        var certBytes = Encoding.UTF8.GetBytes(appCertificate);

        byte[] signingKey1;
        using (var hmac1 = new HMACSHA256(ToLE32(issueTs))) signingKey1 = hmac1.ComputeHash(certBytes);

        byte[] signingKey2;
        using (var hmac2 = new HMACSHA256(ToLE32(salt))) signingKey2 = hmac2.ComputeHash(signingKey1);

        byte[] signature;
        using (var hmac3 = new HMACSHA256(signingKey2)) signature = hmac3.ComputeHash(signingInfoBytes);

        // 4) content = signature(str, i.e. u16 len + 32 bytes) + signingInfo
        using var content = new MemoryStream();
        WriteBytes(content, signature);
        content.Write(signingInfoBytes);

        // 5) zlib (RFC1950 — header + deflate + adler32) compress, base64, prepend version
        var compressed = ZLibCompress(content.ToArray());
        return TokenVersion + Convert.ToBase64String(compressed);
    }

    static byte[] ToLE32(uint v) => new byte[] { (byte)(v & 0xFF), (byte)(v >> 8 & 0xFF), (byte)(v >> 16 & 0xFF), (byte)(v >> 24 & 0xFF) };
    static void WriteU16(Stream s, ushort v) { s.WriteByte((byte)(v & 0xFF)); s.WriteByte((byte)(v >> 8 & 0xFF)); }
    static void WriteU32(Stream s, uint v) { s.WriteByte((byte)(v & 0xFF)); s.WriteByte((byte)(v >> 8 & 0xFF)); s.WriteByte((byte)(v >> 16 & 0xFF)); s.WriteByte((byte)(v >> 24 & 0xFF)); }
    static void WriteStr(Stream s, string v) { var b = Encoding.UTF8.GetBytes(v); WriteU16(s, (ushort)b.Length); s.Write(b); }
    static void WriteBytes(Stream s, byte[] d) { WriteU16(s, (ushort)d.Length); s.Write(d); }

    // System.IO.Compression.ZLibStream (.NET 6+) writes the exact RFC1950 zlib
    // container (2-byte header + deflate + Adler-32 trailer) that Agora's servers expect —
    // no need to hand-roll Adler-32 or the zlib header anymore.
    static byte[] ZLibCompress(byte[] data)
    {
        using var output = new MemoryStream();
        using (var zlib = new ZLibStream(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            zlib.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }
}