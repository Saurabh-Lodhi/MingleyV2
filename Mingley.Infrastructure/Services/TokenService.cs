using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Mingley.Application.Interfaces;
using Mingley.Domain.Entities;
using Mingley.Infrastructure.Persistence;

namespace Mingley.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration  _cfg;
    private readonly MingleyDbContext _db;

    public TokenService(IConfiguration cfg, MingleyDbContext db)
    {
        _cfg = cfg;
        _db  = db;
    }

    // ── Access token ─────────────────────────────────────────────────────────
    public string GenerateAccessToken(User user)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email,          user.Email   ?? ""),
            new Claim(ClaimTypes.Role,           user.Role),
            new Claim("gender",                  user.Gender  ?? ""),
            new Claim("isPremium",               user.IsPremium.ToString().ToLower()),
            new Claim("fullName",                user.FullName ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        var token = new JwtSecurityToken(
            issuer:            _cfg["Jwt:Issuer"],
            audience:          _cfg["Jwt:Audience"],
            claims:            claims,
            expires:           DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ── Refresh token — stored in DB, survives restarts ───────────────────────
    public string GenerateRefreshToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public async Task StoreRefreshTokenAsync(Guid userId, string token)
    {
        // Expire old tokens for this user first (keep DB clean)
        var old = await _db.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync();
        foreach (var t in old) t.IsRevoked = true;

        _db.RefreshTokens.Add(new RefreshToken
        {
            Token     = token,
            UserId    = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsRevoked = false,
        });
        await _db.SaveChangesAsync();
    }

    public async Task<Guid?> ValidateRefreshTokenAsync(string token)
    {
        var rt = await _db.RefreshTokens
            .FirstOrDefaultAsync(t =>
                t.Token     == token &&
                !t.IsRevoked &&
                t.ExpiresAt  > DateTime.UtcNow);

        return rt?.UserId;
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        var rt = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
        if (rt is null) return;
        rt.IsRevoked = true;
        await _db.SaveChangesAsync();
    }

    // ── Sync shims (kept so existing callers don't break during transition) ───
    public Guid? ValidateRefreshToken(string token)
        => ValidateRefreshTokenAsync(token).GetAwaiter().GetResult();

    public void StoreRefreshToken(Guid uid, string token)
        => StoreRefreshTokenAsync(uid, token).GetAwaiter().GetResult();

    public void RevokeRefreshToken(string token)
        => RevokeRefreshTokenAsync(token).GetAwaiter().GetResult();
}
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Security.Cryptography;
//using System.Text;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;
//using Mingley.Application.Interfaces;
//using Mingley.Domain.Entities;

//namespace Mingley.Infrastructure.Services;

//public class TokenService : ITokenService
//{
//    private readonly IConfiguration _cfg;
//    private static readonly Dictionary<string, Guid> _store = new();
//    private static readonly object _lock = new();

//    public TokenService(IConfiguration cfg) => _cfg = cfg;

//    public string GenerateAccessToken(User user)
//    {
//        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Secret"]!));
//        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
//        var claims = new[]
//        {
//            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
//            new Claim(ClaimTypes.Email,          user.Email ?? ""),
//            new Claim(ClaimTypes.Role,           user.Role),
//            new Claim("gender",                  user.Gender ?? ""),
//            new Claim("isPremium",               user.IsPremium.ToString().ToLower()),
//            new Claim("fullName",                user.FullName ?? ""),
//            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//        };
//        var token = new JwtSecurityToken(
//            issuer: _cfg["Jwt:Issuer"], audience: _cfg["Jwt:Audience"],
//            claims: claims, expires: DateTime.UtcNow.AddDays(7),
//            signingCredentials: creds);
//        return new JwtSecurityTokenHandler().WriteToken(token);
//    }

//    public string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

//    public Guid? ValidateRefreshToken(string token)
//    { lock (_lock) { return _store.TryGetValue(token, out var uid) ? uid : null; } }

//    public void StoreRefreshToken(Guid uid, string token)
//    { lock (_lock) { _store[token] = uid; } }

//    public void RevokeRefreshToken(string token)
//    { lock (_lock) { _store.Remove(token); } }
//}
