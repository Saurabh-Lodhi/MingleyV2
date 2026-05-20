
using Microsoft.EntityFrameworkCore;
using Mingley.Application.DTOs.SuperChat;
using Mingley.Application.Interfaces;
using Mingley.Domain.Entities;
using Mingley.Infrastructure.Persistence;

namespace Mingley.Infrastructure.Services;

public class SuperChatService : ISuperChatService
{
    private readonly MingleyDbContext _db;
    private readonly INotificationService _notifs;
    private readonly IHubNotifier _hub;

    public SuperChatService(MingleyDbContext db, INotificationService notifs, IHubNotifier hub)
    { _db = db; _notifs = notifs; _hub = hub; }

    public async Task<SendSuperChatResponse> SendAsync(Guid fromUserId, SendSuperChatRequest req)
    {
        if (!Guid.TryParse(req.ToUserId, out var toUserId))
            throw new InvalidOperationException("Invalid target user ID.");
        if (fromUserId == toUserId)
            throw new InvalidOperationException("Cannot send SuperChat to yourself.");

        // NEW: Use custom coin amount (minimum 50, defaults to 500)
        var coinAmount = Math.Max(50, req.CoinAmount);
        var message = string.IsNullOrWhiteSpace(req.Message)
                            ? $"💰 Sent you {coinAmount} coins!"
                            : req.Message.Trim();

        var sender = await _db.Users.FindAsync(fromUserId) ?? throw new InvalidOperationException("User not found.");
        var target = await _db.Users.FindAsync(toUserId) ?? throw new InvalidOperationException("Target user not found.");

        if (sender.CoinBalance < coinAmount)
            throw new InvalidOperationException(
                $"Insufficient coins. Need {coinAmount} coins for SuperChat. You have {sender.CoinBalance}.");

        // Deduct coins from sender
        sender.CoinBalance -= coinAmount;
        sender.UpdatedAt = DateTime.UtcNow;

        // Calculate INR commission: 50% girl / 50% company
        var totalInr = coinAmount * MingleyDbContext.CoinToInrRate;
        var girlCommission = totalInr * MingleyDbContext.GirlCommissionPct;
        var companyRevenue = totalInr - girlCommission;

        // Track INR earnings (coins are credited on accept — see RespondAsync)
        target.TotalEarned += girlCommission;
        target.UpdatedAt = DateTime.UtcNow;

        var sc = new SuperChat
        {
            FromUserId = fromUserId,
            ToUserId = toUserId,
            Message = message,
            CoinAmount = coinAmount,
            GirlCommission = girlCommission,
            CompanyRevenue = companyRevenue,
        };
        _db.SuperChats.Add(sc);

        _db.CoinTransactions.Add(new CoinTransaction
        {
            UserId = fromUserId,
            Coins = coinAmount,
            Direction = "debit",
            Description = $"SuperChat to {target.FullName}",
            TransactionType = "superchat",
            ReferenceId = sc.Id.ToString(),
        });

        await _db.SaveChangesAsync();

        // How many coins the receiver will get when they accept (50%)
        var coinsReceiverWillGet = coinAmount / 2;

        // Real-time push
        await _hub.SendToUserAsync(toUserId.ToString(), "NewSuperChat", new
        {
            superChatId = sc.Id.ToString(),
            fromUserId = fromUserId.ToString(),
            fromName = sender.FullName,
            fromAvatar = sender.Avatar,
            message = sc.Message,
            coinAmount = sc.CoinAmount,
            coinsYouWillGet = coinsReceiverWillGet,
            commission = girlCommission,
            createdAt = sc.CreatedAt,
        });
        await _notifs.CreateAsync(toUserId,
            $"⭐ SuperChat from {sender.FullName}!",
            $"\"{message.Substring(0, Math.Min(60, message.Length))}\" — accept to earn {coinsReceiverWillGet} coins!",
            "superchat", sc.Id.ToString());

        return new SendSuperChatResponse
        {
            SuperChatId = sc.Id.ToString(),
            CoinsDeducted = coinAmount,
            NewBalance = sender.CoinBalance,
            GirlCommission = girlCommission,
            CompanyRevenue = companyRevenue,
            CoinsReceiverWillGet = coinsReceiverWillGet,
        };
    }

    public async Task<SuperChatDto> RespondAsync(Guid toUserId, Guid superChatId)
    {
        var sc = await _db.SuperChats.Include(s => s.FromUser).Include(s => s.ToUser)
            .FirstOrDefaultAsync(s => s.Id == superChatId && s.ToUserId == toUserId)
            ?? throw new InvalidOperationException("SuperChat not found.");

        if (sc.IsResponded)
            throw new InvalidOperationException("You already responded to this SuperChat.");

        // NEW: Credit 50% of the sent coins to the receiver's CoinBalance
        var coinsToAward = sc.CoinAmount / 2;
        if (sc.ToUser != null)
        {
            sc.ToUser.CoinBalance += coinsToAward;
            sc.ToUser.UpdatedAt = DateTime.UtcNow;
        }

        _db.CoinTransactions.Add(new CoinTransaction
        {
            UserId = toUserId,
            Coins = coinsToAward,
            Direction = "credit",
            Description = $"SuperChat coins from {sc.FromUser?.FullName}",
            TransactionType = "superchat_commission",
            ReferenceId = sc.Id.ToString(),
        });

        // Create match if not already exists
        var existingMatch = await _db.Matches.FirstOrDefaultAsync(m =>
            (m.User1Id == sc.FromUserId && m.User2Id == sc.ToUserId) ||
            (m.User1Id == sc.ToUserId && m.User2Id == sc.FromUserId));

        Match match;
        Chat chat;

        if (existingMatch == null)
        {
            match = new Match { User1Id = sc.FromUserId, User2Id = sc.ToUserId };
            chat = new Chat { MatchId = match.Id };
            _db.Matches.Add(match);
            _db.Chats.Add(chat);

            _db.Notifications.AddRange(
                new Notification
                {
                    UserId = sc.FromUserId,
                    Title = "🎉 SuperChat Match!",
                    Body = $"{sc.ToUser?.FullName} responded to your SuperChat!",
                    Type = "superchat_response",
                    ReferenceId = match.Id.ToString()
                },
                new Notification
                {
                    UserId = toUserId,
                    Title = $"💰 You earned {coinsToAward} coins!",
                    Body = $"{coinsToAward} coins credited to your wallet from SuperChat!",
                    Type = "commission"
                }
            );
        }
        else
        {
            match = existingMatch;
            chat = await _db.Chats.FirstOrDefaultAsync(c => c.MatchId == match.Id) ?? new Chat { MatchId = match.Id };
            if (chat.Id == Guid.Empty) { _db.Chats.Add(chat); }
        }

        sc.IsResponded = true;
        sc.RespondedAt = DateTime.UtcNow;
        sc.MatchCreatedId = match.Id;
        await _db.SaveChangesAsync();

        // Notify sender of the response
        await _hub.SendToUserAsync(sc.FromUserId.ToString(), "SuperChatResponded", new
        {
            superChatId = superChatId.ToString(),
            matchId = match.Id.ToString(),
            chatId = chat.Id.ToString(),
            user = new { id = sc.ToUser?.Id.ToString(), fullName = sc.ToUser?.FullName, avatar = sc.ToUser?.Avatar },
        });
        await _hub.SendToUserAsync(toUserId.ToString(), "NewMatch", new
        {
            matchId = match.Id.ToString(),
            chatId = chat.Id.ToString(),
            user = new { id = sc.FromUser?.Id.ToString(), fullName = sc.FromUser?.FullName, avatar = sc.FromUser?.Avatar },
        });

        return MapSc(sc, coinsToAward);
    }

    public async Task<List<SuperChatDto>> GetReceivedAsync(Guid userId)
    {
        var list = await _db.SuperChats.Include(s => s.FromUser)
            .Where(s => s.ToUserId == userId).OrderByDescending(s => s.CreatedAt).ToListAsync();
        return list.Select(s => MapSc(s)).ToList();
    }

    public async Task<List<SuperChatDto>> GetSentAsync(Guid userId)
    {
        var list = await _db.SuperChats.Include(s => s.ToUser)
            .Where(s => s.FromUserId == userId).OrderByDescending(s => s.CreatedAt).ToListAsync();
        return list.Select(s => MapSc(s)).ToList();
    }

    private static SuperChatDto MapSc(SuperChat s, int coinsAwarded = 0) => new()
    {
        Id = s.Id.ToString(),
        FromUserId = s.FromUserId.ToString(),
        ToUserId = s.ToUserId.ToString(),
        FromUserName = s.FromUser?.FullName,
        FromUserAvatar = s.FromUser?.Avatar,
        ToUserName = s.ToUser?.FullName,
        ToUserAvatar = s.ToUser?.Avatar,
        Message = s.Message,
        CoinAmount = s.CoinAmount,
        CoinsAwarded = coinsAwarded > 0 ? coinsAwarded : (s.IsResponded ? s.CoinAmount / 2 : 0),
        GirlCommission = s.GirlCommission,
        CompanyRevenue = s.CompanyRevenue,
        IsResponded = s.IsResponded,
        RespondedAt = s.RespondedAt,
        MatchCreatedId = s.MatchCreatedId?.ToString(),
        CreatedAt = s.CreatedAt,
    };
}


//using Microsoft.EntityFrameworkCore;
//using Mingley.Application.DTOs.SuperChat;
//using Mingley.Application.Interfaces;
//using Mingley.Domain.Entities;
//using Mingley.Infrastructure.Persistence;

//namespace Mingley.Infrastructure.Services;

//public class SuperChatService : ISuperChatService
//{
//    private readonly MingleyDbContext _db;
//    private readonly INotificationService _notifs;
//    private readonly IHubNotifier _hub;

//    public SuperChatService(MingleyDbContext db, INotificationService notifs, IHubNotifier hub)
//    { _db = db; _notifs = notifs; _hub = hub; }

//    public async Task<SendSuperChatResponse> SendAsync(Guid fromUserId, SendSuperChatRequest req)
//    {
//        if (!Guid.TryParse(req.ToUserId, out var toUserId))
//            throw new InvalidOperationException("Invalid target user ID.");
//        if (fromUserId == toUserId)
//            throw new InvalidOperationException("Cannot send SuperChat to yourself.");
//        if (string.IsNullOrWhiteSpace(req.Message))
//            throw new InvalidOperationException("Message is required.");

//        var sender = await _db.Users.FindAsync(fromUserId) ?? throw new InvalidOperationException("User not found.");
//        var target = await _db.Users.FindAsync(toUserId)   ?? throw new InvalidOperationException("Target user not found.");

//        if (sender.CoinBalance < MingleyDbContext.SuperChatCost)
//            throw new InvalidOperationException($"Need {MingleyDbContext.SuperChatCost} coins for SuperChat. You have {sender.CoinBalance}.");

//        // Deduct coins from sender
//        sender.CoinBalance -= MingleyDbContext.SuperChatCost;
//        sender.UpdatedAt    = DateTime.UtcNow;

//        // Calculate commission: 50% girl / 50% company
//        var totalInr       = MingleyDbContext.SuperChatCost * MingleyDbContext.CoinToInrRate;
//        var girlCommission = totalInr * MingleyDbContext.GirlCommissionPct;
//        var companyRevenue = totalInr - girlCommission;

//        // Credit commission to girl
//        target.TotalEarned += girlCommission;
//        target.UpdatedAt    = DateTime.UtcNow;

//        var sc = new SuperChat
//        {
//            FromUserId      = fromUserId,
//            ToUserId        = toUserId,
//            Message         = req.Message.Trim(),
//            CoinAmount      = MingleyDbContext.SuperChatCost,
//            GirlCommission  = girlCommission,
//            CompanyRevenue  = companyRevenue,
//        };
//        _db.SuperChats.Add(sc);

//        _db.CoinTransactions.Add(new CoinTransaction
//        {
//            UserId = fromUserId, Coins = MingleyDbContext.SuperChatCost, Direction = "debit",
//            Description = $"SuperChat to {target.FullName}", TransactionType = "superchat", ReferenceId = sc.Id.ToString(),
//        });

//        await _db.SaveChangesAsync();

//        // Real-time push
//        await _hub.SendToUserAsync(toUserId.ToString(), "NewSuperChat", new
//        {
//            superChatId = sc.Id.ToString(),
//            fromUserId  = fromUserId.ToString(),
//            fromName    = sender.FullName,
//            fromAvatar  = sender.Avatar,
//            message     = sc.Message,
//            coinAmount  = sc.CoinAmount,
//            commission  = girlCommission,
//            createdAt   = sc.CreatedAt,
//        });
//        await _notifs.CreateAsync(toUserId, $"⭐ SuperChat from {sender.FullName}!",
//            $"\"{req.Message.Substring(0, Math.Min(60, req.Message.Length))}\" — respond to match & earn ₹{girlCommission:F0}!",
//            "superchat", sc.Id.ToString());

//        return new SendSuperChatResponse
//        {
//            SuperChatId    = sc.Id.ToString(),
//            CoinsDeducted  = MingleyDbContext.SuperChatCost,
//            NewBalance     = sender.CoinBalance,
//            GirlCommission = girlCommission,
//            CompanyRevenue = companyRevenue,
//        };
//    }

//    public async Task<SuperChatDto> RespondAsync(Guid toUserId, Guid superChatId)
//    {
//        var sc = await _db.SuperChats.Include(s => s.FromUser).Include(s => s.ToUser)
//            .FirstOrDefaultAsync(s => s.Id == superChatId && s.ToUserId == toUserId)
//            ?? throw new InvalidOperationException("SuperChat not found.");

//        if (sc.IsResponded)
//            throw new InvalidOperationException("You already responded to this SuperChat.");

//        // Create match if not already exists
//        var existingMatch = await _db.Matches.FirstOrDefaultAsync(m =>
//            (m.User1Id == sc.FromUserId && m.User2Id == sc.ToUserId) ||
//            (m.User1Id == sc.ToUserId   && m.User2Id == sc.FromUserId));

//        Match match;
//        Chat chat;

//        if (existingMatch == null)
//        {
//            match = new Match { User1Id = sc.FromUserId, User2Id = sc.ToUserId };
//            chat  = new Chat  { MatchId = match.Id };
//            _db.Matches.Add(match);
//            _db.Chats.Add(chat);

//            _db.Notifications.AddRange(
//                new Notification { UserId = sc.FromUserId, Title = "🎉 SuperChat Match!", Body = $"{sc.ToUser?.FullName} responded to your SuperChat!", Type = "superchat_response", ReferenceId = match.Id.ToString() },
//                new Notification { UserId = toUserId, Title = "💰 You earned a commission!", Body = $"₹{sc.GirlCommission:F0} credited to your earnings!", Type = "commission" }
//            );
//        }
//        else
//        {
//            match = existingMatch;
//            chat  = await _db.Chats.FirstOrDefaultAsync(c => c.MatchId == match.Id) ?? new Chat { MatchId = match.Id };
//            if (chat.Id == Guid.Empty) { _db.Chats.Add(chat); }
//        }

//        sc.IsResponded    = true;
//        sc.RespondedAt    = DateTime.UtcNow;
//        sc.MatchCreatedId = match.Id;
//        await _db.SaveChangesAsync();

//        // Notify sender
//        await _hub.SendToUserAsync(sc.FromUserId.ToString(), "SuperChatResponded", new
//        {
//            superChatId = superChatId.ToString(), matchId = match.Id.ToString(), chatId = chat.Id.ToString(),
//            user = new { id = sc.ToUser?.Id.ToString(), fullName = sc.ToUser?.FullName, avatar = sc.ToUser?.Avatar },
//        });
//        await _hub.SendToUserAsync(toUserId.ToString(), "NewMatch", new
//        {
//            matchId = match.Id.ToString(), chatId = chat.Id.ToString(),
//            user = new { id = sc.FromUser?.Id.ToString(), fullName = sc.FromUser?.FullName, avatar = sc.FromUser?.Avatar },
//        });

//        return MapSc(sc);
//    }

//    public async Task<List<SuperChatDto>> GetReceivedAsync(Guid userId)
//    {
//        var list = await _db.SuperChats.Include(s => s.FromUser)
//            .Where(s => s.ToUserId == userId).OrderByDescending(s => s.CreatedAt).ToListAsync();
//        return list.Select(MapSc).ToList();
//    }

//    public async Task<List<SuperChatDto>> GetSentAsync(Guid userId)
//    {
//        var list = await _db.SuperChats.Include(s => s.ToUser)
//            .Where(s => s.FromUserId == userId).OrderByDescending(s => s.CreatedAt).ToListAsync();
//        return list.Select(MapSc).ToList();
//    }

//    private static SuperChatDto MapSc(SuperChat s) => new()
//    {
//        Id = s.Id.ToString(), FromUserId = s.FromUserId.ToString(), ToUserId = s.ToUserId.ToString(),
//        FromUserName = s.FromUser?.FullName, FromUserAvatar = s.FromUser?.Avatar,
//        ToUserName   = s.ToUser?.FullName,   ToUserAvatar   = s.ToUser?.Avatar,
//        Message = s.Message, CoinAmount = s.CoinAmount,
//        GirlCommission = s.GirlCommission, CompanyRevenue = s.CompanyRevenue,
//        IsResponded = s.IsResponded, RespondedAt = s.RespondedAt,
//        MatchCreatedId = s.MatchCreatedId?.ToString(), CreatedAt = s.CreatedAt,
//    };
//}
