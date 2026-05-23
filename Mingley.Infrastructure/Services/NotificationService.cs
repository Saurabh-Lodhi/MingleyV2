using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Mingley.Application.Interfaces;
using Mingley.Domain.Entities;
using Mingley.Infrastructure.Persistence;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Mingley.Infrastructure.Services;

/// <summary>
/// Handles in-app notifications (DB + SignalR) AND device push notifications (FCM).
///
/// Push notifications appear in the phone notification bar like Zomato/Swiggy.
/// Requires:
///   appsettings / Render env vars:
///     Firebase__ProjectId         = your-firebase-project-id
///     Firebase__ServiceAccountKey = { full JSON content of serviceAccountKey.json }
///
/// How to get these:
///   Firebase Console → Project Settings → Service Accounts → Generate New Private Key
///   Copy the entire JSON content into Firebase__ServiceAccountKey env var on Render.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly MingleyDbContext _db;
    private readonly IHubNotifier _hub;
    private readonly IConfiguration _config;

    public NotificationService(MingleyDbContext db, IHubNotifier hub, IConfiguration config)
    {
        _db = db;
        _hub = hub;
        _config = config;
    }

    // ── Create in-app notification + send device push ─────────────────
    public async Task CreateAsync(Guid userId, string title, string body,
                                  string type, string? referenceId = null)
    {
        // 1. Save to DB
        var n = new Notification
        {
            UserId = userId,
            Title = title,
            Body = body,
            Type = type,
            ReferenceId = referenceId,
        };
        _db.Notifications.Add(n);
        await _db.SaveChangesAsync();

        // 2. Real-time push via SignalR (works when app is open)
        await _hub.SendToUserAsync(userId.ToString(), "NewNotification", new
        {
            id = n.Id.ToString(),
            title,
            body,
            type,
            referenceId,
            isRead = false,
            createdAt = n.CreatedAt,
        });

        // 3. FCM device push (works when app is in background / killed)
        //    Fires-and-forget — never let push failure break the main flow
        _ = Task.Run(() => SendFcmAsync(userId, title, body, type, referenceId));
    }

    // ── Get paginated notifications ───────────────────────────────────
    public async Task<List<object>> GetAllAsync(Guid userId, int page)
    {
        const int ps = 30;
        return await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * ps).Take(ps)
            .Select(n => (object)new
            {
                id = n.Id.ToString(),
                n.Title,
                n.Body,
                n.Type,
                n.IsRead,
                n.CreatedAt,
                n.ReferenceId,
            })
            .ToListAsync();
    }

    // ── Mark single notification read ─────────────────────────────────
    public async Task MarkReadAsync(Guid notificationId, Guid userId)
    {
        var n = await _db.Notifications
            .FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId);
        if (n != null)
        {
            n.IsRead = true;
            await _db.SaveChangesAsync();
        }
    }

    // ── Mark all notifications read ───────────────────────────────────
    public async Task MarkAllReadAsync(Guid userId)
    {
        var list = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();
        list.ForEach(n => n.IsRead = true);
        await _db.SaveChangesAsync();
    }

    // ── FCM push via HTTP v1 API ──────────────────────────────────────
    private async Task SendFcmAsync(Guid userId, string title, string body,
                                    string type, string? referenceId)
    {
        try
        {
            // Get FCM token saved on login / app open
            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (string.IsNullOrWhiteSpace(user?.FcmToken))
                return; // user has no device token — skip silently

            var projectId = _config["Firebase:ProjectId"];
            var saKeyJson = _config["Firebase:ServiceAccountKey"];

            if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(saKeyJson))
                return; // Firebase not configured — skip silently

            // Get OAuth2 bearer token for FCM HTTP v1
            var bearerToken = await GetFcmBearerTokenAsync(saKeyJson);
            if (string.IsNullOrWhiteSpace(bearerToken)) return;

            // Build FCM message payload
            var payload = JsonSerializer.Serialize(new
            {
                message = new
                {
                    token = user.FcmToken,
                    notification = new
                    {
                        title,
                        body,
                    },
                    data = new Dictionary<string, string>
                    {
                        ["type"] = type,
                        ["referenceId"] = referenceId ?? "",
                        ["click_action"] = "FLUTTER_NOTIFICATION_CLICK",
                    },
                    android = new
                    {
                        priority = "high",
                        notification = new
                        {
                            sound = "default",
                            click_action = "FLUTTER_NOTIFICATION_CLICK",
                            channel_id = "mingley_default",
                        },
                    },
                    apns = new                               // iOS
                    {
                        payload = new
                        {
                            aps = new { sound = "default", badge = 1 }
                        }
                    },
                }
            });

            using var client = new HttpClient();
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://fcm.googleapis.com/v1/projects/{projectId}/messages:send");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);

            // Token expired / unregistered → clear it from DB
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errBody = await response.Content.ReadAsStringAsync();
                if (errBody.Contains("UNREGISTERED") || errBody.Contains("INVALID_ARGUMENT"))
                {
                    var dbUser = await _db.Users.FindAsync(userId);
                    if (dbUser != null)
                    {
                        dbUser.FcmToken = null;
                        dbUser.UpdatedAt = DateTime.UtcNow;
                        await _db.SaveChangesAsync();
                    }
                }
            }
        }
        catch
        {
            // Never throw — push failure must not break in-app flow
        }
    }

    // ── Get short-lived OAuth2 token for FCM HTTP v1 ──────────────────
    // Uses JWT + Google OAuth2 token endpoint — no Firebase Admin SDK needed.
    private static async Task<string?> GetFcmBearerTokenAsync(string serviceAccountJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(serviceAccountJson);
            var root = doc.RootElement;

            var clientEmail = root.GetProperty("client_email").GetString()!;
            var privateKey = root.GetProperty("private_key").GetString()!
                .Replace("-----BEGIN PRIVATE KEY-----", "")
                .Replace("-----END PRIVATE KEY-----", "")
                .Replace("\n", "")
                .Trim();

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var scope = "https://www.googleapis.com/auth/firebase.messaging";

            // Build JWT header + claims
            var header = Base64UrlEncode(JsonSerializer.Serialize(new
            { alg = "RS256", typ = "JWT" }));

            var claims = Base64UrlEncode(JsonSerializer.Serialize(new
            {
                iss = clientEmail,
                sub = clientEmail,
                aud = "https://oauth2.googleapis.com/token",
                iat = now,
                exp = now + 3600,
                scope,
            }));

            var signingInput = $"{header}.{claims}";

            // Sign with RSA-SHA256
            using var rsa = System.Security.Cryptography.RSA.Create();
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);
            var signature = rsa.SignData(
                Encoding.UTF8.GetBytes(signingInput),
                System.Security.Cryptography.HashAlgorithmName.SHA256,
                System.Security.Cryptography.RSASignaturePadding.Pkcs1);

            var jwt = $"{signingInput}.{Base64UrlEncode(signature)}";

            // Exchange JWT for access token
            using var http = new HttpClient();
            var tokenResp = await http.PostAsync(
                "https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                    ["assertion"] = jwt,
                }));

            var tokenJson = await tokenResp.Content.ReadAsStringAsync();
            using var tDoc = JsonDocument.Parse(tokenJson);
            return tDoc.RootElement.GetProperty("access_token").GetString();
        }
        catch
        {
            return null;
        }
    }

    private static string Base64UrlEncode(string input)
        => Base64UrlEncode(Encoding.UTF8.GetBytes(input));

    private static string Base64UrlEncode(byte[] input)
        => Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}

//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Mingley.Application.Interfaces;
//using Mingley.Domain.Entities;
//using Mingley.Infrastructure.Persistence;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json;

//namespace Mingley.Infrastructure.Services;

///// <summary>
///// Handles in-app notifications (DB + SignalR) AND device push notifications (FCM).
/////
///// Push notifications appear in the phone notification bar like Zomato/Swiggy.
///// Requires:
/////   appsettings / Render env vars:
/////     Firebase__ProjectId         = your-firebase-project-id
/////     Firebase__ServiceAccountKey = { full JSON content of serviceAccountKey.json }
/////
///// How to get these:
/////   Firebase Console → Project Settings → Service Accounts → Generate New Private Key
/////   Copy the entire JSON content into Firebase__ServiceAccountKey env var on Render.
///// </summary>
//public class NotificationService : INotificationService
//{
//    private readonly MingleyDbContext _db;
//    private readonly IHubNotifier _hub;
//    private readonly IConfiguration _config;

//    public NotificationService(MingleyDbContext db, IHubNotifier hub, IConfiguration config)
//    {
//        _db = db;
//        _hub = hub;
//        _config = config;
//    }

//    // ── Create in-app notification + send device push ─────────────────
//    public async Task CreateAsync(Guid userId, string title, string body,
//                                  string type, string? referenceId = null)
//    {
//        // 1. Save to DB
//        var n = new Notification
//        {
//            UserId = userId,
//            Title = title,
//            Body = body,
//            Type = type,
//            ReferenceId = referenceId,
//        };
//        _db.Notifications.Add(n);
//        await _db.SaveChangesAsync();

//        // 2. Real-time push via SignalR (works when app is open)
//        await _hub.SendToUserAsync(userId.ToString(), "NewNotification", new
//        {
//            id = n.Id.ToString(),
//            title,
//            body,
//            type,
//            referenceId,
//            isRead = false,
//            createdAt = n.CreatedAt,
//        });

//        // 3. FCM device push (works when app is in background / killed)
//        //    Fires-and-forget — never let push failure break the main flow
//        _ = Task.Run(() => SendFcmAsync(userId, title, body, type, referenceId));
//    }

//    // ── Get paginated notifications ───────────────────────────────────
//    public async Task<List<object>> GetAllAsync(Guid userId, int page)
//    {
//        const int ps = 30;
//        return await _db.Notifications
//            .Where(n => n.UserId == userId && !n.IsDeleted)
//            .OrderByDescending(n => n.CreatedAt)
//            .Skip((page - 1) * ps).Take(ps)
//            .Select(n => (object)new
//            {
//                id = n.Id.ToString(),
//                n.Title,
//                n.Body,
//                n.Type,
//                n.IsRead,
//                n.CreatedAt,
//                n.ReferenceId,
//            })
//            .ToListAsync();
//    }

//    // ── Mark single notification read ─────────────────────────────────
//    public async Task MarkReadAsync(Guid notificationId, Guid userId)
//    {
//        var n = await _db.Notifications
//            .FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId);
//        if (n != null)
//        {
//            n.IsRead = true;
//            await _db.SaveChangesAsync();
//        }
//    }

//    // ── Mark all notifications read ───────────────────────────────────
//    public async Task MarkAllReadAsync(Guid userId)
//    {
//        var list = await _db.Notifications
//            .Where(n => n.UserId == userId && !n.IsRead)
//            .ToListAsync();
//        list.ForEach(n => n.IsRead = true);
//        await _db.SaveChangesAsync();
//    }

//    // ── FCM push via HTTP v1 API ──────────────────────────────────────
//    private async Task SendFcmAsync(Guid userId, string title, string body,
//                                    string type, string? referenceId)
//    {
//        try
//        {
//            // Get FCM token saved on login / app open
//            var user = await _db.Users
//                .AsNoTracking()
//                .FirstOrDefaultAsync(u => u.Id == userId);

//            if (string.IsNullOrWhiteSpace(user?.FcmToken))
//                return; // user has no device token — skip silently

//            var projectId = _config["Firebase:ProjectId"];
//            var saKeyJson = _config["Firebase:ServiceAccountKey"];

//            if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(saKeyJson))
//                return; // Firebase not configured — skip silently

//            // Get OAuth2 bearer token for FCM HTTP v1
//            var bearerToken = await GetFcmBearerTokenAsync(saKeyJson);
//            if (string.IsNullOrWhiteSpace(bearerToken)) return;

//            // Build FCM message payload
//            var payload = JsonSerializer.Serialize(new
//            {
//                message = new
//                {
//                    token = user.FcmToken,
//                    notification = new
//                    {
//                        title,
//                        body,
//                    },
//                    data = new Dictionary<string, string>
//                    {
//                        ["type"] = type,
//                        ["referenceId"] = referenceId ?? "",
//                        ["click_action"] = "FLUTTER_NOTIFICATION_CLICK",
//                    },
//                    android = new
//                    {
//                        priority = "high",
//                        notification = new
//                        {
//                            sound = "default",
//                            click_action = "FLUTTER_NOTIFICATION_CLICK",
//                            channel_id = "mingley_default",
//                        },
//                    },
//                    apns = new                               // iOS
//                    {
//                        payload = new
//                        {
//                            aps = new { sound = "default", badge = 1 }
//                        }
//                    },
//                }
//            });

//            using var client = new HttpClient();
//            var request = new HttpRequestMessage(
//                HttpMethod.Post,
//                $"https://fcm.googleapis.com/v1/projects/{projectId}/messages:send");

//            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
//            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

//            var response = await client.SendAsync(request);

//            // Token expired / unregistered → clear it from DB
//            if (response.StatusCode == System.Net.HttpStatusCode.NotFound ||
//                response.StatusCode == System.Net.HttpStatusCode.BadRequest)
//            {
//                var errBody = await response.Content.ReadAsStringAsync();
//                if (errBody.Contains("UNREGISTERED") || errBody.Contains("INVALID_ARGUMENT"))
//                {
//                    var dbUser = await _db.Users.FindAsync(userId);
//                    if (dbUser != null)
//                    {
//                        dbUser.FcmToken = null;
//                        dbUser.UpdatedAt = DateTime.UtcNow;
//                        await _db.SaveChangesAsync();
//                    }
//                }
//            }
//        }
//        catch
//        {
//            // Never throw — push failure must not break in-app flow
//        }
//    }

//    // ── Get short-lived OAuth2 token for FCM HTTP v1 ──────────────────
//    // Uses JWT + Google OAuth2 token endpoint — no Firebase Admin SDK needed.
//    private static async Task<string?> GetFcmBearerTokenAsync(string serviceAccountJson)
//    {
//        try
//        {
//            using var doc = JsonDocument.Parse(serviceAccountJson);
//            var root = doc.RootElement;

//            var clientEmail = root.GetProperty("client_email").GetString()!;
//            var privateKey = root.GetProperty("private_key").GetString()!
//                .Replace("-----BEGIN PRIVATE KEY-----", "")
//                .Replace("-----END PRIVATE KEY-----", "")
//                .Replace("\n", "")
//                .Trim();

//            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
//            var scope = "https://www.googleapis.com/auth/firebase.messaging";

//            // Build JWT header + claims
//            var header = Base64UrlEncode(JsonSerializer.Serialize(new
//            { alg = "RS256", typ = "JWT" }));

//            var claims = Base64UrlEncode(JsonSerializer.Serialize(new
//            {
//                iss = clientEmail,
//                sub = clientEmail,
//                aud = "https://oauth2.googleapis.com/token",
//                iat = now,
//                exp = now + 3600,
//                scope,
//            }));

//            var signingInput = $"{header}.{claims}";

//            // Sign with RSA-SHA256
//            using var rsa = System.Security.Cryptography.RSA.Create();
//            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);
//            var signature = rsa.SignData(
//                Encoding.UTF8.GetBytes(signingInput),
//                System.Security.Cryptography.HashAlgorithmName.SHA256,
//                System.Security.Cryptography.RSASignaturePadding.Pkcs1);

//            var jwt = $"{signingInput}.{Base64UrlEncode(signature)}";

//            // Exchange JWT for access token
//            using var http = new HttpClient();
//            var tokenResp = await http.PostAsync(
//                "https://oauth2.googleapis.com/token",
//                new FormUrlEncodedContent(new Dictionary<string, string>
//                {
//                    ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
//                    ["assertion"] = jwt,
//                }));

//            var tokenJson = await tokenResp.Content.ReadAsStringAsync();
//            using var tDoc = JsonDocument.Parse(tokenJson);
//            return tDoc.RootElement.GetProperty("access_token").GetString();
//        }
//        catch
//        {
//            return null;
//        }
//    }

//    private static string Base64UrlEncode(string input)
//        => Base64UrlEncode(Encoding.UTF8.GetBytes(input));

//    private static string Base64UrlEncode(byte[] input)
//        => Convert.ToBase64String(input)
//            .TrimEnd('=')
//            .Replace('+', '-')
//            .Replace('/', '_');
//}

////using Microsoft.EntityFrameworkCore;
////using Mingley.Application.Interfaces;
////using Mingley.Domain.Entities;
////using Mingley.Infrastructure.Persistence;

////namespace Mingley.Infrastructure.Services;

////public class NotificationService : INotificationService
////{
////    private readonly MingleyDbContext _db;
////    private readonly IHubNotifier _hub;

////    public NotificationService(MingleyDbContext db, IHubNotifier hub) { _db = db; _hub = hub; }

////    public async Task CreateAsync(Guid userId, string title, string body, string type, string? referenceId = null)
////    {
////        var n = new Notification { UserId = userId, Title = title, Body = body, Type = type, ReferenceId = referenceId };
////        _db.Notifications.Add(n);
////        await _db.SaveChangesAsync();
////        // Push real-time
////        await _hub.SendToUserAsync(userId.ToString(), "NewNotification", new
////        {
////            id = n.Id.ToString(), title, body, type, referenceId, isRead = false, createdAt = n.CreatedAt,
////        });
////    }

////    public async Task<List<object>> GetAllAsync(Guid userId, int page)
////    {
////        const int ps = 30;
////        var list = await _db.Notifications
////            .Where(n => n.UserId == userId && !n.IsDeleted)
////            .OrderByDescending(n => n.CreatedAt)
////            .Skip((page - 1) * ps).Take(ps)
////            .Select(n => (object)new { id = n.Id.ToString(), n.Title, n.Body, n.Type, n.IsRead, n.CreatedAt, n.ReferenceId })
////            .ToListAsync();
////        return list;
////    }

////    public async Task MarkReadAsync(Guid notificationId, Guid userId)
////    {
////        var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId);
////        if (n != null) { n.IsRead = true; await _db.SaveChangesAsync(); }
////    }

////    public async Task MarkAllReadAsync(Guid userId)
////    {
////        var list = await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
////        list.ForEach(n => n.IsRead = true);
////        await _db.SaveChangesAsync();
////    }
////}
