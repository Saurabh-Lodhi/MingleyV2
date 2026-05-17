using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mingley.Application.DTOs.Common;
using Mingley.Domain.Entities;
using Mingley.Infrastructure.Persistence;
using System.Security.Claims;

namespace Mingley.API.Controllers;

// ════════════════════════════════════════════════════════════
// INTERESTS
// ════════════════════════════════════════════════════════════
[ApiController]
[Route("v1/interests")]
[Produces("application/json")]
public class InterestsController : ControllerBase
{
    private readonly MingleyDbContext _db;
    public InterestsController(MingleyDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Interests
            .Where(i => !i.IsDeleted)
            .OrderBy(i => i.Name)
            .Select(i => new { id = i.Id.ToString(), i.Name, i.Icon })
            .ToListAsync();
        return Ok(ApiResponse<object>.Ok(new { interests = list }));
    }
}

// ════════════════════════════════════════════════════════════
// PRIVACY
// ════════════════════════════════════════════════════════════
[ApiController]
[Route("v1/privacy")]
[Authorize]
[Produces("application/json")]
public class PrivacyController : ControllerBase
{
    private readonly MingleyDbContext _db;
    private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public PrivacyController(MingleyDbContext db) => _db = db;

    [HttpGet("policy")]
    [AllowAnonymous]
    public IActionResult GetPolicy() => Ok(ApiResponse<object>.Ok(new {
        title = "Mingley Privacy Policy & Match Agreement",
        lastUpdated = "2024-01-01",
        content = @"1. SAFETY
- Do not share personal contact details in chat.
- Mingley is not responsible for offline interactions.

2. COIN ECONOMY
- Audio calls: 10 coins/min | Video calls: 100 coins/min
- SuperChat: 500 coins (girl earns 50% commission on respond)
- Male messages: 10 coins (5 with premium) | Female: 3 free then 5 coins
- Verification bonus: 50 coins | Welcome bonus: 100 coins

3. WITHDRAWALS
- Only female users may withdraw earned coins.
- Minimum processing 3-5 business days.

4. PROHIBITED
- Harassment, abuse, spam, or sharing explicit unsolicited content.
- Violations result in immediate account suspension."
    }));

    [HttpPost("accept/{matchId}")]
    public async Task<IActionResult> Accept(Guid matchId)
    {
        var exists = await _db.PrivacyAgreements
            .AnyAsync(p => p.UserId == Me && p.MatchId == matchId);
        if (!exists)
        {
            _db.PrivacyAgreements.Add(new PrivacyAgreement { UserId = Me, MatchId = matchId, Accepted = true });
            await _db.SaveChangesAsync();
        }
        return Ok(ApiResponse.Ok("Privacy policy accepted."));
    }
}

// ════════════════════════════════════════════════════════════
// VERIFICATION
// ════════════════════════════════════════════════════════════
[ApiController]
[Route("v1/verify")]
[Authorize]
[Produces("application/json")]
public class VerificationController : ControllerBase
{
    private readonly MingleyDbContext _db;
    private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public VerificationController(MingleyDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Verify([FromBody] VerifyRequest req)
    {
        var user = await _db.Users.FindAsync(Me)
            ?? throw new InvalidOperationException("User not found.");
        if (user.IsVerified)
            return BadRequest(ApiResponse<object>.Fail("Profile already verified."));

        user.IsVerified  = true;
        user.CoinBalance += MingleyDbContext.VerificationBonus;
        user.UpdatedAt   = DateTime.UtcNow;

        _db.CoinTransactions.Add(new CoinTransaction
        {
            UserId = Me, Coins = MingleyDbContext.VerificationBonus, Direction = "credit",
            Description = "Profile verification bonus", TransactionType = "verification",
        });
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new {
            coinsAwarded = MingleyDbContext.VerificationBonus,
            newBalance   = user.CoinBalance,
            message      = $"Verified! +{MingleyDbContext.VerificationBonus} coins added."
        }));
    }
}
public class VerifyRequest { public string? IdProofUrl { get; set; } }

// ════════════════════════════════════════════════════════════
// GIFTS
// ════════════════════════════════════════════════════════════
[ApiController]
[Route("v1/gifts")]
[Authorize]
[Produces("application/json")]
public class GiftsController : ControllerBase
{
    private readonly MingleyDbContext _db;
    private readonly Mingley.Application.Interfaces.IHubNotifier _hub;
    private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public GiftsController(MingleyDbContext db, Mingley.Application.Interfaces.IHubNotifier hub)
    { _db = db; _hub = hub; }

    [HttpGet("catalog")]
    [AllowAnonymous]
    public async Task<IActionResult> Catalog()
    {
        var gifts = await _db.Gifts.Where(g => g.IsActive && !g.IsDeleted)
            .Select(g => new { id = g.Id.ToString(), g.Name, g.Icon, emoji = g.Emoji ?? g.Icon, price = g.CoinCost, g.CoinCost })
            .ToListAsync();
        return Ok(ApiResponse<object>.Ok(new { gifts }));
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendGiftRequest req)
    {
        if (!Guid.TryParse(req.GiftId, out var giftId) || !Guid.TryParse(req.RecipientId, out var recipientId))
            return BadRequest(ApiResponse<object>.Fail("Invalid IDs."));

        var sender = await _db.Users.FindAsync(Me)
            ?? throw new InvalidOperationException("User not found.");
        var gift = await _db.Gifts.FindAsync(giftId)
            ?? throw new InvalidOperationException("Gift not found.");

        if (sender.CoinBalance < gift.CoinCost)
            return BadRequest(ApiResponse<object>.Fail($"Need {gift.CoinCost} coins. You have {sender.CoinBalance}."));

        sender.CoinBalance -= gift.CoinCost;
        sender.UpdatedAt    = DateTime.UtcNow;

        _db.CoinTransactions.Add(new CoinTransaction
        {
            UserId = Me, Coins = gift.CoinCost, Direction = "debit",
            Description = $"Gift: {gift.Name}", TransactionType = "gift",
        });

        // Add gift message to chat if chatId provided
        if (!string.IsNullOrEmpty(req.ChatId) && Guid.TryParse(req.ChatId, out var chatId))
        {
            var chat = await _db.Chats.Include(c => c.Match)
                .FirstOrDefaultAsync(c => c.Id == chatId && (c.Match.User1Id == Me || c.Match.User2Id == Me));
            if (chat != null)
            {
                var msg = new Message
                {
                    ChatId   = chatId, SenderId = Me, Type = "gift",
                    GiftName = gift.Name, GiftCost = gift.CoinCost,
                    Text     = $"🎁 Sent a {gift.Name}",
                };
                _db.Messages.Add(msg);
                await _db.SaveChangesAsync();
                await _hub.SendToGroupAsync($"chat_{chatId}", "NewMessage", new
                {
                    chatId = chatId.ToString(),
                    message = new { id = msg.Id.ToString(), senderId = Me.ToString(), type = "gift", giftName = gift.Name, giftCost = gift.CoinCost, text = msg.Text, sentAt = msg.CreatedAt }
                });
            }
        }
        else { await _db.SaveChangesAsync(); }

        // Notify recipient
        var notif = new Notification
        {
            UserId = recipientId, Title = $"🎁 {sender.FullName} sent you a {gift.Name}!",
            Body = "Open the app to see your gift.", Type = "gift",
        };
        _db.Notifications.Add(notif);
        await _db.SaveChangesAsync();
        await _hub.SendToUserAsync(recipientId.ToString(), "NewNotification", new
        {
            id = notif.Id.ToString(), notif.Title, notif.Body, notif.Type, giftName = gift.Name
        });

        return Ok(ApiResponse<object>.Ok(new { newBalance = sender.CoinBalance }));
    }
}

public class SendGiftRequest
{
    public string? RecipientId { get; set; }
    public string? GiftId      { get; set; }
    public string? ChatId      { get; set; }
    public string? Message     { get; set; }
}
