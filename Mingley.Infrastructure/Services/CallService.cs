using Microsoft.EntityFrameworkCore;
using Mingley.Application.Interfaces;
using Mingley.Domain.Entities;
using Mingley.Infrastructure.Persistence;

namespace Mingley.Infrastructure.Services;

public class CallService : ICallService
{
    private readonly MingleyDbContext _db;
    private readonly IHubNotifier _hub;
    private readonly INotificationService _notifs;

    public CallService(MingleyDbContext db, IHubNotifier hub, INotificationService notifs)
    { _db = db; _hub = hub; _notifs = notifs; }

    public async Task<object> InitiateCallAsync(Guid callerId, string targetId, string callType)
    {
        if (!Guid.TryParse(targetId, out var tid))
            throw new InvalidOperationException("Invalid target ID.");

        var caller = await _db.Users.FindAsync(callerId) ?? throw new InvalidOperationException("User not found.");
        var target = await _db.Users.FindAsync(tid) ?? throw new InvalidOperationException("Target user not found.");

        // Must be matched
        var match = await _db.Matches.FirstOrDefaultAsync(m =>
            m.IsActive && !m.IsDeleted &&
            ((m.User1Id == callerId && m.User2Id == tid) ||
             (m.User1Id == tid && m.User2Id == callerId)))
            ?? throw new InvalidOperationException("You can only call your matches.");

        // Video call requires premium
        if (callType == "video" && !caller.IsPremium)
            throw new InvalidOperationException("Video calls require a premium subscription.");

        // Check balance for audio call
        var ratePerMin = callType == "video" ? MingleyDbContext.VideoCallCoinPerMin : MingleyDbContext.AudioCallCoinPerMin;
        if (caller.CoinBalance < ratePerMin)
            throw new InvalidOperationException($"Insufficient coins. Need at least {ratePerMin} coins to start a call.");

        var session = new CallSession
        {
            CallerId   = callerId,
            ReceiverId = tid,
            MatchId    = match.Id,
            CallType   = callType,
            Status     = "ringing",
        };
        _db.CallSessions.Add(session);
        await _db.SaveChangesAsync();

        await _hub.SendToUserAsync(tid.ToString(), "IncomingCall", new
        {
            callId   = session.Id.ToString(),
            callType = session.CallType,
            matchId  = match.Id.ToString(),
            caller   = new { id = caller.Id.ToString(), fullName = caller.FullName, avatar = caller.Avatar },
        });

        await _notifs.CreateAsync(tid, $"📞 Incoming {callType} call", $"{caller.FullName} is calling you", "call", session.Id.ToString());

        return new
        {
            callId      = session.Id.ToString(),
            callType    = session.CallType,
            status      = "ringing",
            costPerMin  = ratePerMin,
            matchId     = match.Id.ToString(),
            target      = new { id = target.Id.ToString(), fullName = target.FullName, avatar = target.Avatar },
        };
    }

    public async Task<object> AnswerCallAsync(Guid receiverId, Guid callId)
    {
        var session = await _db.CallSessions.Include(c => c.Caller)
            .FirstOrDefaultAsync(c => c.Id == callId)
            ?? throw new InvalidOperationException("Call not found.");

        if (session.ReceiverId != receiverId)
            throw new InvalidOperationException("Not authorized to answer this call.");

        if (session.Status != "ringing")
            throw new InvalidOperationException("Call is no longer available.");

        session.Status     = "active";
        session.AnsweredAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _hub.SendToUserAsync(session.CallerId.ToString(), "CallAnswered", new
        { callId = callId.ToString(), answeredAt = session.AnsweredAt });

        return new { callId = callId.ToString(), status = "active", answeredAt = session.AnsweredAt };
    }

    public async Task<object> EndCallAsync(Guid userId, Guid callId)
    {
        var session = await _db.CallSessions.Include(c => c.Caller)
            .FirstOrDefaultAsync(c => c.Id == callId && (c.CallerId == userId || c.ReceiverId == userId))
            ?? throw new InvalidOperationException("Call not found.");

        if (session.Status == "ended") return new { callId = callId.ToString(), status = "ended" };

        session.Status    = "ended";
        session.EndedAt   = DateTime.UtcNow;
        session.EndReason = "user_ended";

        int coinsDeducted = 0;
        if (session.AnsweredAt.HasValue)
        {
            var durationSecs    = (int)(DateTime.UtcNow - session.AnsweredAt.Value).TotalSeconds;
            var durationMins    = (int)Math.Ceiling(durationSecs / 60.0);
            session.DurationSeconds = durationSecs;

            var ratePerMin = session.CallType == "video"
                ? MingleyDbContext.VideoCallCoinPerMin
                : MingleyDbContext.AudioCallCoinPerMin;
            coinsDeducted = durationMins * ratePerMin;

            if (session.Caller != null)
            {
                var actualDeducted = Math.Min(coinsDeducted, session.Caller.CoinBalance);
                session.Caller.CoinBalance -= actualDeducted;
                session.CoinsDeducted = actualDeducted;

                _db.CoinTransactions.Add(new CoinTransaction
                {
                    UserId = session.CallerId, Coins = actualDeducted, Direction = "debit",
                    Description = $"{session.CallType} call · {durationMins} min", TransactionType = "call",
                    ReferenceId = callId.ToString(),
                });
                coinsDeducted = actualDeducted;
            }
        }

        await _db.SaveChangesAsync();

        var otherId = session.CallerId == userId ? session.ReceiverId : session.CallerId;
        var payload = new
        {
            callId = callId.ToString(), duration = session.DurationSeconds,
            coinsDeducted, newBalance = session.Caller?.CoinBalance,
        };
        await _hub.SendToUserAsync(otherId.ToString(), "CallEnded", payload);
        await _hub.SendToUserAsync(userId.ToString(), "CallEnded", payload);

        // Add system message to chat
        var chat = await _db.Chats.FirstOrDefaultAsync(c => c.MatchId == session.MatchId);
        if (chat != null)
        {
            var dur = session.DurationSeconds.HasValue
                ? $"{session.DurationSeconds/60:D2}:{session.DurationSeconds%60:D2}" : "0:00";
            _db.Messages.Add(new Message
            {
                ChatId = chat.Id, SenderId = session.CallerId,
                Text = $"{(session.CallType=="video"?"📹":"📞")} {session.CallType} call ended · {dur}",
                Type = "system", CoinsDeducted = 0,
            });
            await _db.SaveChangesAsync();
        }

        return payload;
    }

    public async Task DeclineCallAsync(Guid receiverId, Guid callId)
    {
        var session = await _db.CallSessions.FindAsync(callId)
            ?? throw new InvalidOperationException("Call not found.");
        if (session.ReceiverId != receiverId) throw new InvalidOperationException("Not authorized.");
        session.Status  = "declined";
        session.EndedAt = DateTime.UtcNow;
        session.EndReason = "declined";
        await _db.SaveChangesAsync();
        await _hub.SendToUserAsync(session.CallerId.ToString(), "CallDeclined", new { callId = callId.ToString() });
    }

    public async Task<List<object>> GetHistoryAsync(Guid userId)
    {
        var calls = await _db.CallSessions
            .Include(c => c.Caller).Include(c => c.Receiver)
            .Where(c => c.CallerId == userId || c.ReceiverId == userId)
            .OrderByDescending(c => c.CreatedAt).Take(50).ToListAsync();
        return calls.Select(c => (object)new
        {
            id = c.Id.ToString(), c.CallType, c.Status, c.DurationSeconds, c.CoinsDeducted, c.CreatedAt,
            caller   = new { id = c.Caller?.Id.ToString(),  fullName = c.Caller?.FullName,  avatar = c.Caller?.Avatar  },
            receiver = new { id = c.Receiver?.Id.ToString(), fullName = c.Receiver?.FullName, avatar = c.Receiver?.Avatar },
        }).ToList();
    }
}
