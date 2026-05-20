using Microsoft.EntityFrameworkCore;
using Mingley.Application.DTOs.Chat;
using Mingley.Application.Interfaces;
using Mingley.Domain.Entities;
using Mingley.Infrastructure.Persistence;

namespace Mingley.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly MingleyDbContext _db;
    private readonly IWalletService _wallet;
    private readonly IHubNotifier _hub;
    private readonly INotificationService _notifs;

    public ChatService(MingleyDbContext db, IWalletService wallet, IHubNotifier hub, INotificationService notifs)
    { _db = db; _wallet = wallet; _hub = hub; _notifs = notifs; }

    public async Task<List<ChatListItemDto>> GetChatsAsync(Guid userId)
    {
        var chats = await _db.Chats
            .Include(c => c.Match).ThenInclude(m => m.User1)
            .Include(c => c.Match).ThenInclude(m => m.User2)
            .Include(c => c.Messages)
            .Where(c => !c.IsDeleted && c.Match.IsActive
                     && (c.Match.User1Id == userId || c.Match.User2Id == userId))
            .ToListAsync();

        return chats
            .OrderByDescending(c => c.Messages.Where(m => !m.IsDeleted).Max(m => (DateTime?)m.CreatedAt) ?? c.CreatedAt)
            .Select(c =>
            {
                var other = c.Match.User1Id == userId ? c.Match.User2! : c.Match.User1!;
                var msgs = c.Messages.Where(m => !m.IsDeleted).OrderByDescending(m => m.CreatedAt).ToList();
                var lastMsg = msgs.FirstOrDefault();
                var unread = msgs.Count(m => m.SenderId != userId && m.ReadAt == null);
                return new ChatListItemDto
                {
                    ChatId = c.Id.ToString(),
                    MatchId = c.Id.ToString(),
                    UnreadCount = unread,
                    Participant = new ChatParticipantDto
                    {
                        Id = other.Id.ToString(),
                        FullName = other.FullName,
                        Avatar = other.Avatar,
                        IsOnline = other.IsOnline,
                        LastActiveAt = other.LastActiveAt,
                    },
                    LastMessage = lastMsg == null ? null : MapMsg(lastMsg),
                };
            }).ToList();
    }

    public async Task<List<ChatMessageDto>> GetMessagesAsync(Guid userId, Guid chatId, int page)
    {
        var chat = await _db.Chats.Include(c => c.Match)
            .FirstOrDefaultAsync(c => c.Id == chatId && !c.IsDeleted
                && (c.Match.User1Id == userId || c.Match.User2Id == userId))
            ?? throw new InvalidOperationException("Chat not found or access denied.");

        const int ps = 50;
        var messages = await _db.Messages
            .Include(m => m.Sender)
            .Include(m => m.ReplyToMessage)
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * ps).Take(ps)
            .ToListAsync();

        return messages.Select(MapMsg).ToList();
    }

    public async Task<SendMessageResponse> SendMessageAsync(Guid senderId, Guid chatId, SendMessageRequest req)
    {
        var text = req.ContentText;
        if (string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(req.ImageUrl))
            throw new InvalidOperationException("Message content is required.");

        var chat = await _db.Chats.Include(c => c.Match)
            .FirstOrDefaultAsync(c => c.Id == chatId && !c.IsDeleted
                && (c.Match.User1Id == senderId || c.Match.User2Id == senderId))
            ?? throw new InvalidOperationException("Chat not found or you are not a participant.");

        var sender = await _db.Users.FindAsync(senderId)
            ?? throw new InvalidOperationException("User not found.");

        int coinsDeducted = 0;

        if (sender.Gender?.ToLower() == "male")
        {
            var cost = sender.IsPremium ? MingleyDbContext.MalePremiumCostPerMsg : MingleyDbContext.MaleCostPerMessage;
            if (sender.CoinBalance < cost)
                throw new InvalidOperationException($"Insufficient coins. Need {cost} coins. Top up your wallet.");
            await _wallet.DeductCoinsAsync(senderId, cost, "Message sent", "message", chatId.ToString());
            coinsDeducted = cost;
        }
        else
        {
            var sentCount = await _db.Messages.CountAsync(m => m.ChatId == chatId && m.SenderId == senderId && !m.IsDeleted);
            if (sentCount >= MingleyDbContext.FemaleFreeMessages)
            {
                if (sender.CoinBalance < MingleyDbContext.FemaleMessageCost)
                    throw new InvalidOperationException($"Insufficient coins. Need {MingleyDbContext.FemaleMessageCost} coins.");
                await _wallet.DeductCoinsAsync(senderId, MingleyDbContext.FemaleMessageCost, "Message sent", "message", chatId.ToString());
                coinsDeducted = MingleyDbContext.FemaleMessageCost;
            }
        }

        Guid? replyId = null;
        if (!string.IsNullOrWhiteSpace(req.ReplyToMessageId) && Guid.TryParse(req.ReplyToMessageId, out var rid))
        {
            var exists = await _db.Messages.AnyAsync(m => m.Id == rid && m.ChatId == chatId);
            if (exists) replyId = rid;
        }

        var msg = new Message
        {
            ChatId = chatId,
            SenderId = senderId,
            Text = req.ImageUrl != null ? null : text,
            Type = req.Type,
            ImageUrl = req.ImageUrl,
            CoinsDeducted = coinsDeducted,
            ReplyToMessageId = replyId,
        };
        _db.Messages.Add(msg);
        await _db.SaveChangesAsync();

        await _db.Entry(msg).Reference(m => m.Sender).LoadAsync();
        if (replyId.HasValue) await _db.Entry(msg).Reference(m => m.ReplyToMessage).LoadAsync();

        var dto = MapMsg(msg);

        await _hub.SendToGroupAsync($"chat_{chatId}", "NewMessage", new
        {
            matchId = chatId.ToString(),
            chatId = chatId.ToString(),
            message = dto
        });

        var otherId = chat.Match.User1Id == senderId ? chat.Match.User2Id : chat.Match.User1Id;
        var other = await _db.Users.FindAsync(otherId);
        if (other is { IsOnline: false })
            await _notifs.CreateAsync(otherId, sender.FullName ?? "New message",
                req.ImageUrl != null ? "📷 Sent a photo" : (text?.Length > 60 ? text[..60] + "…" : text) ?? "",
                "message", chatId.ToString());

        var updatedBalance = (await _db.Users.FindAsync(senderId))?.CoinBalance ?? 0;
        var remaining = await GetRemainingQuota(senderId, chatId, sender.Gender ?? "");
        return new SendMessageResponse
        {
            Id = msg.Id.ToString(),
            CoinsDeducted = coinsDeducted,
            NewBalance = updatedBalance,
            Remaining = remaining,
            Message = dto,
        };
    }

    public async Task MarkReadAsync(Guid userId, Guid chatId)
    {
        var msgs = await _db.Messages
            .Where(m => m.ChatId == chatId && m.SenderId != userId && m.ReadAt == null && !m.IsDeleted)
            .ToListAsync();
        if (!msgs.Any()) return;
        foreach (var m in msgs) m.ReadAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _hub.SendToGroupAsync($"chat_{chatId}", "MessagesRead", new
        {
            matchId = chatId.ToString(),
            chatId = chatId.ToString(),
            readBy = userId.ToString()
        });
    }

    public async Task DeleteMessageAsync(Guid userId, Guid chatId, Guid messageId)
    {
        var msg = await _db.Messages
            .FirstOrDefaultAsync(m => m.Id == messageId && m.ChatId == chatId && m.SenderId == userId && !m.IsDeleted)
            ?? throw new InvalidOperationException("Message not found or you cannot delete it.");
        msg.IsDeleted = true;
        msg.DeletedAt = DateTime.UtcNow;
        msg.Text = null;
        msg.ImageUrl = null;
        await _db.SaveChangesAsync();
        await _hub.SendToGroupAsync($"chat_{chatId}", "MessageDeleted", new
        {
            matchId = chatId.ToString(),
            chatId = chatId.ToString(),
            messageId = messageId.ToString()
        });
    }

    public async Task<ChatQuotaDto> GetQuotaAsync(Guid userId, Guid chatId)
    {
        var user = await _db.Users.FindAsync(userId) ?? throw new InvalidOperationException("User not found.");
        int free = 0, remaining, cost;
        if (user.Gender?.ToLower() == "male")
        {
            cost = user.IsPremium ? MingleyDbContext.MalePremiumCostPerMsg : MingleyDbContext.MaleCostPerMessage;
            remaining = cost > 0 ? user.CoinBalance / cost : 9999;
        }
        else
        {
            var sent = await _db.Messages.CountAsync(m => m.ChatId == chatId && m.SenderId == userId && !m.IsDeleted);
            cost = MingleyDbContext.FemaleMessageCost;
            free = Math.Max(0, MingleyDbContext.FemaleFreeMessages - sent);
            remaining = free > 0 ? free : (cost > 0 ? user.CoinBalance / cost : 9999);
        }
        return new ChatQuotaDto { FreeRemaining = free, Remaining = remaining, IsPremium = user.IsPremium, CostPerMessage = cost };
    }

    // NEW: Transfer coins to the other person in a chat
    public async Task<SendCoinsResponse> SendCoinsAsync(Guid senderId, Guid chatId, SendCoinsRequest req)
    {
        if (req.CoinAmount <= 0)
            throw new InvalidOperationException("Coin amount must be greater than 0.");

        var chat = await _db.Chats.Include(c => c.Match)
            .FirstOrDefaultAsync(c => c.Id == chatId && !c.IsDeleted
                && (c.Match.User1Id == senderId || c.Match.User2Id == senderId))
            ?? throw new InvalidOperationException("Chat not found or you are not a participant.");

        var sender = await _db.Users.FindAsync(senderId)
            ?? throw new InvalidOperationException("Sender not found.");

        if (sender.CoinBalance < req.CoinAmount)
            throw new InvalidOperationException(
                $"Insufficient coins. You have {sender.CoinBalance}, need {req.CoinAmount}.");

        var receiverId = chat.Match.User1Id == senderId ? chat.Match.User2Id : chat.Match.User1Id;
        var receiver = await _db.Users.FindAsync(receiverId)
            ?? throw new InvalidOperationException("Receiver not found.");

        sender.CoinBalance -= req.CoinAmount;
        sender.UpdatedAt = DateTime.UtcNow;
        receiver.CoinBalance += req.CoinAmount;
        receiver.UpdatedAt = DateTime.UtcNow;

        _db.CoinTransactions.Add(new CoinTransaction
        {
            UserId = senderId,
            Coins = req.CoinAmount,
            Direction = "debit",
            Description = $"Coins sent to {receiver.FullName}",
            TransactionType = "coin_gift",
            ReferenceId = chatId.ToString(),
        });
        _db.CoinTransactions.Add(new CoinTransaction
        {
            UserId = receiverId,
            Coins = req.CoinAmount,
            Direction = "credit",
            Description = $"Coins received from {sender.FullName}",
            TransactionType = "coin_gift",
            ReferenceId = chatId.ToString(),
        });

        var msgText = string.IsNullOrWhiteSpace(req.Message)
            ? $"💰 Sent {req.CoinAmount} coins!"
            : req.Message;

        var msg = new Message
        {
            ChatId = chatId,
            SenderId = senderId,
            Text = msgText,
            Type = "coins",
            CoinAmount = req.CoinAmount,
            CoinsDeducted = req.CoinAmount,
        };
        _db.Messages.Add(msg);
        await _db.SaveChangesAsync();

        await _db.Entry(msg).Reference(m => m.Sender).LoadAsync();
        var dto = MapMsg(msg);

        await _hub.SendToGroupAsync($"chat_{chatId}", "NewMessage", new
        {
            matchId = chatId.ToString(),
            chatId = chatId.ToString(),
            message = dto
        });

        await _notifs.CreateAsync(receiverId,
            $"💰 {sender.FullName} sent you coins!",
            $"You received {req.CoinAmount} coins!",
            "coin_gift", chatId.ToString());

        return new SendCoinsResponse
        {
            MessageId = msg.Id.ToString(),
            CoinsDeducted = req.CoinAmount,
            SenderNewBalance = sender.CoinBalance,
            Message = dto,
        };
    }

    private static ChatMessageDto MapMsg(Message m) => new()
    {
        Id = m.Id.ToString(),
        ChatId = m.ChatId.ToString(),
        SenderId = m.SenderId.ToString(),
        SenderName = m.Sender?.FullName,
        SenderAvatar = m.Sender?.Avatar,
        Text = m.IsDeleted ? null
                        : m.Type == "coins" ? $"💰 Sent {m.CoinAmount} coins!"
                        : (m.Text ?? (m.ImageUrl != null ? "[image]" : null)),
        MessageType = m.IsDeleted ? "deleted" : (m.Type?.ToUpper() ?? "TEXT"),
        ImageUrl = m.IsDeleted ? null : m.ImageUrl,
        GiftName = m.GiftName,
        GiftCost = m.GiftCost,
        CoinAmount = m.CoinAmount,
        CreatedAt = m.CreatedAt,
        ReadAt = m.ReadAt,
        CoinsDeducted = m.CoinsDeducted,
        IsDeleted = m.IsDeleted,
        ReplyToMessageId = m.ReplyToMessageId?.ToString(),
        ReplyToText = m.ReplyToMessage?.Text,
    };

    private async Task<int> GetRemainingQuota(Guid userId, Guid chatId, string gender)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return 0;
        if (gender.ToLower() == "male")
        {
            var cost = user.IsPremium ? MingleyDbContext.MalePremiumCostPerMsg : MingleyDbContext.MaleCostPerMessage;
            return cost > 0 ? user.CoinBalance / cost : 9999;
        }
        var sent = await _db.Messages.CountAsync(m => m.ChatId == chatId && m.SenderId == userId && !m.IsDeleted);
        var free = Math.Max(0, MingleyDbContext.FemaleFreeMessages - sent);
        return free > 0 ? free : (MingleyDbContext.FemaleMessageCost > 0 ? user.CoinBalance / MingleyDbContext.FemaleMessageCost : 9999);
    }
}


//using Microsoft.EntityFrameworkCore;
//using Mingley.Application.DTOs.Chat;
//using Mingley.Application.Interfaces;
//using Mingley.Domain.Entities;
//using Mingley.Infrastructure.Persistence;

//namespace Mingley.Infrastructure.Services;

//public class ChatService : IChatService
//{
//    private readonly MingleyDbContext _db;
//    private readonly IWalletService _wallet;
//    private readonly IHubNotifier _hub;
//    private readonly INotificationService _notifs;

//    public ChatService(MingleyDbContext db, IWalletService wallet, IHubNotifier hub, INotificationService notifs)
//    { _db = db; _wallet = wallet; _hub = hub; _notifs = notifs; }

//    public async Task<List<ChatListItemDto>> GetChatsAsync(Guid userId)
//    {
//        var chats = await _db.Chats
//            .Include(c => c.Match).ThenInclude(m => m.User1)
//            .Include(c => c.Match).ThenInclude(m => m.User2)
//            .Include(c => c.Messages)
//            .Where(c => !c.IsDeleted && c.Match.IsActive
//                     && (c.Match.User1Id == userId || c.Match.User2Id == userId))
//            .ToListAsync();

//        return chats
//            .OrderByDescending(c => c.Messages.Where(m => !m.IsDeleted).Max(m => (DateTime?)m.CreatedAt) ?? c.CreatedAt)
//            .Select(c =>
//            {
//                var other   = c.Match.User1Id == userId ? c.Match.User2! : c.Match.User1!;
//                var msgs    = c.Messages.Where(m => !m.IsDeleted).OrderByDescending(m => m.CreatedAt).ToList();
//                var lastMsg = msgs.FirstOrDefault();
//                var unread  = msgs.Count(m => m.SenderId != userId && m.ReadAt == null);
//                return new ChatListItemDto
//                {
//                    ChatId  = c.Id.ToString(),
//                    MatchId = c.Id.ToString(),  // FIX: frontend uses matchId to key messages
//                    UnreadCount = unread,
//                    Participant = new ChatParticipantDto
//                    {
//                        Id           = other.Id.ToString(),
//                        FullName     = other.FullName,
//                        Avatar       = other.Avatar,
//                        IsOnline     = other.IsOnline,
//                        LastActiveAt = other.LastActiveAt,
//                    },
//                    LastMessage = lastMsg == null ? null : MapMsg(lastMsg),
//                };
//            }).ToList();
//    }

//    public async Task<List<ChatMessageDto>> GetMessagesAsync(Guid userId, Guid chatId, int page)
//    {
//        var chat = await _db.Chats.Include(c => c.Match)
//            .FirstOrDefaultAsync(c => c.Id == chatId && !c.IsDeleted
//                && (c.Match.User1Id == userId || c.Match.User2Id == userId))
//            ?? throw new InvalidOperationException("Chat not found or access denied.");

//        const int ps = 50;
//        var messages = await _db.Messages
//            .Include(m => m.Sender)
//            .Include(m => m.ReplyToMessage)
//            .Where(m => m.ChatId == chatId)
//            .OrderByDescending(m => m.CreatedAt)
//            .Skip((page - 1) * ps).Take(ps)
//            .ToListAsync();

//        return messages.Select(MapMsg).ToList();
//    }

//    public async Task<SendMessageResponse> SendMessageAsync(Guid senderId, Guid chatId, SendMessageRequest req)
//    {
//        var text = req.ContentText;
//        if (string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(req.ImageUrl))
//            throw new InvalidOperationException("Message content is required.");

//        var chat = await _db.Chats.Include(c => c.Match)
//            .FirstOrDefaultAsync(c => c.Id == chatId && !c.IsDeleted
//                && (c.Match.User1Id == senderId || c.Match.User2Id == senderId))
//            ?? throw new InvalidOperationException("Chat not found or you are not a participant.");

//        var sender = await _db.Users.FindAsync(senderId)
//            ?? throw new InvalidOperationException("User not found.");

//        int coinsDeducted = 0;

//        if (sender.Gender?.ToLower() == "male")
//        {
//            var cost = sender.IsPremium ? MingleyDbContext.MalePremiumCostPerMsg : MingleyDbContext.MaleCostPerMessage;
//            if (sender.CoinBalance < cost)
//                throw new InvalidOperationException($"Insufficient coins. Need {cost} coins. Top up your wallet.");
//            await _wallet.DeductCoinsAsync(senderId, cost, "Message sent", "message", chatId.ToString());
//            coinsDeducted = cost;
//        }
//        else
//        {
//            var sentCount = await _db.Messages.CountAsync(m => m.ChatId == chatId && m.SenderId == senderId && !m.IsDeleted);
//            if (sentCount >= MingleyDbContext.FemaleFreeMessages)
//            {
//                if (sender.CoinBalance < MingleyDbContext.FemaleMessageCost)
//                    throw new InvalidOperationException($"Insufficient coins. Need {MingleyDbContext.FemaleMessageCost} coins.");
//                await _wallet.DeductCoinsAsync(senderId, MingleyDbContext.FemaleMessageCost, "Message sent", "message", chatId.ToString());
//                coinsDeducted = MingleyDbContext.FemaleMessageCost;
//            }
//        }

//        Guid? replyId = null;
//        if (!string.IsNullOrWhiteSpace(req.ReplyToMessageId) && Guid.TryParse(req.ReplyToMessageId, out var rid))
//        {
//            var exists = await _db.Messages.AnyAsync(m => m.Id == rid && m.ChatId == chatId);
//            if (exists) replyId = rid;
//        }

//        var msgType = req.Type; // text | image | gift
//        var msg = new Message
//        {
//            ChatId            = chatId,
//            SenderId          = senderId,
//            Text              = req.ImageUrl != null ? null : text,
//            Type              = msgType,
//            ImageUrl          = req.ImageUrl,
//            CoinsDeducted     = coinsDeducted,
//            ReplyToMessageId  = replyId,
//        };
//        _db.Messages.Add(msg);
//        await _db.SaveChangesAsync();

//        await _db.Entry(msg).Reference(m => m.Sender).LoadAsync();
//        if (replyId.HasValue) await _db.Entry(msg).Reference(m => m.ReplyToMessage).LoadAsync();

//        var dto = MapMsg(msg);

//        // Real-time push — use chat group
//        await _hub.SendToGroupAsync($"chat_{chatId}", "NewMessage", new
//        {
//            matchId = chatId.ToString(),
//            chatId  = chatId.ToString(),
//            message = dto
//        });

//        // Push notification if other user is offline
//        var otherId = chat.Match.User1Id == senderId ? chat.Match.User2Id : chat.Match.User1Id;
//        var other   = await _db.Users.FindAsync(otherId);
//        if (other is { IsOnline: false })
//            await _notifs.CreateAsync(otherId, sender.FullName ?? "New message",
//                req.ImageUrl != null ? "📷 Sent a photo" : (text?.Length > 60 ? text[..60] + "…" : text) ?? "",
//                "message", chatId.ToString());

//        var updatedBalance = (await _db.Users.FindAsync(senderId))?.CoinBalance ?? 0;
//        var remaining = await GetRemainingQuota(senderId, chatId, sender.Gender ?? "");
//        return new SendMessageResponse
//        {
//            Id             = msg.Id.ToString(),
//            CoinsDeducted  = coinsDeducted,
//            NewBalance     = updatedBalance,
//            Remaining      = remaining,
//            Message        = dto,
//        };
//    }

//    public async Task MarkReadAsync(Guid userId, Guid chatId)
//    {
//        var msgs = await _db.Messages
//            .Where(m => m.ChatId == chatId && m.SenderId != userId && m.ReadAt == null && !m.IsDeleted)
//            .ToListAsync();
//        if (!msgs.Any()) return;
//        foreach (var m in msgs) m.ReadAt = DateTime.UtcNow;
//        await _db.SaveChangesAsync();
//        await _hub.SendToGroupAsync($"chat_{chatId}", "MessagesRead", new
//        {
//            matchId = chatId.ToString(),
//            chatId  = chatId.ToString(),
//            readBy  = userId.ToString()
//        });
//    }

//    public async Task DeleteMessageAsync(Guid userId, Guid chatId, Guid messageId)
//    {
//        var msg = await _db.Messages
//            .FirstOrDefaultAsync(m => m.Id == messageId && m.ChatId == chatId && m.SenderId == userId && !m.IsDeleted)
//            ?? throw new InvalidOperationException("Message not found or you cannot delete it.");
//        msg.IsDeleted = true;
//        msg.DeletedAt = DateTime.UtcNow;
//        msg.Text      = null;
//        msg.ImageUrl  = null;
//        await _db.SaveChangesAsync();
//        await _hub.SendToGroupAsync($"chat_{chatId}", "MessageDeleted", new
//        {
//            matchId   = chatId.ToString(),
//            chatId    = chatId.ToString(),
//            messageId = messageId.ToString()
//        });
//    }

//    public async Task<ChatQuotaDto> GetQuotaAsync(Guid userId, Guid chatId)
//    {
//        var user = await _db.Users.FindAsync(userId) ?? throw new InvalidOperationException("User not found.");
//        int free = 0, remaining, cost;
//        if (user.Gender?.ToLower() == "male")
//        {
//            cost      = user.IsPremium ? MingleyDbContext.MalePremiumCostPerMsg : MingleyDbContext.MaleCostPerMessage;
//            remaining = cost > 0 ? user.CoinBalance / cost : 9999;
//        }
//        else
//        {
//            var sent  = await _db.Messages.CountAsync(m => m.ChatId == chatId && m.SenderId == userId && !m.IsDeleted);
//            cost      = MingleyDbContext.FemaleMessageCost;
//            free      = Math.Max(0, MingleyDbContext.FemaleFreeMessages - sent);
//            remaining = free > 0 ? free : (cost > 0 ? user.CoinBalance / cost : 9999);
//        }
//        return new ChatQuotaDto { FreeRemaining = free, Remaining = remaining, IsPremium = user.IsPremium, CostPerMessage = cost };
//    }

//    private static ChatMessageDto MapMsg(Message m) => new()
//    {
//        Id              = m.Id.ToString(),
//        ChatId          = m.ChatId.ToString(),
//        SenderId        = m.SenderId.ToString(),
//        SenderName      = m.Sender?.FullName,
//        SenderAvatar    = m.Sender?.Avatar,
//        Text            = m.IsDeleted ? null : (m.Text ?? (m.ImageUrl != null ? "[image]" : null)),
//        MessageType     = m.IsDeleted ? "deleted" : (m.Type?.ToUpper() ?? "TEXT"),
//        ImageUrl        = m.IsDeleted ? null : m.ImageUrl,
//        GiftName        = m.GiftName,
//        GiftCost        = m.GiftCost,
//        CoinAmount      = m.CoinAmount,
//        CreatedAt       = m.CreatedAt,
//        ReadAt          = m.ReadAt,
//        CoinsDeducted   = m.CoinsDeducted,
//        IsDeleted       = m.IsDeleted,
//        ReplyToMessageId = m.ReplyToMessageId?.ToString(),
//        ReplyToText      = m.ReplyToMessage?.Text,
//    };

//    private async Task<int> GetRemainingQuota(Guid userId, Guid chatId, string gender)
//    {
//        var user = await _db.Users.FindAsync(userId);
//        if (user == null) return 0;
//        if (gender.ToLower() == "male")
//        {
//            var cost = user.IsPremium ? MingleyDbContext.MalePremiumCostPerMsg : MingleyDbContext.MaleCostPerMessage;
//            return cost > 0 ? user.CoinBalance / cost : 9999;
//        }
//        var sent = await _db.Messages.CountAsync(m => m.ChatId == chatId && m.SenderId == userId && !m.IsDeleted);
//        var free = Math.Max(0, MingleyDbContext.FemaleFreeMessages - sent);
//        return free > 0 ? free : (MingleyDbContext.FemaleMessageCost > 0 ? user.CoinBalance / MingleyDbContext.FemaleMessageCost : 9999);
//    }
//}
