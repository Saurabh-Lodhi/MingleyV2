using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Mingley.Infrastructure.Services;

/// <summary>
/// Generates Agora AccessToken2 (v007) — no NuGet package required.
/// Matches Agora's official Go/Java SDK token format exactly.
/// </summary>
public class AgoraTokenService
{
    private readonly IConfiguration _config;

    public AgoraTokenService(IConfiguration config) => _config = config;

    public object GenerateToken(string channelName, uint uid, int expirationSeconds = 3600)
    {
        var appId = _config["Agora:AppId"]!;
        var appCertificate = _config["Agora:AppCertificate"]!;

        var token = AgoraAccessToken2.Build(appId, appCertificate, channelName, uid, expirationSeconds);

        return new
        {
            appId,
            token,
            channelName,
            uid,
            expiresIn = expirationSeconds
        };
    }
}

// ─── Agora AccessToken2 (007) — matches official Go SDK exactly ───────────────

internal static class AgoraAccessToken2
{
    private const string TokenVersion = "007";

    // Service types
    private const ushort ServiceRtc = 1;

    // RTC privileges
    private const ushort PrivJoinChannel = 1;
    private const ushort PrivPublishAudio = 2;
    private const ushort PrivPublishVideo = 3;
    private const ushort PrivSubscribeAudio = 5;
    private const ushort PrivSubscribeVideo = 6;

    public static string Build(
        string appId,
        string appCertificate,
        string channelName,
        uint uid,
        int expirationSeconds = 3600)
    {
        // ── Timestamps ────────────────────────────────────────────────────────
        var now = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var expire = now + (uint)expirationSeconds;
        var salt = (uint)new Random().Next(1, int.MaxValue);

        var uidStr = uid == 0 ? "" : uid.ToString();

        var privileges = new Dictionary<ushort, uint>
        {
            [PrivJoinChannel] = expire,
            [PrivPublishAudio] = expire,
            [PrivPublishVideo] = expire,
            [PrivSubscribeAudio] = expire,
            [PrivSubscribeVideo] = expire,
        };

        // ── 1. Pack message body ──────────────────────────────────────────────
        // Layout: salt(u32) + expire(u32) + serviceCount(u16) + [serviceType(u16) + channel(str) + uid(str) + privCount(u16) + [privId(u16)+privExpire(u32)...]]
        using var msgStream = new MemoryStream();

        WriteUInt32(msgStream, salt);
        WriteUInt32(msgStream, expire);

        // number of services
        WriteUInt16(msgStream, 1);

        // RTC service block
        WriteUInt16(msgStream, ServiceRtc);
        WriteString(msgStream, channelName);
        WriteString(msgStream, uidStr);

        WriteUInt16(msgStream, (ushort)privileges.Count);
        foreach (var (key, val) in privileges)
        {
            WriteUInt16(msgStream, key);
            WriteUInt32(msgStream, val);
        }

        var msgBytes = msgStream.ToArray();

        // ── 2. HMAC-SHA256 signature ──────────────────────────────────────────
        // CRITICAL: sigInput = appId(utf8) + now(u32 LE) + salt(u32 LE) + msgBytes
        //           'now' (issue timestamp) NOT 'expire' — this matches Agora Go SDK
        using var sigInput = new MemoryStream();
        sigInput.Write(Encoding.UTF8.GetBytes(appId));
        WriteUInt32(sigInput, now);     // ← issue timestamp (NOT expire)
        WriteUInt32(sigInput, salt);
        sigInput.Write(msgBytes);

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appCertificate));
        var signature = hmac.ComputeHash(sigInput.ToArray());

        // ── 3. Assemble: len-prefixed(signature) + msgBytes ───────────────────
        using var tokenBody = new MemoryStream();
        WriteBytes(tokenBody, signature);
        tokenBody.Write(msgBytes);

        // ── 4. Zlib compress → Base64 → prepend version prefix ────────────────
        var compressed = ZlibCompress(tokenBody.ToArray());
        return TokenVersion + Convert.ToBase64String(compressed);
    }

    // ── Binary helpers (little-endian) ───────────────────────────────────────

    private static void WriteUInt16(Stream s, ushort v)
    {
        s.WriteByte((byte)(v & 0xFF));
        s.WriteByte((byte)((v >> 8) & 0xFF));
    }

    private static void WriteUInt32(Stream s, uint v)
    {
        s.WriteByte((byte)(v & 0xFF));
        s.WriteByte((byte)((v >> 8) & 0xFF));
        s.WriteByte((byte)((v >> 16) & 0xFF));
        s.WriteByte((byte)((v >> 24) & 0xFF));
    }

    private static void WriteString(Stream s, string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        WriteUInt16(s, (ushort)bytes.Length);
        s.Write(bytes);
    }

    private static void WriteBytes(Stream s, byte[] data)
    {
        WriteUInt16(s, (ushort)data.Length);
        s.Write(data);
    }

    // ── Zlib (RFC 1950): 0x78 0x9C header + deflate + Adler-32 checksum ──────
    private static byte[] ZlibCompress(byte[] data)
    {
        using var output = new MemoryStream();

        // zlib header: CMF=0x78 (deflate, window=32k), FLG=0x9C (best compression, no dict)
        output.WriteByte(0x78);
        output.WriteByte(0x9C);

        using (var deflate = new DeflateStream(output, CompressionLevel.Optimal, leaveOpen: true))
            deflate.Write(data, 0, data.Length);

        // Adler-32 checksum (big-endian) appended after compressed data
        var adler = Adler32(data);
        output.WriteByte((byte)((adler >> 24) & 0xFF));
        output.WriteByte((byte)((adler >> 16) & 0xFF));
        output.WriteByte((byte)((adler >> 8) & 0xFF));
        output.WriteByte((byte)(adler & 0xFF));

        return output.ToArray();
    }

    private static uint Adler32(byte[] data)
    {
        const uint Mod = 65521;
        uint a = 1, b = 0;
        foreach (var bt in data)
        {
            a = (a + bt) % Mod;
            b = (b + a) % Mod;
        }
        return (b << 16) | a;
    }
}

//using System.IO.Compression;
//using System.Security.Cryptography;
//using System.Text;
//using Microsoft.Extensions.Configuration;

//namespace Mingley.Infrastructure.Services;

///// <summary>
///// Generates Agora AccessToken2 (v007) — no NuGet package required.
///// </summary>
//public class AgoraTokenService
//{
//    private readonly IConfiguration _config;

//    public AgoraTokenService(IConfiguration config) => _config = config;

//    public object GenerateToken(string channelName, uint uid, int expirationSeconds = 3600)
//    {
//        var appId = _config["Agora:AppId"]!;
//        var appCertificate = _config["Agora:AppCertificate"]!;

//        var token = AgoraAccessToken2.Build(appId, appCertificate, channelName, uid, expirationSeconds);

//        return new
//        {
//            appId,
//            token,
//            channelName,
//            uid,
//            expiresIn = expirationSeconds
//        };
//    }
//}

//// ─── Agora AccessToken2 (007) — self-contained ────────────────────────────────

//internal static class AgoraAccessToken2
//{
//    private const string TokenVersion = "007";

//    private const ushort ServiceRtc = 1;

//    private const ushort PrivJoinChannel = 1;
//    private const ushort PrivPublishAudio = 2;
//    private const ushort PrivPublishVideo = 3;
//    private const ushort PrivSubscribeAudio = 5;
//    private const ushort PrivSubscribeVideo = 6;

//    public static string Build(
//        string appId,
//        string appCertificate,
//        string channelName,
//        uint uid,
//        int expirationSeconds = 3600)
//    {
//        var now = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
//        var expire = now + (uint)expirationSeconds;
//        var salt = (uint)new Random().Next(1, int.MaxValue);

//        var uidStr = uid == 0 ? "" : uid.ToString();

//        var privileges = new Dictionary<ushort, uint>
//        {
//            [PrivJoinChannel] = expire,
//            [PrivPublishAudio] = expire,
//            [PrivPublishVideo] = expire,
//            [PrivSubscribeAudio] = expire,
//            [PrivSubscribeVideo] = expire,
//        };

//        // ── 1. Pack message body ───────────────────────────────────────────────
//        using var msgStream = new MemoryStream();

//        WriteUInt32(msgStream, salt);
//        WriteUInt32(msgStream, expire);

//        // ✅ FIX: number of services MUST come before the first service block
//        WriteUInt16(msgStream, 1);

//        // RTC service block
//        WriteUInt16(msgStream, ServiceRtc);
//        WriteString(msgStream, channelName);
//        WriteString(msgStream, uidStr);

//        WriteUInt16(msgStream, (ushort)privileges.Count);
//        foreach (var (key, val) in privileges)
//        {
//            WriteUInt16(msgStream, key);
//            WriteUInt32(msgStream, val);
//        }

//        var msgBytes = msgStream.ToArray();

//        // ── 2. HMAC-SHA256 signature ───────────────────────────────────────────
//        using var sigInput = new MemoryStream();
//        sigInput.Write(Encoding.UTF8.GetBytes(appId));
//        WriteUInt32(sigInput, expire);
//        WriteUInt32(sigInput, salt);
//        sigInput.Write(msgBytes);

//        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appCertificate));
//        var signature = hmac.ComputeHash(sigInput.ToArray());

//        // ── 3. Assemble: len-prefixed signature + message ──────────────────────
//        using var tokenBody = new MemoryStream();
//        WriteBytes(tokenBody, signature);
//        tokenBody.Write(msgBytes);

//        // ── 4. Zlib compress → Base64 → prepend "007" ─────────────────────────
//        var compressed = ZlibCompress(tokenBody.ToArray());
//        return TokenVersion + Convert.ToBase64String(compressed);
//    }

//    private static void WriteUInt16(Stream s, ushort v)
//    {
//        s.WriteByte((byte)(v & 0xFF));
//        s.WriteByte((byte)((v >> 8) & 0xFF));
//    }

//    private static void WriteUInt32(Stream s, uint v)
//    {
//        s.WriteByte((byte)(v & 0xFF));
//        s.WriteByte((byte)((v >> 8) & 0xFF));
//        s.WriteByte((byte)((v >> 16) & 0xFF));
//        s.WriteByte((byte)((v >> 24) & 0xFF));
//    }

//    private static void WriteString(Stream s, string str)
//    {
//        var bytes = Encoding.UTF8.GetBytes(str);
//        WriteUInt16(s, (ushort)bytes.Length);
//        s.Write(bytes);
//    }

//    private static void WriteBytes(Stream s, byte[] data)
//    {
//        WriteUInt16(s, (ushort)data.Length);
//        s.Write(data);
//    }

//    private static byte[] ZlibCompress(byte[] data)
//    {
//        using var output = new MemoryStream();
//        output.WriteByte(0x78);
//        output.WriteByte(0x9C);

//        using (var deflate = new DeflateStream(output, CompressionLevel.Optimal, leaveOpen: true))
//            deflate.Write(data, 0, data.Length);

//        var adler = Adler32(data);
//        output.WriteByte((byte)((adler >> 24) & 0xFF));
//        output.WriteByte((byte)((adler >> 16) & 0xFF));
//        output.WriteByte((byte)((adler >> 8) & 0xFF));
//        output.WriteByte((byte)(adler & 0xFF));

//        return output.ToArray();
//    }

//    private static uint Adler32(byte[] data)
//    {
//        const uint Mod = 65521;
//        uint a = 1, b = 0;
//        foreach (var bt in data)
//        {
//            a = (a + bt) % Mod;
//            b = (b + a) % Mod;
//        }
//        return (b << 16) | a;
//    }
//}


//////using System.IO.Compression;
//////using System.Security.Cryptography;
//////using System.Text;
//////using Microsoft.Extensions.Configuration;

//////namespace Mingley.Infrastructure.Services;

///////// <summary>
///////// Generates Agora AccessToken2 (v007) — no NuGet package required.
///////// </summary>
//////public class AgoraTokenService
//////{
//////    private readonly IConfiguration _config;

//////    public AgoraTokenService(IConfiguration config) => _config = config;

//////    public object GenerateToken(string channelName, uint uid, int expirationSeconds = 3600)
//////    {
//////        var appId = _config["Agora:AppId"]!;
//////        var appCertificate = _config["Agora:AppCertificate"]!;

//////        var token = AgoraAccessToken2.Build(appId, appCertificate, channelName, uid, expirationSeconds);

//////        return new
//////        {
//////            appId,
//////            token,
//////            channelName,
//////            uid,
//////            expiresIn = expirationSeconds
//////        };
//////    }
//////}

//////// ─── Agora AccessToken2 (007) — self-contained ────────────────────────────────

//////internal static class AgoraAccessToken2
//////{
//////    private const string TokenVersion = "007";

//////    private const ushort ServiceRtc = 1;

//////    private const ushort PrivJoinChannel = 1;
//////    private const ushort PrivPublishAudio = 2;
//////    private const ushort PrivPublishVideo = 3;
//////    private const ushort PrivSubscribeAudio = 5;
//////    private const ushort PrivSubscribeVideo = 6;

//////    public static string Build(
//////        string appId,
//////        string appCertificate,
//////        string channelName,
//////        uint uid,
//////        int expirationSeconds = 3600)
//////    {
//////        var now = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
//////        var expire = now + (uint)expirationSeconds;
//////        var salt = (uint)new Random().Next(1, int.MaxValue);

//////        var uidStr = uid == 0 ? "" : uid.ToString();

//////        var privileges = new Dictionary<ushort, uint>
//////        {
//////            [PrivJoinChannel] = expire,
//////            [PrivPublishAudio] = expire,
//////            [PrivPublishVideo] = expire,
//////            [PrivSubscribeAudio] = expire,
//////            [PrivSubscribeVideo] = expire,
//////        };

//////        // ── 1. Pack message body ───────────────────────────────────────────────
//////        using var msgStream = new MemoryStream();

//////        WriteUInt32(msgStream, salt);
//////        WriteUInt32(msgStream, expire);

//////        // ✅ FIX: number of services MUST come before the first service block
//////        WriteUInt16(msgStream, 1);

//////        // RTC service block
//////        WriteUInt16(msgStream, ServiceRtc);
//////        WriteString(msgStream, channelName);
//////        WriteString(msgStream, uidStr);

//////        WriteUInt16(msgStream, (ushort)privileges.Count);
//////        foreach (var (key, val) in privileges)
//////        {
//////            WriteUInt16(msgStream, key);
//////            WriteUInt32(msgStream, val);
//////        }

//////        var msgBytes = msgStream.ToArray();

//////        // ── 2. HMAC-SHA256 signature ───────────────────────────────────────────
//////        using var sigInput = new MemoryStream();
//////        sigInput.Write(Encoding.UTF8.GetBytes(appId));
//////        WriteUInt32(sigInput, expire);
//////        WriteUInt32(sigInput, salt);
//////        sigInput.Write(msgBytes);

//////        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appCertificate));
//////        var signature = hmac.ComputeHash(sigInput.ToArray());

//////        // ── 3. Assemble: len-prefixed signature + message ──────────────────────
//////        using var tokenBody = new MemoryStream();
//////        WriteBytes(tokenBody, signature);
//////        tokenBody.Write(msgBytes);

//////        // ── 4. Zlib compress → Base64 → prepend "007" ─────────────────────────
//////        var compressed = ZlibCompress(tokenBody.ToArray());
//////        return TokenVersion + Convert.ToBase64String(compressed);
//////    }

//////    private static void WriteUInt16(Stream s, ushort v)
//////    {
//////        s.WriteByte((byte)(v & 0xFF));
//////        s.WriteByte((byte)((v >> 8) & 0xFF));
//////    }

//////    private static void WriteUInt32(Stream s, uint v)
//////    {
//////        s.WriteByte((byte)(v & 0xFF));
//////        s.WriteByte((byte)((v >> 8) & 0xFF));
//////        s.WriteByte((byte)((v >> 16) & 0xFF));
//////        s.WriteByte((byte)((v >> 24) & 0xFF));
//////    }

//////    private static void WriteString(Stream s, string str)
//////    {
//////        var bytes = Encoding.UTF8.GetBytes(str);
//////        WriteUInt16(s, (ushort)bytes.Length);
//////        s.Write(bytes);
//////    }

//////    private static void WriteBytes(Stream s, byte[] data)
//////    {
//////        WriteUInt16(s, (ushort)data.Length);
//////        s.Write(data);
//////    }

//////    private static byte[] ZlibCompress(byte[] data)
//////    {
//////        using var output = new MemoryStream();
//////        output.WriteByte(0x78);
//////        output.WriteByte(0x9C);

//////        using (var deflate = new DeflateStream(output, CompressionLevel.Optimal, leaveOpen: true))
//////            deflate.Write(data, 0, data.Length);

//////        var adler = Adler32(data);
//////        output.WriteByte((byte)((adler >> 24) & 0xFF));
//////        output.WriteByte((byte)((adler >> 16) & 0xFF));
//////        output.WriteByte((byte)((adler >> 8) & 0xFF));
//////        output.WriteByte((byte)(adler & 0xFF));

//////        return output.ToArray();
//////    }

//////    private static uint Adler32(byte[] data)
//////    {
//////        const uint Mod = 65521;
//////        uint a = 1, b = 0;
//////        foreach (var bt in data)
//////        {
//////            a = (a + bt) % Mod;
//////            b = (b + a) % Mod;
//////        }
//////        return (b << 16) | a;
//////    }
//////}'

////using Microsoft.Extensions.Configuration;

////namespace Mingley.Infrastructure.Services;

/////// <summary>
/////// Testing mode — no token required (App ID only).
/////// Re-enable certificate + token generation for production.
/////// </summary>
////public class AgoraTokenService
////{
////    private readonly IConfiguration _config;

////    public AgoraTokenService(IConfiguration config) => _config = config;

////    public object GenerateToken(string channelName, uint uid, int expirationSeconds = 3600)
////    {
////        var appId = _config["Agora:AppId"]!;

////        return new
////        {
////            appId,
////            token = (string?)null,   // null = testing mode (no certificate)
////            channelName,
////            uid,
////            expiresIn = expirationSeconds
////        };
////    }
////}