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

    public DiscoverService(MingleyDbContext db, INotificationService notifs, IHubNotifier hub, IWalletService wallet)
    { _db = db; _notifs = notifs; _hub = hub; _wallet = wallet; }

    public async Task<(List<DiscoverUserDto> Users, PaginationDto Pagination)> GetFeedAsync(
        Guid userId, int page, int limit, DiscoverFilters? filters = null)
    {
        var me = await _db.Users
            .Include(u => u.Preference).Include(u => u.Location).Include(u => u.Interests)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        if (me == null) return (new(), new PaginationDto());

        var pref = me.Preference;

        // Determine target gender: use filter first, then preference, then opposite
        var targetGender = filters?.Gender?.ToLower() switch
        {
            "male"   => "male",
            "female" => "female",
            _ => pref?.InterestedIn?.ToLower() switch
            {
                "girls" or "female" => "female",
                "boys"  or "male"   => "male",
                _ => me.Gender?.ToLower() == "male" ? "female" : "male"
            }
        };

        var blockedIds = await _db.Blocks
            .Where(b => b.BlockerId == userId || b.BlockedUserId == userId)
            .Select(b => b.BlockerId == userId ? b.BlockedUserId : b.BlockerId).ToListAsync();
        var swipedIds = await _db.Swipes.Where(s => s.SwiperId == userId).Select(s => s.TargetId).ToListAsync();
        var exclude = blockedIds.Union(swipedIds).Append(userId).ToHashSet();

        var q = _db.Users
            .Include(u => u.Location)
            .Include(u => u.Images.Where(i => !i.IsDeleted))
            .Include(u => u.Interests).ThenInclude(i => i.Interest)
            .Where(u => !u.IsDeleted && u.IsActive && u.ProfileComplete
                     && !exclude.Contains(u.Id) && u.Gender!.ToLower() == targetGender);

        // ── Apply age filters (use inline filter first, fallback to preferences) ──
        var minAge = filters?.MinAge ?? pref?.MinAge ?? 0;
        var maxAge = filters?.MaxAge ?? pref?.MaxAge ?? 0;
        if (minAge > 0) { var maxDob = DateTime.UtcNow.AddYears(-minAge); q = q.Where(u => u.DateOfBirth <= maxDob); }
        if (maxAge > 0) { var minDob = DateTime.UtcNow.AddYears(-maxAge - 1); q = q.Where(u => u.DateOfBirth >= minDob); }

        // ── Online only filter ──
        var onlineOnly = filters?.OnlineOnly ?? pref?.OnlineOnly ?? false;
        if (onlineOnly) q = q.Where(u => u.IsOnline);

        // ── Verified only (premium feature) ──
        if (pref?.VerifiedOnly == true && me.IsPremium) q = q.Where(u => u.IsVerified);

        var total = await q.CountAsync();
        var users = await q
            .OrderByDescending(u => u.IsOnline)
            .ThenByDescending(u => u.IsPremium)
            .ThenByDescending(u => u.LastActiveAt)
            .Skip((page - 1) * limit).Take(limit)
            .ToListAsync();

        var myInterestIds = me.Interests.Select(i => i.InterestId).ToHashSet();
        var dtos = users.Select(u => MapDiscover(u, me, myInterestIds)).ToList();
        return (dtos, new PaginationDto { Page = page, Limit = limit, Total = total, HasNext = (page * limit) < total });
    }

    public async Task<SwipeResponse> SwipeAsync(Guid swiperId, SwipeRequest req)
    {
        if (!Guid.TryParse(req.TargetId, out var targetId))
            throw new InvalidOperationException("Invalid target ID.");
        if (swiperId == targetId)
            throw new InvalidOperationException("Cannot swipe on yourself.");

        var me = await _db.Users.FindAsync(swiperId) ?? throw new InvalidOperationException("User not found.");
        int coinsDeducted = 0;

        if (req.Action == "superlike")
        {
            if (me.CoinBalance < MingleyDbContext.SuperLikeCost)
                throw new InvalidOperationException($"Need {MingleyDbContext.SuperLikeCost} coins for Super Like. You have {me.CoinBalance}.");
            await _wallet.DeductCoinsAsync(swiperId, MingleyDbContext.SuperLikeCost, "Super Like sent", "superlike", req.TargetId);
            coinsDeducted = MingleyDbContext.SuperLikeCost;
        }

        // Prevent duplicate swipes
        var exists = await _db.Swipes.AnyAsync(s => s.SwiperId == swiperId && s.TargetId == targetId);
        if (exists) return new SwipeResponse { IsMatch = false };

        var swipe = new Swipe { SwiperId = swiperId, TargetId = targetId, Action = req.Action };
        _db.Swipes.Add(swipe);
        await _db.SaveChangesAsync();

        // Check for mutual like → create match
        if (req.Action != "dislike")
        {
            var mutual = await _db.Swipes
                .AnyAsync(s => s.SwiperId == targetId && s.TargetId == swiperId && s.Action != "dislike");

            if (mutual)
            {
                // Create match + chat
                var match = new Match { User1Id = swiperId, User2Id = targetId };
                _db.Matches.Add(match);
                await _db.SaveChangesAsync();

                var chat = new Chat { MatchId = match.Id };
                _db.Chats.Add(chat);
                await _db.SaveChangesAsync();

                var target = await _db.Users.FindAsync(targetId);

                // Notify both users
                await _notifs.CreateAsync(swiperId, "🎉 New Match!", $"You matched with {target?.FullName}!", "match", match.Id.ToString());
                await _notifs.CreateAsync(targetId, "🎉 New Match!", $"You matched with {me.FullName}!", "match", match.Id.ToString());

                var matchDto = new MatchDto
                {
                    MatchId   = match.Id.ToString(),
                    MatchedAt = match.CreatedAt,
                    User      = new MatchedUserDto { Id = targetId.ToString(), FullName = target?.FullName, Avatar = target?.Avatar, IsOnline = target?.IsOnline ?? false },
                };

                // Push to both via SignalR
                await _hub.SendToUserAsync(swiperId.ToString(), "NewMatch", new
                {
                    matchId = match.Id.ToString(), chatId = chat.Id.ToString(),
                    matchedAt = match.CreatedAt,
                    user = new { id = targetId.ToString(), fullName = target?.FullName, avatar = target?.Avatar, isOnline = target?.IsOnline }
                });
                await _hub.SendToUserAsync(targetId.ToString(), "NewMatch", new
                {
                    matchId = match.Id.ToString(), chatId = chat.Id.ToString(),
                    matchedAt = match.CreatedAt,
                    user = new { id = swiperId.ToString(), fullName = me.FullName, avatar = me.Avatar, isOnline = me.IsOnline }
                });

                return new SwipeResponse { IsMatch = true, Match = matchDto };
            }
        }

        return new SwipeResponse { IsMatch = false, CoinsDeducted = coinsDeducted };
    }

    public async Task<List<MatchListItemDto>> GetMatchesAsync(Guid userId, int page, int limit)
    {
        var matches = await _db.Matches
            .Include(m => m.User1).Include(m => m.User2).Include(m => m.Chat)
            .Where(m => !m.IsDeleted && m.IsActive && (m.User1Id == userId || m.User2Id == userId))
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * limit).Take(limit)
            .ToListAsync();

        return matches.Select(m =>
        {
            var other = m.User1Id == userId ? m.User2! : m.User1!;
            return new MatchListItemDto
            {
                MatchId   = m.Id.ToString(),
                ChatId    = m.Chat?.Id.ToString(),
                MatchedAt = m.CreatedAt,
                User      = new MatchedUserDto
                {
                    Id = other.Id.ToString(), FullName = other.FullName, Avatar = other.Avatar,
                    IsOnline = other.IsOnline, LastActiveAt = other.LastActiveAt,
                },
            };
        }).ToList();
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
        var me = await _db.Users.Include(u => u.Interests).FirstOrDefaultAsync(u => u.Id == userId);
        if (me == null) return new();

        var likerIds = await _db.Swipes
            .Where(s => s.TargetId == userId && s.Action != "dislike")
            .Select(s => s.SwiperId).ToListAsync();

        var swipedByMe = await _db.Swipes.Where(s => s.SwiperId == userId).Select(s => s.TargetId).ToListAsync();
        var pending = likerIds.Except(swipedByMe).ToList();

        var users = await _db.Users
            .Include(u => u.Images.Where(i => !i.IsDeleted))
            .Include(u => u.Interests).ThenInclude(i => i.Interest)
            .Where(u => pending.Contains(u.Id) && !u.IsDeleted)
            .ToListAsync();

        var myInterestIds = me.Interests.Select(i => i.InterestId).ToHashSet();
        return users.Select(u => MapDiscover(u, me, myInterestIds)).ToList();
    }

    private static DiscoverUserDto MapDiscover(User u, User me, HashSet<Guid> myInterests)
    {
        var age = u.DateOfBirth.HasValue
            ? (int)((DateTime.UtcNow - u.DateOfBirth.Value).TotalDays / 365.25)
            : (int?)null;
        var score = u.Interests.Count(i => myInterests.Contains(i.InterestId)) * 10
                  + (u.IsOnline ? 5 : 0) + (u.IsPremium ? 3 : 0) + (u.IsVerified ? 2 : 0);
        return new DiscoverUserDto
        {
            Id = u.Id.ToString(), FullName = u.FullName, Age = age, Bio = u.Bio,
            Gender = u.Gender, IsVerified = u.IsVerified, IsPremium = u.IsPremium, IsOnline = u.IsOnline,
            City = u.Location?.City,
            Avatar = u.Images.FirstOrDefault(i => i.IsPrimary)?.Url ?? u.Images.FirstOrDefault()?.Url ?? u.Avatar,
            Images = u.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).ToList(),
            Interests = u.Interests.Select(i => i.Interest?.Name ?? "").Where(n => !string.IsNullOrEmpty(n)).ToList(),
            MatchScore = score,
        };
    }
}
