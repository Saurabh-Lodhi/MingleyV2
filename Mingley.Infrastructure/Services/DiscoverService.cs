using Microsoft.EntityFrameworkCore;
using Mingley.Application.DTOs.Discover;
using Mingley.Application.DTOs.Users;
using Mingley.Application.Interfaces;
using Mingley.Domain.Entities;
using Mingley.Infrastructure.Persistence;

namespace Mingley.Infrastructure.Services;

public class DiscoverService : IDiscoverService
{
    private readonly MingleyDbContext _db;
    private readonly INotificationService _notifs;
    private readonly IHubNotifier _hub;
    private readonly IWalletService _wallet;

    private const int DefaultRadiusKm = 100;
    private const int MaxFreeRadiusKm = 100;

    public DiscoverService(MingleyDbContext db, INotificationService notifs, IHubNotifier hub, IWalletService wallet)
    { _db = db; _notifs = notifs; _hub = hub; _wallet = wallet; }

    public async Task<(List<DiscoverUserDto> Users, PaginationDto Pagination)> GetFeedAsync(
        Guid userId, int page, int limit, DiscoverFilters? filters = null)
    {
        var me = await _db.Users
            .Include(u => u.Preference)
            .Include(u => u.Location)
            .Include(u => u.Interests)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        if (me == null) return (new(), new PaginationDto());

        var pref = me.Preference;

        // VIP restrictions on filters
        if (filters != null)
        {
            if (!me.IsPremium && filters.MaxDistance.HasValue && filters.MaxDistance > MaxFreeRadiusKm)
                throw new InvalidOperationException(
                    $"Searching beyond {MaxFreeRadiusKm}km requires a VIP subscription. Upgrade to Gold or Platinum.");

            if (!me.IsPremium && !string.IsNullOrWhiteSpace(filters.City))
                throw new InvalidOperationException("Searching by city requires a VIP subscription.");

            if (!me.IsPremium && filters.Global == true)
                throw new InvalidOperationException("Global search requires a VIP subscription.");
        }

        var targetGender = filters?.Gender?.ToLower() switch
        {
            "male" => "male",
            "female" => "female",
            _ => pref?.InterestedIn?.ToLower() switch
            {
                "girls" or "female" => "female",
                "boys" or "male" => "male",
                _ => me.Gender?.ToLower() == "male" ? "female" : "male"
            }
        };

        var blockedIds = await _db.Blocks
            .Where(b => b.BlockerId == userId || b.BlockedUserId == userId)
            .Select(b => b.BlockerId == userId ? b.BlockedUserId : b.BlockerId)
            .ToListAsync();
        var swipedIds = await _db.Swipes.Where(s => s.SwiperId == userId).Select(s => s.TargetId).ToListAsync();
        var exclude = blockedIds.Union(swipedIds).Append(userId).ToHashSet();

        var q = _db.Users
            .Include(u => u.Location)
            .Include(u => u.Images)
            .Include(u => u.Interests).ThenInclude((UserInterest i) => i.Interest)
            .Where(u => !u.IsDeleted && u.IsActive && u.ProfileComplete
                     && !exclude.Contains(u.Id) && u.Gender!.ToLower() == targetGender);

        var minAge = filters?.MinAge ?? pref?.MinAge ?? 0;
        var maxAge = filters?.MaxAge ?? pref?.MaxAge ?? 0;
        if (minAge > 0) { var maxDob = DateTime.UtcNow.AddYears(-minAge); q = q.Where(u => u.DateOfBirth <= maxDob); }
        if (maxAge > 0) { var minDob = DateTime.UtcNow.AddYears(-maxAge - 1); q = q.Where(u => u.DateOfBirth >= minDob); }

        var onlineOnly = filters?.OnlineOnly ?? pref?.OnlineOnly ?? false;
        if (onlineOnly) q = q.Where(u => u.IsOnline);

        if (pref?.VerifiedOnly == true && me.IsPremium) q = q.Where(u => u.IsVerified);

        // City filter (VIP only)
        if (!string.IsNullOrWhiteSpace(filters?.City) && me.IsPremium)
        {
            var cityLower = filters.City.ToLower();
            q = q.Where(u => u.Location != null && u.Location.City != null &&
                             u.Location.City.ToLower().Contains(cityLower));
        }

        var total = await q.CountAsync();
        var users = await q
            .OrderByDescending(u => u.IsOnline)
            .ThenByDescending(u => u.IsPremium)
            .ThenByDescending(u => u.LastActiveAt)
            .Skip((page - 1) * limit).Take(limit)
            .ToListAsync();

        var myInterestIds = me.Interests.Select(i => i.InterestId).ToHashSet();
        // Travel Mode (Premium): match from the travel destination instead of real GPS location.
        var myLat = (me.IsTravelMode && me.TravelLat.HasValue) ? me.TravelLat : me.Location?.Lat;
        var myLng = (me.IsTravelMode && me.TravelLng.HasValue) ? me.TravelLng : me.Location?.Lng;

        var radiusKm = filters?.MaxDistance ?? (me.IsPremium ? pref?.MaxDistance ?? DefaultRadiusKm : DefaultRadiusKm);
        if (!me.IsPremium) radiusKm = Math.Min(radiusKm, MaxFreeRadiusKm);
        if (filters?.Global == true && me.IsPremium) radiusKm = int.MaxValue;

        var dtos = users
            .Select(u =>
            {
                var dto = MapDiscover(u, me, myInterestIds);
                if (myLat.HasValue && myLng.HasValue
                    && u.Location?.Lat.HasValue == true
                    && u.Location?.Lng.HasValue == true)
                {
                    dto.Distance = HaversineKm(myLat.Value, myLng.Value,
                                               u.Location.Lat.Value, u.Location.Lng.Value);
                }
                return dto;
            })
            .Where(u => !myLat.HasValue || u.Distance == null || u.Distance <= radiusKm)
            .OrderBy(u => u.Distance ?? double.MaxValue)
            .ThenByDescending(u => u.IsOnline)
            .ToList();

        return (dtos, new PaginationDto
        {
            Page = page,
            Limit = limit,
            Total = total,
            HasNext = (page * limit) < total,
        });
    }

    public async Task<SwipeResponse> SwipeAsync(Guid swiperId, SwipeRequest req)
    {
        if (!Guid.TryParse(req.TargetId, out var targetId))
            throw new InvalidOperationException("Invalid target ID.");
        if (swiperId == targetId) throw new InvalidOperationException("Cannot swipe on yourself.");

        var me = await _db.Users.FindAsync(swiperId) ?? throw new InvalidOperationException("User not found.");
        int coinsDeducted = 0;

        if (req.Action == "superlike")
        {
            if (me.CoinBalance < MingleyDbContext.SuperLikeCost)
                throw new InvalidOperationException(
                    $"Need {MingleyDbContext.SuperLikeCost} coins for Super Like. You have {me.CoinBalance}.");
            await _wallet.DeductCoinsAsync(swiperId, MingleyDbContext.SuperLikeCost, "Super Like sent", "superlike", req.TargetId);
            coinsDeducted = MingleyDbContext.SuperLikeCost;
        }

        var exists = await _db.Swipes.AnyAsync(s => s.SwiperId == swiperId && s.TargetId == targetId);
        if (exists)
        {
            var remaining = me.CoinBalance / Mingley.Infrastructure.Persistence.MingleyDbContext.SuperLikeCost;
            return new SwipeResponse { IsMatch = false, RemainingSuperlikes = remaining };
        }

        _db.Swipes.Add(new Swipe { SwiperId = swiperId, TargetId = targetId, Action = req.Action });
        await _db.SaveChangesAsync();

        if (req.Action != "dislike")
        {
            var mutual = await _db.Swipes
                .AnyAsync(s => s.SwiperId == targetId && s.TargetId == swiperId && s.Action != "dislike");

            if (mutual)
            {
                var match = new Match { User1Id = swiperId, User2Id = targetId };
                _db.Matches.Add(match);
                await _db.SaveChangesAsync();

                var chat = new Chat { MatchId = match.Id };
                _db.Chats.Add(chat);
                await _db.SaveChangesAsync();

                var target = await _db.Users.FindAsync(targetId);
                await _notifs.CreateAsync(swiperId, "🎉 New Match!", $"You matched with {target?.FullName}!", "match", match.Id.ToString());
                await _notifs.CreateAsync(targetId, "🎉 New Match!", $"You matched with {me.FullName}!", "match", match.Id.ToString());

                var matchDto = new MatchDto
                {
                    MatchId = match.Id.ToString(),
                    MatchedAt = match.CreatedAt,
                    User = new MatchedUserDto
                    {
                        Id = targetId.ToString(),
                        FullName = target?.FullName,
                        Avatar = target?.Avatar,
                        IsOnline = target?.IsOnline ?? false,
                    },
                };

                await _hub.SendToUserAsync(swiperId.ToString(), "NewMatch", new
                {
                    matchId = match.Id.ToString(),
                    chatId = chat.Id.ToString(),
                    matchedAt = match.CreatedAt,
                    user = new { id = targetId.ToString(), fullName = target?.FullName, avatar = target?.Avatar },
                });
                await _hub.SendToUserAsync(targetId.ToString(), "NewMatch", new
                {
                    matchId = match.Id.ToString(),
                    chatId = chat.Id.ToString(),
                    matchedAt = match.CreatedAt,
                    user = new { id = swiperId.ToString(), fullName = me.FullName, avatar = me.Avatar },
                });

                // Refresh coin balance after deduction
                await _db.Entry(me).ReloadAsync();
                var remainingSuperLikes = me.CoinBalance / Mingley.Infrastructure.Persistence.MingleyDbContext.SuperLikeCost;
                return new SwipeResponse { IsMatch = true, Match = matchDto, CoinsDeducted = coinsDeducted, RemainingSuperlikes = remainingSuperLikes };
            }
        }
        // Calculate remaining superlikes based on current coin balance
        await _db.Entry(me).ReloadAsync();
        var remainingSuperlikes = me.CoinBalance / Mingley.Infrastructure.Persistence.MingleyDbContext.SuperLikeCost;
        return new SwipeResponse { IsMatch = false, CoinsDeducted = coinsDeducted, RemainingSuperlikes = remainingSuperlikes };
    }

    public async Task<List<MatchListItemDto>> GetMatchesAsync(Guid userId, int page, int limit)
    {
        var matches = await _db.Matches
            .Include(m => m.User1)
            .Include(m => m.User2)
            .Include(m => m.Chat).ThenInclude((Chat c) => c.Messages)
            .Where(m => !m.IsDeleted && m.IsActive && (m.User1Id == userId || m.User2Id == userId))
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * limit).Take(limit)
            .ToListAsync();

        if (!matches.Any()) return new List<MatchListItemDto>();

        var otherIds = matches
            .Select(m => m.User1Id == userId ? m.User2Id : m.User1Id)
            .Distinct().ToList();

        var images = await _db.Set<UserImage>()
            .Where(i => otherIds.Contains(i.UserId) && !i.IsDeleted).ToListAsync();
        var interests = await _db.Set<UserInterest>()
            .Include((UserInterest i) => i.Interest)
            .Where(i => otherIds.Contains(i.UserId)).ToListAsync();
        var locations = await _db.Set<UserLocation>()
            .Where(l => otherIds.Contains(l.UserId)).ToListAsync();

        var imgByUser = images.GroupBy(i => i.UserId).ToDictionary(g => g.Key, g => g.ToList());
        var intByUser = interests.GroupBy(i => i.UserId).ToDictionary(g => g.Key, g => g.ToList());
        var locByUser = locations.ToDictionary(l => l.UserId);

        return matches.Select(m =>
        {
            var other = m.User1Id == userId ? m.User2 : m.User1;
            if (other == null) return null;

            var msgs = m.Chat?.Messages?.Where(msg => !msg.IsDeleted)
                              .OrderByDescending(msg => msg.CreatedAt).ToList()
                          ?? new List<Message>();
            var lastMsg = msgs.FirstOrDefault();
            var unread = msgs.Count(msg => msg.SenderId != userId && msg.ReadAt == null);

            var userImgs = imgByUser.TryGetValue(other.Id, out var imgs) ? imgs : new List<UserImage>();
            var userInts = intByUser.TryGetValue(other.Id, out var ints) ? ints : new List<UserInterest>();
            locByUser.TryGetValue(other.Id, out var location);

            var primaryImg = userImgs.FirstOrDefault(i => i.IsPrimary)?.Url
                           ?? userImgs.OrderBy(i => i.SortOrder).FirstOrDefault()?.Url
                           ?? other.Avatar;

            var age = other.DateOfBirth.HasValue
                ? (int)((DateTime.UtcNow - other.DateOfBirth.Value).TotalDays / 365.25)
                : (int?)null;

            return new MatchListItemDto
            {
                MatchId = m.Id.ToString(),
                ChatId = m.Chat?.Id.ToString(),
                MatchedAt = m.CreatedAt,
                UnreadCount = unread,
                User = new MatchedUserDto
                {
                    Id = other.Id.ToString(),
                    FullName = other.FullName,
                    Avatar = primaryImg,
                    IsOnline = other.IsOnline,
                    LastActiveAt = other.LastActiveAt,
                },
                MatchedUser = new MatchedUserDto
                {
                    Id = other.Id.ToString(),
                    FullName = other.FullName,
                    Avatar = primaryImg,
                    IsOnline = other.IsOnline,
                    LastActiveAt = other.LastActiveAt,
                },
                LastMessage = lastMsg == null ? null : new LastMessageDto
                {
                    Text = lastMsg.Type == "coins"
                                ? $"💰 Sent {lastMsg.CoinAmount} coins"
                                : lastMsg.ImageUrl != null ? "📷 Photo" : lastMsg.Text,
                    Type = lastMsg.Type,
                    SentAt = lastMsg.CreatedAt,
                },
                FullProfile = new MatchFullProfileDto
                {
                    Id = other.Id.ToString(),
                    FullName = other.FullName,
                    Age = age,
                    Bio = other.Bio,
                    Gender = other.Gender,
                    Avatar = primaryImg,
                    IsVerified = other.IsVerified,
                    IsPremium = other.IsPremium,
                    IsOnline = other.IsOnline,
                    LastActiveAt = other.LastActiveAt,
                    City = location?.City,
                    Images = userImgs.OrderBy(i => i.SortOrder).Select(i => i.Url).ToList(),
                    Interests = userInts
                                    .Select(i => i.Interest?.Name ?? "")
                                    .Where(n => !string.IsNullOrEmpty(n)).ToList(),
                },
            };
        }).Where(x => x != null).Select(x => x!).ToList();
    }

    public async Task UnmatchAsync(Guid userId, Guid matchId)
    {
        var match = await _db.Matches.Include(m => m.Chat)
            .FirstOrDefaultAsync(m => m.Id == matchId && (m.User1Id == userId || m.User2Id == userId))
            ?? throw new InvalidOperationException("Match not found.");
        match.IsActive = false;
        match.UpdatedAt = DateTime.UtcNow;
        if (match.Chat != null) { match.Chat.IsDeleted = true; match.Chat.UpdatedAt = DateTime.UtcNow; }
        await _db.SaveChangesAsync();
        var otherId = match.User1Id == userId ? match.User2Id : match.User1Id;
        await _hub.SendToUserAsync(otherId.ToString(), "Unmatched", new { matchId = matchId.ToString() });
    }

    public async Task<List<DiscoverUserDto>> GetWhoLikedMeAsync(Guid userId)
    {
        var me = await _db.Users.Include(u => u.Interests)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (me == null) return new();

        var likerIds = await _db.Swipes
            .Where(s => s.TargetId == userId && s.Action != "dislike")
            .Select(s => s.SwiperId).ToListAsync();
        var swipedByMe = await _db.Swipes
            .Where(s => s.SwiperId == userId).Select(s => s.TargetId).ToListAsync();
        var pending = likerIds.Except(swipedByMe).ToList();

        var users = await _db.Users
            .Include(u => u.Images)
            .Include(u => u.Interests).ThenInclude((UserInterest i) => i.Interest)
            .Where(u => pending.Contains(u.Id) && !u.IsDeleted)
            .ToListAsync();

        var myInterestIds = me.Interests.Select(i => i.InterestId).ToHashSet();
        return users.Select(u => MapDiscover(u, me, myInterestIds)).ToList();
    }

    //public async Task<List<TrendingSection>> GetTrendingAsync(Guid userId)
    //{
    //    var me = await _db.Users.FindAsync(userId);
    //    if (me == null) return new();

    //    var femaleUsers = await _db.Users
    //        .Include(u => u.Images)
    //        .Include(u => u.Interests).ThenInclude((UserInterest i) => i.Interest)
    //        .Include(u => u.Location)
    //        .Where(u => u.Gender != null && u.Gender.ToLower() == "female"
    //                 && u.IsActive && !u.IsDeleted && u.ProfileComplete && u.Id != userId)
    //        .ToListAsync();

    //    if (!femaleUsers.Any()) return new();

    //    var userIds = femaleUsers.Select(u => u.Id).ToList();

    //    var matchCounts = await _db.Matches
    //        .Where(m => m.IsActive && (userIds.Contains(m.User1Id) || userIds.Contains(m.User2Id)))
    //        .GroupBy(m => userIds.Contains(m.User1Id) ? m.User1Id : m.User2Id)
    //        .Select(g => new { UserId = g.Key, Count = g.Count() })
    //        .ToDictionaryAsync(x => x.UserId, x => x.Count);

    //    var scCounts = await _db.SuperChats
    //        .Where(s => userIds.Contains(s.ToUserId) && s.IsResponded)
    //        .GroupBy(s => s.ToUserId)
    //        .Select(g => new { UserId = g.Key, Count = g.Count(), Earned = g.Sum(s => s.GirlCommission) })
    //        .ToDictionaryAsync(x => x.UserId, x => new { x.Count, x.Earned });

    //    var trendingUsers = femaleUsers.Select(u =>
    //    {
    //        matchCounts.TryGetValue(u.Id, out var matches);
    //        scCounts.TryGetValue(u.Id, out var sc);

    //        var age = u.DateOfBirth.HasValue
    //            ? (int)((DateTime.UtcNow - u.DateOfBirth.Value).TotalDays / 365.25)
    //            : (int?)null;

    //        var score = (matches * 3)
    //                  + ((sc?.Count ?? 0) * 5)
    //                  + (u.IsOnline ? 10 : 0)
    //                  + (u.IsPremium ? 5 : 0)
    //                  + (u.IsVerified ? 3 : 0);

    //        return new TrendingUserDto
    //        {
    //            Id = u.Id.ToString(),
    //            FullName = u.FullName,
    //            Age = age,
    //            Bio = u.Bio,
    //            Avatar = u.Images.FirstOrDefault(i => i.IsPrimary)?.Url
    //                            ?? u.Images.FirstOrDefault()?.Url ?? u.Avatar,
    //            IsVerified = u.IsVerified,
    //            IsPremium = u.IsPremium,
    //            IsOnline = u.IsOnline,
    //            City = u.Location?.City,
    //            TotalMatches = matches,
    //            TotalSuperChats = sc?.Count ?? 0,
    //            TotalEarned = sc?.Earned ?? 0,
    //            TrendingScore = score,
    //            Images = u.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).ToList(),
    //            Interests = u.Interests
    //                                .Select(i => i.Interest?.Name ?? "")
    //                                .Where(n => !string.IsNullOrEmpty(n)).ToList(),
    //        };
    //    }).ToList();

    //    return new List<TrendingSection>
    //    {
    //        new() { Section = "trending",     Title = "🔥 Trending Now",       Subtitle = "Most active profiles this week",         Users = trendingUsers.OrderByDescending(u => u.TrendingScore).Take(10).ToList() },
    //        new() { Section = "most_popular", Title = "⭐ Most Popular",        Subtitle = "Girls with the most matches",            Users = trendingUsers.OrderByDescending(u => u.TotalMatches).Take(10).ToList() },
    //        new() { Section = "top_earners",  Title = "💰 Top SuperChat",       Subtitle = "Girls earning from SuperChats",          Users = trendingUsers.OrderByDescending(u => u.TotalSuperChats).Take(10).ToList() },
    //        new() { Section = "online_now",   Title = "🟢 Online Now",          Subtitle = "Available to chat right now",            Users = trendingUsers.Where(u => u.IsOnline).OrderByDescending(u => u.TrendingScore).Take(10).ToList() },
    //        new() { Section = "recommended",  Title = "💫 Recommended For You", Subtitle = "Based on your activity",                 Users = trendingUsers.OrderBy(_ => Guid.NewGuid()).Take(10).ToList() },
    //    };
    //}

    public async Task<List<TrendingSection>> GetTrendingAsync(Guid userId)
    {
        var me = await _db.Users.FindAsync(userId);
        if (me == null) return new();

        var femaleUsers = await _db.Users
            .Include(u => u.Images)
            .Include(u => u.Interests).ThenInclude((UserInterest i) => i.Interest)
            .Include(u => u.Location)
            .Where(u => u.Gender != null && u.Gender.ToLower() == "female"
                     && u.IsActive && !u.IsDeleted && u.ProfileComplete && u.Id != userId)
            .ToListAsync();

        if (!femaleUsers.Any()) return new();

        var userIds = femaleUsers.Select(u => u.Id).ToList();

        var matchCounts = await _db.Matches
            .Where(m => m.IsActive && (userIds.Contains(m.User1Id) || userIds.Contains(m.User2Id)))
            .GroupBy(m => userIds.Contains(m.User1Id) ? m.User1Id : m.User2Id)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        var scCounts = await _db.SuperChats
            .Where(s => userIds.Contains(s.ToUserId) && s.IsResponded)
            .GroupBy(s => s.ToUserId)
            .Select(g => new { UserId = g.Key, Count = g.Count(), Earned = g.Sum(s => s.GirlCommission) })
            .ToDictionaryAsync(x => x.UserId, x => new { x.Count, x.Earned });

        var trendingUsers = femaleUsers.Select(u =>
        {
            matchCounts.TryGetValue(u.Id, out var matches);
            scCounts.TryGetValue(u.Id, out var sc);

            var age = u.DateOfBirth.HasValue
                ? (int)((DateTime.UtcNow - u.DateOfBirth.Value).TotalDays / 365.25)
                : (int?)null;

            // Admin-pinned IsTrending flag gives a massive score boost — always floats to top
            var adminBoost = u.IsTrending ? 1000 : 0;

            var score = adminBoost
                      + (matches * 3)
                      + ((sc?.Count ?? 0) * 5)
                      + (u.IsOnline ? 10 : 0)
                      + (u.IsPremium ? 5 : 0)
                      + (u.IsVerified ? 3 : 0);

            return new TrendingUserDto
            {
                Id = u.Id.ToString(),
                FullName = u.FullName,
                Age = age,
                Bio = u.Bio,
                Avatar = u.Images.FirstOrDefault(i => i.IsPrimary)?.Url
                                ?? u.Images.FirstOrDefault()?.Url ?? u.Avatar,
                IsVerified = u.IsVerified,
                IsPremium = u.IsPremium,
                IsOnline = u.IsOnline,
                IsTrending = u.IsTrending,
                City = u.Location?.City,
                TotalMatches = matches,
                TotalSuperChats = sc?.Count ?? 0,
                TotalEarned = sc?.Earned ?? 0,
                TrendingScore = score,
                Images = u.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).ToList(),
                Interests = u.Interests
                                    .Select(i => i.Interest?.Name ?? "")
                                    .Where(n => !string.IsNullOrEmpty(n)).ToList(),
            };
        }).ToList();

        // Admin-pinned users get a dedicated "Featured" section at the very top
        var featuredUsers = trendingUsers
            .Where(u => u.IsTrending)
            .OrderByDescending(u => u.TrendingScore)
            .Take(10).ToList();

        var sections = new List<TrendingSection>();

        if (featuredUsers.Any())
            sections.Add(new() { Section = "featured", Title = "⭐ Featured", Subtitle = "Hand-picked profiles just for you", Users = featuredUsers });

        sections.Add(new() { Section = "trending", Title = "🔥 Trending Now", Subtitle = "Most active profiles this week", Users = trendingUsers.OrderByDescending(u => u.TrendingScore).Take(10).ToList() });
        sections.Add(new() { Section = "most_popular", Title = "⭐ Most Popular", Subtitle = "Girls with the most matches", Users = trendingUsers.OrderByDescending(u => u.TotalMatches).Take(10).ToList() });
        sections.Add(new() { Section = "top_earners", Title = "💰 Top SuperChat", Subtitle = "Girls earning from SuperChats", Users = trendingUsers.OrderByDescending(u => u.TotalSuperChats).Take(10).ToList() });
        sections.Add(new() { Section = "online_now", Title = "🟢 Online Now", Subtitle = "Available to chat right now", Users = trendingUsers.Where(u => u.IsOnline).OrderByDescending(u => u.TrendingScore).Take(10).ToList() });
        sections.Add(new() { Section = "recommended", Title = "💫 Recommended For You", Subtitle = "Based on your activity", Users = trendingUsers.OrderBy(_ => Guid.NewGuid()).Take(10).ToList() });

        return sections;
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                 + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
                 * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRad(double deg) => deg * Math.PI / 180;

    private static DiscoverUserDto MapDiscover(User u, User me, HashSet<Guid> myInterests)
    {
        var age = u.DateOfBirth.HasValue
            ? (int)((DateTime.UtcNow - u.DateOfBirth.Value).TotalDays / 365.25)
            : (int?)null;
        var score = u.Interests.Count(i => myInterests.Contains(i.InterestId)) * 10
                  + (u.IsOnline ? 5 : 0) + (u.IsPremium ? 3 : 0) + (u.IsVerified ? 2 : 0);
        return new DiscoverUserDto
        {
            Id = u.Id.ToString(),
            FullName = u.FullName,
            Age = age,
            Bio = u.Bio,
            Gender = u.Gender,
            IsVerified = u.IsVerified,
            IsPremium = u.IsPremium,
            IsOnline = u.IsOnline,
            City = u.Location?.City,
            Avatar = u.Images.FirstOrDefault(i => i.IsPrimary)?.Url
                       ?? u.Images.FirstOrDefault()?.Url ?? u.Avatar,
            Images = u.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).ToList(),
            Interests = u.Interests
                          .Select(i => i.Interest?.Name ?? "")
                          .Where(n => !string.IsNullOrEmpty(n)).ToList(),
            MatchScore = score,
        };
    }
}