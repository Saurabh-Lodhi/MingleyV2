using Microsoft.EntityFrameworkCore;
using Mingley.Application.DTOs.Users;
using Mingley.Application.Interfaces;
using Mingley.Domain.Entities;
using Mingley.Infrastructure.Persistence;

namespace Mingley.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly MingleyDbContext _db;
    private readonly IHubNotifier _hub;
    private readonly IWalletService _wallet;

    public UserService(MingleyDbContext db, IHubNotifier hub, IWalletService wallet)
    { _db = db; _hub = hub; _wallet = wallet; }

    public async Task<UserProfileDto?> GetMeAsync(Guid userId)
    {
        var user = await _db.Users
            .Include(u => u.Location)
            .Include(u => u.Preference)
            .Include(u => u.Images.Where(i => !i.IsDeleted))
            .Include(u => u.Interests).ThenInclude(i => i.Interest)
            .Include(u => u.Subscription).ThenInclude(s => s!.Plan)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        return user == null ? null : MapProfile(user);
    }

    public async Task<UserProfileDto?> GetUserAsync(Guid userId, Guid requesterId)
    {
        if (userId == requesterId) return await GetMeAsync(userId);

        var blocked = await _db.Blocks.AnyAsync(b =>
            (b.BlockerId == requesterId && b.BlockedUserId == userId) ||
            (b.BlockerId == userId && b.BlockedUserId == requesterId));
        if (blocked) return null;

        var user = await _db.Users
            .Include(u => u.Location)
            .Include(u => u.Images.Where(i => !i.IsDeleted))
            .Include(u => u.Interests).ThenInclude(i => i.Interest)
            .Include(u => u.Subscription).ThenInclude(s => s!.Plan)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted && u.IsActive);
        return user == null ? null : MapProfile(user);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest req)
    {
        var user = await _db.Users.FindAsync(userId) ?? throw new InvalidOperationException("User not found.");
        if (req.FullName != null) user.FullName = req.FullName.Trim();
        if (req.Bio != null) user.Bio = req.Bio.Trim();
        if (req.Gender != null) user.Gender = req.Gender.ToLower().Trim();
        if (req.DateOfBirth.HasValue) user.DateOfBirth = req.DateOfBirth.Value.ToUniversalTime();
        if (req.Avatar != null) user.Avatar = req.Avatar;
        if (req.CoverPhoto != null) user.CoverPhoto = req.CoverPhoto;
        if (req.Profession != null) user.Profession = req.Profession.Trim();
        user.ProfileComplete = !string.IsNullOrWhiteSpace(user.FullName)
                            && user.DateOfBirth.HasValue
                            && !string.IsNullOrWhiteSpace(user.Gender);
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return (await GetMeAsync(userId))!;
    }

    public async Task UpdateInterestsAsync(Guid userId, List<string> interests)
    {
        var old = await _db.UserInterests.Where(ui => ui.UserId == userId).ToListAsync();
        _db.UserInterests.RemoveRange(old);
        foreach (var name in interests.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var interest = await _db.Interests.FirstOrDefaultAsync(i => i.Name.ToLower() == name.ToLower());
            if (interest == null)
            {
                interest = new Interest { Name = name };
                _db.Interests.Add(interest);
                await _db.SaveChangesAsync();
            }
            _db.UserInterests.Add(new UserInterest { UserId = userId, InterestId = interest.Id });
        }
        await _db.SaveChangesAsync();
    }

    public async Task UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest req)
    {
        var pref = await _db.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
        if (pref == null) { pref = new UserPreference { UserId = userId }; _db.UserPreferences.Add(pref); }
        if (req.InterestedIn != null) pref.InterestedIn = req.InterestedIn;
        if (req.MinAge.HasValue) pref.MinAge = req.MinAge.Value;
        if (req.MaxAge.HasValue) pref.MaxAge = req.MaxAge.Value;
        if (req.MaxDistance.HasValue) pref.MaxDistance = req.MaxDistance.Value;
        if (req.RelationshipType != null) pref.RelationshipType = req.RelationshipType;
        if (req.NearbyOnly.HasValue) pref.NearbyOnly = req.NearbyOnly.Value;
        if (req.OnlineOnly.HasValue) pref.OnlineOnly = req.OnlineOnly.Value;
        if (req.VerifiedOnly.HasValue) pref.VerifiedOnly = req.VerifiedOnly.Value;
        if (req.Location != null) pref.Location = req.Location;
        pref.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    //public async Task UpdateLocationAsync(Guid userId, UpdateLocationRequest req)
    //{
    //    var user = await _db.Users.FindAsync(userId);
    //    if (user != null && user.IsLocationLocked)
    //        throw new InvalidOperationException("Location is locked. Disable location lock in settings to update manually.");

    //    var loc = await _db.UserLocations.FirstOrDefaultAsync(l => l.UserId == userId);
    //    if (loc == null) { loc = new UserLocation { UserId = userId }; _db.UserLocations.Add(loc); }
    //    if (req.Lat.HasValue) loc.Lat = req.Lat;
    //    if (req.Lng.HasValue) loc.Lng = req.Lng;
    //    if (req.City != null) loc.City = req.City;
    //    if (req.Country != null) loc.Country = req.Country;
    //    loc.UpdatedAt = DateTime.UtcNow;
    //    await _db.SaveChangesAsync();
    //}


    public async Task UpdateLocationAsync(Guid userId, UpdateLocationRequest req)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user != null && user.IsLocationLocked)
            throw new InvalidOperationException("Location is locked. Disable location lock in settings to update manually.");

        var loc = await _db.UserLocations.FirstOrDefaultAsync(l => l.UserId == userId);
        if (loc == null) { loc = new UserLocation { UserId = userId }; _db.UserLocations.Add(loc); }

        if (req.Lat.HasValue) loc.Lat = req.Lat;
        if (req.Lng.HasValue) loc.Lng = req.Lng;

        // Reverse geocode if city/country are missing or "Unknown"
        if (req.Lat.HasValue && req.Lng.HasValue &&
            (string.IsNullOrWhiteSpace(req.City) || req.City == "Unknown" ||
             string.IsNullOrWhiteSpace(req.Country) || req.Country == "Unknown"))
        {
            try
            {
                using var http = new System.Net.Http.HttpClient();
                http.DefaultRequestHeaders.Add("User-Agent", "MingleyApp/1.0");
                var url = $"https://nominatim.openstreetmap.org/reverse?lat={req.Lat}&lon={req.Lng}&format=json&addressdetails=1";
                var response = await http.GetStringAsync(url);
                var json = System.Text.Json.JsonDocument.Parse(response);
                var address = json.RootElement.GetProperty("address");

                // Try city → town → village → county → state_district
                string? city = null;
                foreach (var key in new[] { "city", "town", "village", "suburb", "county", "state_district", "state" })
                {
                    if (address.TryGetProperty(key, out var val))
                    {
                        city = val.GetString();
                        break;
                    }
                }

                string? country = null;
                if (address.TryGetProperty("country", out var countryVal))
                    country = countryVal.GetString();

                if (!string.IsNullOrWhiteSpace(city)) loc.City = city;
                if (!string.IsNullOrWhiteSpace(country)) loc.Country = country;
            }
            catch
            {
                // Fall back to whatever was sent
                if (req.City != null) loc.City = req.City;
                if (req.Country != null) loc.Country = req.Country;
            }
        }
        else
        {
            if (req.City != null) loc.City = req.City;
            if (req.Country != null) loc.Country = req.Country;
        }

        loc.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task SetTravelModeAsync(Guid userId, SetTravelModeRequest req)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        user.IsTravelMode = req.Enabled;

        if (req.Enabled)
        {
            // Store travel destination coordinates
            if (req.Lat.HasValue) user.TravelLat = req.Lat;
            if (req.Lng.HasValue) user.TravelLng = req.Lng;
            if (req.City != null) user.TravelCity = req.City;
        }
        else
        {
            // Clear travel data when disabling
            user.TravelLat = null;
            user.TravelLng = null;
            user.TravelCity = null;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
    public async Task UpdateFcmTokenAsync(Guid userId, string token)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return;
        user.FcmToken = token;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
    public async Task UpdateCoverPhotoAsync(Guid userId, string coverPhotoUrl)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found.");
        user.CoverPhoto = coverPhotoUrl;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<ImageDto> AddImageAsync(Guid userId, AddImageRequest req)
    {
        var count = await _db.UserImages.CountAsync(i => i.UserId == userId && !i.IsDeleted);
        if (count >= 9) throw new InvalidOperationException("Maximum 9 photos allowed.");
        var img = new UserImage { UserId = userId, Url = req.Url, SortOrder = count, IsPrimary = req.IsPrimary || count == 0 };
        if (req.IsPrimary)
        {
            var old = await _db.UserImages.Where(i => i.UserId == userId && i.IsPrimary && !i.IsDeleted).ToListAsync();
            old.ForEach(i => i.IsPrimary = false);
        }
        _db.UserImages.Add(img);
        await _db.SaveChangesAsync();
        return new ImageDto { Id = img.Id.ToString(), Url = img.Url, SortOrder = img.SortOrder, IsPrimary = img.IsPrimary };
    }

    public async Task DeleteImageAsync(Guid userId, Guid imageId)
    {
        var img = await _db.UserImages.FirstOrDefaultAsync(i => i.Id == imageId && i.UserId == userId)
            ?? throw new InvalidOperationException("Image not found.");
        img.IsDeleted = true; img.DeletedAt = DateTime.UtcNow;
        if (img.IsPrimary)
        {
            var next = await _db.UserImages.Where(i => i.UserId == userId && !i.IsDeleted && i.Id != imageId).FirstOrDefaultAsync();
            if (next != null) next.IsPrimary = true;
        }
        await _db.SaveChangesAsync();
    }

    public async Task ReorderImagesAsync(Guid userId, ReorderImagesRequest req)
    {
        foreach (var item in req.Order)
        {
            if (!Guid.TryParse(item.ImageId, out var imgId)) continue;
            var img = await _db.UserImages.FirstOrDefaultAsync(i => i.Id == imgId && i.UserId == userId);
            if (img != null) img.SortOrder = item.SortOrder;
        }
        await _db.SaveChangesAsync();
    }

    public async Task SetPrimaryImageAsync(Guid userId, Guid imageId)
    {
        var all = await _db.UserImages.Where(i => i.UserId == userId && !i.IsDeleted).ToListAsync();
        all.ForEach(i => i.IsPrimary = i.Id == imageId);
        var target = all.FirstOrDefault(i => i.Id == imageId);
        if (target == null) throw new InvalidOperationException("Image not found.");
        var user = await _db.Users.FindAsync(userId);
        if (user != null) { user.Avatar = target.Url; user.UpdatedAt = DateTime.UtcNow; }
        await _db.SaveChangesAsync();
    }

    public async Task BlockUserAsync(Guid blockerId, Guid targetId)
    {
        if (await _db.Blocks.AnyAsync(b => b.BlockerId == blockerId && b.BlockedUserId == targetId)) return;
        _db.Blocks.Add(new Block { BlockerId = blockerId, BlockedUserId = targetId });
        await _db.SaveChangesAsync();
        var match = await _db.Matches.FirstOrDefaultAsync(m =>
            (m.User1Id == blockerId && m.User2Id == targetId) ||
            (m.User1Id == targetId && m.User2Id == blockerId));
        if (match != null) { match.IsActive = false; match.IsDeleted = true; match.DeletedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
    }

    public async Task UnblockUserAsync(Guid blockerId, Guid targetId)
    {
        var b = await _db.Blocks.FirstOrDefaultAsync(b => b.BlockerId == blockerId && b.BlockedUserId == targetId);
        if (b != null) { _db.Blocks.Remove(b); await _db.SaveChangesAsync(); }
    }

    public async Task<List<UserProfileDto>> GetBlockedUsersAsync(Guid userId)
    {
        var users = await _db.Blocks
            .Include(b => b.BlockedUser).ThenInclude(u => u!.Location)
            .Where(b => b.BlockerId == userId)
            .Select(b => b.BlockedUser!)
            .ToListAsync();
        return users.Select(u => MapProfile(u)).ToList();
    }

    public async Task ReportUserAsync(Guid reporterId, string targetId, string reason, string? description)
    {
        if (!Guid.TryParse(targetId, out var tid)) throw new InvalidOperationException("Invalid target ID.");
        _db.Reports.Add(new Report { ReporterId = reporterId, ReportedUserId = tid, Reason = reason, Description = description });
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAccountAsync(Guid userId, DeleteAccountRequest req)
    {
        var user = await _db.Users.FindAsync(userId) ?? throw new InvalidOperationException("User not found.");
        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash!))
            throw new InvalidOperationException("Incorrect password.");
        user.IsDeleted = true;
        user.IsActive = false;
        user.DeletedAt = DateTime.UtcNow;
        user.Email = $"deleted_{user.Id}@mingley.app";
        user.Phone = null;
        user.PasswordHash = null;
        user.FullName = "Deleted User";
        user.Avatar = null;
        user.OtpCode = null;
        user.FcmToken = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task SetOnlineStatusAsync(Guid userId, bool isOnline)
    {
        var u = await _db.Users.FindAsync(userId);
        if (u == null) return;
        u.IsOnline = isOnline;
        u.LastActiveAt = DateTime.UtcNow;
        u.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        var matchedIds = await _db.Matches
            .Where(m => m.IsActive && !m.IsDeleted && (m.User1Id == userId || m.User2Id == userId))
            .Select(m => m.User1Id == userId ? m.User2Id.ToString() : m.User1Id.ToString())
            .ToListAsync();
        foreach (var id in matchedIds)
            await _hub.SendToUserAsync(id, "UserOnlineStatus", new { userId = userId.ToString(), isOnline, lastSeen = DateTime.UtcNow });
    }

    public async Task<List<ContactOnAppDto>> GetContactsOnAppAsync(List<string> phoneNumbers)
    {
        if (!phoneNumbers.Any()) return new List<ContactOnAppDto>();
        var normalized = phoneNumbers.Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToList();
        return await _db.Users
            .Where(u => !u.IsDeleted && u.IsActive && u.Phone != null && normalized.Contains(u.Phone))
            .Select(u => new ContactOnAppDto
            {
                UserId = u.Id.ToString(),
                FullName = u.FullName,
                Avatar = u.Avatar,
                Phone = u.Phone,
                IsOnline = u.IsOnline,
                IsVerified = u.IsVerified,
            })
            .ToListAsync();
    }

    public static UserProfileDto MapProfile(User u) => new()
    {
        Id = u.Id.ToString(),
        FullName = u.FullName,
        Email = u.Email,
        Phone = u.Phone,
        Gender = u.Gender,
        Avatar = u.Avatar,
        Bio = u.Bio,
        IsVerified = u.IsVerified,
        IsActive = u.IsActive,
        IsPremium = u.IsPremium,
        IsOnline = u.IsOnline,
        ProfileComplete = u.ProfileComplete,
        CoinBalance = u.CoinBalance,
        TotalEarned = u.TotalEarned,
        Role = u.Role,
        TwoFactorEnabled = u.TwoFactorEnabled,
        LastActiveAt = u.LastActiveAt,
        Age = u.DateOfBirth.HasValue ? (int?)((DateTime.UtcNow - u.DateOfBirth.Value).Days / 365) : null,
        DateOfBirth = u.DateOfBirth,
        CoverPhoto = u.CoverPhoto,
        SuperlikesRemaining = u.CoinBalance / Mingley.Infrastructure.Persistence.MingleyDbContext.SuperLikeCost,
        Interests = u.Interests.Select(i => i.Interest?.Name ?? "").Where(n => n != "").ToList(),
        Images = u.Images.Where(i => !i.IsDeleted).OrderBy(i => i.SortOrder).Select(i => new ImageDto
        {
            Id = i.Id.ToString(),
            Url = i.Url,
            SortOrder = i.SortOrder,
            IsPrimary = i.IsPrimary
        }).ToList(),
        Location = u.Location == null ? null : new LocationDto
        {
            Lat = u.Location.Lat,
            Lng = u.Location.Lng,
            City = u.Location.City,
            Country = u.Location.Country
        },
        Preference = u.Preference == null ? null : new PreferenceDto
        {
            InterestedIn = u.Preference.InterestedIn,
            MinAge = u.Preference.MinAge,
            MaxAge = u.Preference.MaxAge,
            MaxDistance = u.Preference.MaxDistance,
            RelationshipType = u.Preference.RelationshipType,
            NearbyOnly = u.Preference.NearbyOnly,
            OnlineOnly = u.Preference.OnlineOnly,
            VerifiedOnly = u.Preference.VerifiedOnly,
            Location = u.Preference.Location
        },
        Subscription = (u.Subscription != null && u.Subscription.IsActive && u.Subscription.EndDate > DateTime.UtcNow)
            ? new SubscriptionInfoDto
            {
                Id = u.Subscription.Id.ToString(),
                PlanName = u.Subscription.Plan?.Name,
                StartDate = u.Subscription.StartDate,
                EndDate = u.Subscription.EndDate,
                IsActive = true,
                AutoRenew = u.Subscription.AutoRenew,
                DaysRemaining = (int)(u.Subscription.EndDate - DateTime.UtcNow).TotalDays
            }
            : null,
    };
}