using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mingley.Application.DTOs.Common;
using Mingley.Application.Interfaces;
using Mingley.Domain.Entities;
using Mingley.Infrastructure.Persistence;
using System.Security.Claims;

namespace Mingley.API.Controllers;

[ApiController]
[Route("v1/admin")]
[Authorize(Roles = "admin")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly MingleyDbContext _db;
    private readonly IWalletService _wallet;
    private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public AdminController(MingleyDbContext db, IWalletService wallet)
    { _db = db; _wallet = wallet; }

    // ════════════════════════════════════════════════════════════════
    // TEMP MIGRATION — run once then it auto-disables itself
    // ════════════════════════════════════════════════════════════════
    [HttpPost("run-migration")]
    [AllowAnonymous]
    public async Task<IActionResult> RunMigration()
    {
        await _db.Database.ExecuteSqlRawAsync(@"
            ALTER TABLE ""Users""
            ADD COLUMN IF NOT EXISTS ""IsCreatedByAdmin"" boolean NOT NULL DEFAULT false,
            ADD COLUMN IF NOT EXISTS ""IsSuspended""      boolean NOT NULL DEFAULT false,
            ADD COLUMN IF NOT EXISTS ""SuspendedAt""      timestamp with time zone NULL,
            ADD COLUMN IF NOT EXISTS ""SuspendReason""    text NULL,
            ADD COLUMN IF NOT EXISTS ""SuspendedBy""      text NULL;
        ");
        return Ok(new { success = true, message = "Migration complete! All 5 columns added to Users table." });
    }

    // ════════════════════════════════════════════════════════════════
    // DASHBOARD
    // ════════════════════════════════════════════════════════════════
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var now = DateTime.UtcNow;
        var today = now.Date;

        var totalUsers = await _db.Users.IgnoreQueryFilters().CountAsync();
        var activeUsers = await _db.Users.CountAsync();
        var premiumUsers = await _db.Users.CountAsync(u => u.IsPremium);
        var onlineUsers = await _db.Users.CountAsync(u => u.IsOnline);
        var totalMatches = await _db.Matches.IgnoreQueryFilters().CountAsync();
        var totalMessages = await _db.Messages.IgnoreQueryFilters().CountAsync();
        var totalSuperChats = await _db.SuperChats.CountAsync();
        var superChatRevenue = await _db.SuperChats.SumAsync(s => s.CompanyRevenue);
        var totalCommissions = await _db.SuperChats.SumAsync(s => s.GirlCommission);
        var pendingDeposits = await _db.DepositRequests.CountAsync(d => d.Status == "pending");
        var pendingWithdrawals = await _db.WithdrawalRequests.CountAsync(w => w.Status == "pending");
        var newUsersToday = await _db.Users.CountAsync(u => u.CreatedAt >= today);
        var matchesToday = await _db.Matches.CountAsync(m => m.CreatedAt >= today);
        var callSessions = await _db.CallSessions.CountAsync();
        var totalCoinsDeducted = await _db.CoinTransactions.Where(t => t.Direction == "debit").SumAsync(t => (long)t.Coins);
        var totalCoinsIssued = await _db.CoinTransactions.Where(t => t.Direction == "credit").SumAsync(t => (long)t.Coins);
        var totalInCirculation = await _db.Users.SumAsync(u => (long)u.CoinBalance);
        var managedUsers = await _db.Users.IgnoreQueryFilters().CountAsync(u => u.IsCreatedByAdmin);
        var suspendedUsers = await _db.Users.IgnoreQueryFilters().CountAsync(u => u.IsSuspended);

        return Ok(ApiResponse<object>.Ok(new
        {
            users = new
            {
                totalUsers,
                activeUsers,
                premiumUsers,
                onlineUsers,
                newUsersToday,
                managedUsers,
                suspendedUsers
            },
            activity = new
            {
                totalMatches,
                matchesToday,
                totalMessages,
                callSessions,
                totalSuperChats
            },
            finance = new
            {
                superChatRevenue,
                totalCommissions,
                totalCoinsDeducted,
                totalCoinsIssued,
                totalInCirculation,
                pendingDeposits,
                pendingWithdrawals
            },
        }));
    }

    // ════════════════════════════════════════════════════════════════
    // REGULAR USERS
    // ════════════════════════════════════════════════════════════════
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? gender = null,
        [FromQuery] bool? isPremium = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool? isSuspended = null)
    {
        var q = _db.Users.IgnoreQueryFilters().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(u =>
                (u.FullName != null && u.FullName.ToLower().Contains(search.ToLower())) ||
                (u.Email != null && u.Email.ToLower().Contains(search.ToLower())) ||
                (u.Phone != null && u.Phone.Contains(search)));

        if (!string.IsNullOrWhiteSpace(gender)) q = q.Where(u => u.Gender == gender);
        if (isPremium.HasValue) q = q.Where(u => u.IsPremium == isPremium.Value);
        if (isActive.HasValue) q = q.Where(u => u.IsActive == isActive.Value);
        if (isSuspended.HasValue) q = q.Where(u => u.IsSuspended == isSuspended.Value);

        var total = await q.CountAsync();
        var users = await q
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * limit).Take(limit)
            .Select(u => new
            {
                id = u.Id.ToString(),
                u.FullName,
                u.Email,
                u.Phone,
                u.Gender,
                u.IsVerified,
                u.IsPremium,
                u.IsActive,
                u.IsOnline,
                u.IsDeleted,
                u.IsSuspended,
                u.SuspendReason,
                u.CoinBalance,
                u.TotalEarned,
                u.Role,
                u.IsCreatedByAdmin,
                u.CreatedAt,
                u.LastActiveAt,
                u.ProfileComplete,
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new { users, total, page, limit }));
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _db.Users.IgnoreQueryFilters()
            .Include(u => u.Location)
            .Include(u => u.Preference)
            .Include(u => u.Images)
            .Include(u => u.Interests).ThenInclude(i => i.Interest)
            .Include(u => u.Subscription).ThenInclude(s => s!.Plan)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound(ApiResponse<object>.Fail("User not found.", 404));

        var matchCount = await _db.Matches.IgnoreQueryFilters().CountAsync(m => m.User1Id == id || m.User2Id == id);
        var msgCount = await _db.Messages.IgnoreQueryFilters().CountAsync(m => m.SenderId == id);
        var callCount = await _db.CallSessions.CountAsync(c => c.CallerId == id || c.ReceiverId == id);
        var spentCoins = await _db.CoinTransactions.Where(t => t.UserId == id && t.Direction == "debit").SumAsync(t => (long)t.Coins);
        var earnedCoins = await _db.CoinTransactions.Where(t => t.UserId == id && t.Direction == "credit").SumAsync(t => (long)t.Coins);

        var recentTxns = await _db.CoinTransactions
            .Where(t => t.UserId == id)
            .OrderByDescending(t => t.CreatedAt).Take(20)
            .Select(t => new { t.Id, t.Coins, t.Direction, t.Description, t.TransactionType, t.CreatedAt })
            .ToListAsync();

        var deposits = await _db.DepositRequests.Where(d => d.UserId == id).OrderByDescending(d => d.CreatedAt).Take(10).ToListAsync();
        var withdrawals = await _db.WithdrawalRequests.Where(w => w.UserId == id).OrderByDescending(w => w.CreatedAt).Take(10).ToListAsync();

        return Ok(ApiResponse<object>.Ok(new
        {
            user = new
            {
                id = user.Id.ToString(),
                user.FullName,
                user.Email,
                user.Phone,
                user.Gender,
                user.IsVerified,
                user.IsPremium,
                user.IsActive,
                user.IsOnline,
                user.IsDeleted,
                user.IsSuspended,
                user.SuspendReason,
                user.SuspendedAt,
                user.IsCreatedByAdmin,
                user.CoinBalance,
                user.TotalEarned,
                user.Role,
                user.CreatedAt,
                user.LastActiveAt,
                user.ProfileComplete,
                user.Bio,
                user.Avatar,
                Location = user.Location == null ? null : new
                {
                    user.Location.City,
                    user.Location.Country,
                    user.Location.Lat,
                    user.Location.Lng
                },
                Preference = user.Preference == null ? null : new
                {
                    user.Preference.InterestedIn,
                    user.Preference.MinAge,
                    user.Preference.MaxAge
                },
                Interests = user.Interests.Select(i => i.Interest?.Name),
                Images = user.Images.Select(i => i.Url),
                Subscription = user.Subscription == null ? null : new
                {
                    PlanName = user.Subscription.Plan?.Name,
                    user.Subscription.StartDate,
                    user.Subscription.EndDate,
                    user.Subscription.IsActive,
                },
            },
            stats = new { matchCount, msgCount, callCount, spentCoins, earnedCoins },
            recentTxns,
            deposits,
            withdrawals,
        }));
    }

    [HttpPost("users/create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest req)
    {
        if (await _db.Users.IgnoreQueryFilters()
            .AnyAsync(u => u.Email == req.Email || u.Phone == req.Phone))
            return BadRequest(ApiResponse<object>.Fail("Email or phone already exists."));

        var user = new User
        {
            FullName = req.FullName,
            Email = req.Email?.ToLower().Trim(),
            Phone = req.Phone?.Trim(),
            Gender = req.Gender,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password ?? "Admin@123"),
            IsVerified = true,
            IsActive = true,
            ProfileComplete = true,
            Role = req.Role ?? "user",
            CoinBalance = req.InitialCoins,
            DateOfBirth = req.DateOfBirth?.ToUniversalTime(),
        };
        _db.Users.Add(user);
        _db.UserPreferences.Add(new UserPreference { UserId = user.Id });
        await _db.SaveChangesAsync();

        if (req.InitialCoins > 0)
        {
            _db.CoinTransactions.Add(new CoinTransaction
            {
                UserId = user.Id,
                Coins = req.InitialCoins,
                Direction = "credit",
                Description = "Admin created account bonus",
                TransactionType = "admin_grant",
            });
            await _db.SaveChangesAsync();
        }

        return StatusCode(201, ApiResponse<object>.Created(
            new { id = user.Id.ToString(), user.Email, user.FullName, user.Role },
            "User created."));
    }

    [HttpPut("users/{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new InvalidOperationException("User not found.");
        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse.Ok($"User {(user.IsActive ? "activated" : "deactivated")}."));
    }

    [HttpPost("users/{id}/grant-subscription")]
    public async Task<IActionResult> GrantSubscription(Guid id, [FromBody] GrantSubRequest req)
    {
        SubscriptionPlan? plan = null;

        if (Guid.TryParse(req.PlanId, out var planGuid))
            plan = await _db.SubscriptionPlans.FindAsync(planGuid);

        plan ??= await _db.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.IsActive && !p.IsDeleted &&
                p.Name.ToLower() == req.PlanId.ToLower().Trim());

        if (plan == null) return BadRequest(ApiResponse<object>.Fail("Plan not found."));

        var old = await _db.UserSubscriptions.Where(s => s.UserId == id && s.IsActive).ToListAsync();
        old.ForEach(s => { s.IsActive = false; s.UpdatedAt = DateTime.UtcNow; });

        var days = req.Days > 0 ? req.Days : plan.DurationDays;
        var sub = new UserSubscription
        {
            UserId = id,
            PlanId = plan.Id,
            EndDate = DateTime.UtcNow.AddDays(days),
            IsActive = true,
            AutoRenew = false,
        };
        _db.UserSubscriptions.Add(sub);

        var user = await _db.Users.FindAsync(id);
        if (user != null) { user.IsPremium = true; user.UpdatedAt = DateTime.UtcNow; }

        await _db.SaveChangesAsync();
        return Ok(ApiResponse.Ok($"{plan.Name} granted for {days} days."));
    }

    [HttpPost("users/{id}/add-coins")]
    public async Task<IActionResult> AddCoins(Guid id, [FromBody] AddCoinsRequest req)
    {
        await _wallet.AddCoinsAsync(id, req.Coins, req.Description ?? "Admin credit", "admin_credit");
        return Ok(ApiResponse.Ok($"{req.Coins} coins added."));
    }

    [HttpGet("users/{id}/chats")]
    public async Task<IActionResult> GetUserChats(Guid id)
    {
        var matches = await _db.Matches.IgnoreQueryFilters()
            .Include(m => m.Chat)
            .ThenInclude(c => c!.Messages.OrderByDescending(msg => msg.CreatedAt).Take(1))
            .Include(m => m.User1)
            .Include(m => m.User2)
            .Where(m => m.User1Id == id || m.User2Id == id)
            .ToListAsync();

        var result = matches.Select(m => new
        {
            matchId = m.Id.ToString(),
            chatId = m.Chat?.Id.ToString(),
            isActive = m.IsActive,
            participant = m.User1Id == id
                ? new { m.User2!.FullName, m.User2.Avatar }
                : new { m.User1!.FullName, m.User1.Avatar },
            lastMessage = m.Chat?.Messages.FirstOrDefault()?.Text,
            messageCount = m.Chat?.Messages.Count ?? 0,
        });

        return Ok(ApiResponse<object>.Ok(new { chats = result }));
    }

    [HttpGet("users/{id}/messages")]
    public async Task<IActionResult> GetUserMessages(Guid id, [FromQuery] Guid? chatId)
    {
        var q = _db.Messages.IgnoreQueryFilters()
            .Include(m => m.Sender)
            .Where(m => m.SenderId == id);

        if (chatId.HasValue) q = q.Where(m => m.ChatId == chatId.Value);

        var msgs = await q
            .OrderByDescending(m => m.CreatedAt).Take(100)
            .Select(m => new
            {
                id = m.Id.ToString(),
                m.Text,
                m.Type,
                m.ImageUrl,
                m.CoinAmount,
                m.CreatedAt,
                m.CoinsDeducted,
                m.IsDeleted,
                chatId = m.ChatId.ToString(),
                Sender = new { m.Sender!.FullName, m.Sender.Avatar },
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new { messages = msgs, total = msgs.Count }));
    }

    // ════════════════════════════════════════════════════════════════
    // MANAGED USERS (Created by Admin — separate tab)
    // ════════════════════════════════════════════════════════════════

    [HttpGet("managed-users")]
    public async Task<IActionResult> GetManagedUsers(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null)
    {
        var q = _db.Users.IgnoreQueryFilters()
            .Where(u => u.IsCreatedByAdmin);

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(u =>
                (u.FullName != null && u.FullName.ToLower().Contains(search.ToLower())) ||
                (u.Email != null && u.Email.ToLower().Contains(search.ToLower())) ||
                (u.Phone != null && u.Phone.Contains(search)));

        if (status == "suspended") q = q.Where(u => u.IsSuspended);
        else if (status == "active") q = q.Where(u => u.IsActive && !u.IsSuspended);
        else if (status == "paused") q = q.Where(u => !u.IsActive && !u.IsSuspended);

        var total = await q.CountAsync();
        var users = await q
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * limit).Take(limit)
            .Select(u => new
            {
                id = u.Id.ToString(),
                u.FullName,
                u.Email,
                u.Phone,
                u.Gender,
                u.Avatar,
                u.IsActive,
                u.IsSuspended,
                u.SuspendReason,
                u.SuspendedAt,
                u.IsVerified,
                u.IsPremium,
                u.CoinBalance,
                u.TotalEarned,
                u.CreatedAt,
                u.LastActiveAt,
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new { users, total, page, limit }));
    }

    [HttpPost("managed-users/create")]
    public async Task<IActionResult> CreateManagedUser([FromBody] CreateUserRequest req)
    {
        if (await _db.Users.IgnoreQueryFilters()
            .AnyAsync(u => u.Email == req.Email || u.Phone == req.Phone))
            return BadRequest(ApiResponse<object>.Fail("Email or phone already exists."));

        var user = new User
        {
            FullName = req.FullName,
            Email = req.Email?.ToLower().Trim(),
            Phone = req.Phone?.Trim(),
            Gender = req.Gender,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password ?? "Admin@123"),
            IsVerified = true,
            IsActive = true,
            ProfileComplete = true,
            Role = "user",
            CoinBalance = req.InitialCoins,
            DateOfBirth = req.DateOfBirth?.ToUniversalTime(),
            IsCreatedByAdmin = true,
        };
        _db.Users.Add(user);
        _db.UserPreferences.Add(new UserPreference { UserId = user.Id });
        await _db.SaveChangesAsync();

        if (req.InitialCoins > 0)
        {
            _db.CoinTransactions.Add(new CoinTransaction
            {
                UserId = user.Id,
                Coins = req.InitialCoins,
                Direction = "credit",
                Description = "Admin managed account bonus",
                TransactionType = "admin_grant",
            });
            await _db.SaveChangesAsync();
        }

        return StatusCode(201, ApiResponse<object>.Created(
            new
            {
                id = user.Id.ToString(),
                user.Email,
                user.FullName,
                user.CoinBalance,
                isCreatedByAdmin = true,
            },
            $"Managed user {user.FullName} created."));
    }

    // ── Suspend ──────────────────────────────────────────────────────
    [HttpPost("users/{id}/suspend")]
    public async Task<IActionResult> SuspendUser(Guid id, [FromBody] SuspendRequest req)
    {
        var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new InvalidOperationException("User not found.");

        user.IsSuspended = true;
        user.IsActive = false;
        user.SuspendedAt = DateTime.UtcNow;
        user.SuspendReason = req.Reason ?? "Suspended by admin";
        user.SuspendedBy = Me.ToString();
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new
        {
            userId = id.ToString(),
            isSuspended = true,
            reason = user.SuspendReason,
            suspendedAt = user.SuspendedAt,
        }, $"User {user.FullName} has been suspended."));
    }

    // ── Pause (temporary deactivate) ─────────────────────────────────
    [HttpPost("users/{id}/pause")]
    public async Task<IActionResult> PauseUser(Guid id, [FromBody] SuspendRequest req)
    {
        var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new InvalidOperationException("User not found.");

        user.IsActive = false;
        user.SuspendReason = req.Reason ?? "Paused by admin";
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new
        {
            userId = id.ToString(),
            isActive = false,
            reason = user.SuspendReason,
        }, $"User {user.FullName} has been paused."));
    }

    // ── Resume ───────────────────────────────────────────────────────
    [HttpPost("users/{id}/resume")]
    public async Task<IActionResult> ResumeUser(Guid id)
    {
        var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new InvalidOperationException("User not found.");

        user.IsSuspended = false;
        user.IsActive = true;
        user.SuspendedAt = null;
        user.SuspendReason = null;
        user.SuspendedBy = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new
        {
            userId = id.ToString(),
            isSuspended = false,
            isActive = true,
        }, $"User {user.FullName} has been resumed successfully."));
    }

    // ── Full Logs ────────────────────────────────────────────────────
    [HttpGet("users/{id}/full-logs")]
    public async Task<IActionResult> GetUserFullLogs(Guid id)
    {
        var user = await _db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new InvalidOperationException("User not found.");

        // Messages sent
        var messages = await _db.Messages.IgnoreQueryFilters()
            .Where(m => m.SenderId == id)
            .OrderByDescending(m => m.CreatedAt).Take(100)
            .Select(m => new
            {
                id = m.Id.ToString(),
                chatId = m.ChatId.ToString(),
                text = m.IsDeleted ? "[deleted]" : m.Text,
                type = m.Type,
                coinAmount = m.CoinAmount,
                createdAt = m.CreatedAt,
                isDeleted = m.IsDeleted,
            })
            .ToListAsync();

        // Call sessions
        var calls = await _db.CallSessions
            .Include(c => c.Caller)
            .Include(c => c.Receiver)
            .Where(c => c.CallerId == id || c.ReceiverId == id)
            .OrderByDescending(c => c.CreatedAt).Take(50)
            .Select(c => new
            {
                id = c.Id.ToString(),
                callType = c.CallType,
                status = c.Status,
                duration = c.DurationSeconds,
                coinsUsed = c.CoinsDeducted,
                direction = c.CallerId == id ? "outgoing" : "incoming",
                otherUser = c.CallerId == id
                    ? c.Receiver!.FullName
                    : c.Caller!.FullName,
                answeredAt = c.AnsweredAt,
                endedAt = c.EndedAt,
                createdAt = c.CreatedAt,
            })
            .ToListAsync();

        // Coin transactions
        var transactions = await _db.CoinTransactions
            .Where(t => t.UserId == id)
            .OrderByDescending(t => t.CreatedAt).Take(100)
            .Select(t => new
            {
                id = t.Id.ToString(),
                t.Coins,
                t.Direction,
                t.Description,
                t.TransactionType,
                t.ReferenceId,
                t.CreatedAt,
            })
            .ToListAsync();

        // SuperChats
        var superchats = await _db.SuperChats
            .Include(s => s.FromUser)
            .Include(s => s.ToUser)
            .Where(s => s.FromUserId == id || s.ToUserId == id)
            .OrderByDescending(s => s.CreatedAt).Take(50)
            .Select(s => new
            {
                id = s.Id.ToString(),
                direction = s.FromUserId == id ? "sent" : "received",
                message = s.Message,
                coinAmount = s.CoinAmount,
                isResponded = s.IsResponded,
                otherUser = s.FromUserId == id
                    ? s.ToUser!.FullName
                    : s.FromUser!.FullName,
                createdAt = s.CreatedAt,
            })
            .ToListAsync();

        // Deposit / withdrawal requests
        var deposits = await _db.DepositRequests.Where(d => d.UserId == id).OrderByDescending(d => d.CreatedAt).Take(20)
            .Select(d => new { id = d.Id.ToString(), d.UtrId, d.RequestedCoins, d.Status, d.CreatedAt }).ToListAsync();
        var withdrawals = await _db.WithdrawalRequests.Where(w => w.UserId == id).OrderByDescending(w => w.CreatedAt).Take(20)
            .Select(w => new { id = w.Id.ToString(), w.Coins, w.BankOrUpi, w.Status, w.CreatedAt }).ToListAsync();

        var totalSpent = transactions.Where(t => t.Direction == "debit").Sum(t => t.Coins);
        var totalEarned = transactions.Where(t => t.Direction == "credit").Sum(t => t.Coins);

        return Ok(ApiResponse<object>.Ok(new
        {
            user = new
            {
                id = user.Id.ToString(),
                user.FullName,
                user.Email,
                user.Phone,
                user.Gender,
                user.Avatar,
                user.CoinBalance,
                user.TotalEarned,
                user.IsSuspended,
                user.SuspendReason,
                user.IsCreatedByAdmin,
                user.CreatedAt,
                user.LastActiveAt,
            },
            stats = new
            {
                totalMessages = messages.Count,
                totalCalls = calls.Count,
                totalTransactions = transactions.Count,
                totalSuperChats = superchats.Count,
                totalCoinsSpent = totalSpent,
                totalCoinsEarned = totalEarned,
                netCoins = totalEarned - totalSpent,
            },
            messages,
            calls,
            transactions,
            superchats,
            deposits,
            withdrawals,
        }));
    }

    // ════════════════════════════════════════════════════════════════
    // DEPOSITS
    // ════════════════════════════════════════════════════════════════
    [HttpGet("deposits")]
    public async Task<IActionResult> GetDeposits([FromQuery] string status = "pending")
    {
        var q = _db.DepositRequests.Include(d => d.User).AsQueryable();
        if (status != "all") q = q.Where(d => d.Status == status);

        var list = await q.OrderByDescending(d => d.CreatedAt).Take(100)
            .Select(d => new
            {
                id = d.Id.ToString(),
                d.UtrId,
                d.ScreenshotUrl,
                d.RequestedCoins,
                d.Status,
                d.AdminNote,
                d.CreatedAt,
                User = new { d.User!.FullName, d.User.Email, d.User.CoinBalance },
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new { deposits = list, total = list.Count }));
    }

    [HttpPost("deposits/{id}/approve")]
    public async Task<IActionResult> ApproveDeposit(Guid id, [FromBody] AdminNoteRequest req)
    {
        var dep = await _db.DepositRequests.Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id && d.Status == "pending")
            ?? throw new InvalidOperationException("Deposit not found or already processed.");

        dep.Status = "approved";
        dep.AdminNote = req.Note;
        dep.User!.CoinBalance += dep.RequestedCoins ?? 0;
        dep.User.UpdatedAt = DateTime.UtcNow;

        _db.CoinTransactions.Add(new CoinTransaction
        {
            UserId = dep.UserId,
            Coins = dep.RequestedCoins ?? 0,
            Direction = "credit",
            Description = $"Deposit approved (UTR: {dep.UtrId})",
            TransactionType = "deposit",
        });
        await _db.SaveChangesAsync();

        return Ok(ApiResponse.Ok($"✅ {dep.RequestedCoins} coins credited to {dep.User.FullName}."));
    }

    [HttpPost("deposits/{id}/reject")]
    public async Task<IActionResult> RejectDeposit(Guid id, [FromBody] AdminNoteRequest req)
    {
        var dep = await _db.DepositRequests.FirstOrDefaultAsync(d => d.Id == id && d.Status == "pending")
            ?? throw new InvalidOperationException("Deposit not found.");
        dep.Status = "rejected";
        dep.AdminNote = req.Note;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse.Ok("Deposit rejected."));
    }

    // ════════════════════════════════════════════════════════════════
    // WITHDRAWALS
    // ════════════════════════════════════════════════════════════════
    [HttpGet("withdrawals")]
    public async Task<IActionResult> GetWithdrawals([FromQuery] string status = "pending")
    {
        var q = _db.WithdrawalRequests.Include(w => w.User).AsQueryable();
        if (status != "all") q = q.Where(w => w.Status == status);

        var list = await q.OrderByDescending(w => w.CreatedAt).Take(100)
            .Select(w => new
            {
                id = w.Id.ToString(),
                w.Coins,
                w.BankOrUpi,
                w.Status,
                w.AdminNote,
                w.CreatedAt,
                User = new { w.User!.FullName, w.User.Email, w.User.Gender, w.User.TotalEarned },
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new { withdrawals = list, total = list.Count }));
    }

    [HttpPost("withdrawals/{id}/approve")]
    public async Task<IActionResult> ApproveWithdrawal(Guid id, [FromBody] AdminNoteRequest req)
    {
        var wr = await _db.WithdrawalRequests.FirstOrDefaultAsync(w => w.Id == id && w.Status == "pending")
            ?? throw new InvalidOperationException("Withdrawal not found.");
        wr.Status = "approved";
        wr.AdminNote = req.Note;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse.Ok("Withdrawal approved. Process the payment manually."));
    }

    [HttpPost("withdrawals/{id}/reject")]
    public async Task<IActionResult> RejectWithdrawal(Guid id, [FromBody] AdminNoteRequest req)
    {
        var wr = await _db.WithdrawalRequests.Include(w => w.User)
            .FirstOrDefaultAsync(w => w.Id == id && w.Status == "pending")
            ?? throw new InvalidOperationException("Withdrawal not found.");

        wr.Status = "rejected";
        wr.AdminNote = req.Note;
        wr.User!.CoinBalance += wr.Coins;
        wr.User.UpdatedAt = DateTime.UtcNow;

        _db.CoinTransactions.Add(new CoinTransaction
        {
            UserId = wr.UserId,
            Coins = wr.Coins,
            Direction = "credit",
            Description = "Withdrawal rejected — coins refunded",
            TransactionType = "refund",
        });
        await _db.SaveChangesAsync();
        return Ok(ApiResponse.Ok("Withdrawal rejected and coins refunded."));
    }

    // ════════════════════════════════════════════════════════════════
    // REPORTS
    // ════════════════════════════════════════════════════════════════
    [HttpGet("reports")]
    public async Task<IActionResult> GetReports([FromQuery] string status = "pending")
    {
        var q = _db.Reports.Include(r => r.Reporter).Include(r => r.ReportedUser).AsQueryable();
        if (status != "all") q = q.Where(r => r.Status == status);

        var list = await q.OrderByDescending(r => r.CreatedAt).Take(100)
            .Select(r => new
            {
                id = r.Id.ToString(),
                r.Reason,
                r.Description,
                r.Status,
                r.CreatedAt,
                Reporter = new { r.Reporter!.FullName, r.Reporter.Email },
                ReportedUser = new { r.ReportedUser!.FullName, r.ReportedUser.Email },
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new { reports = list, total = list.Count }));
    }

    [HttpPost("reports/{id}/resolve")]
    public async Task<IActionResult> ResolveReport(Guid id, [FromBody] AdminNoteRequest req)
    {
        var report = await _db.Reports.FindAsync(id)
            ?? throw new InvalidOperationException("Report not found.");
        report.Status = "resolved";
        report.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse.Ok("Report resolved."));
    }

    // ════════════════════════════════════════════════════════════════
    // SUPERCHATS
    // ════════════════════════════════════════════════════════════════
    [HttpGet("superchat")]
    public async Task<IActionResult> GetSuperChats([FromQuery] int page = 1, [FromQuery] bool? isResponded = null)
    {
        var q = _db.SuperChats.Include(s => s.FromUser).Include(s => s.ToUser).AsQueryable();
        if (isResponded.HasValue) q = q.Where(s => s.IsResponded == isResponded.Value);

        var list = await q.OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * 30).Take(30)
            .Select(s => new
            {
                id = s.Id.ToString(),
                s.Message,
                s.CoinAmount,
                s.CompanyRevenue,
                s.GirlCommission,
                s.IsResponded,
                s.RespondedAt,
                s.CreatedAt,
                FromUser = new { s.FromUser!.FullName, s.FromUser.Email },
                ToUser = new { s.ToUser!.FullName, s.ToUser.Email },
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new { superChats = list }));
    }

    // ════════════════════════════════════════════════════════════════
    // NOTIFICATIONS
    // ════════════════════════════════════════════════════════════════
    [HttpPost("notifications/broadcast")]
    public async Task<IActionResult> Broadcast([FromBody] BroadcastRequest req)
    {
        var userIds = await _db.Users.Select(u => u.Id).ToListAsync();
        var notifs = userIds.Select(uid => new Notification
        {
            UserId = uid,
            Title = req.Title,
            Body = req.Body,
            Type = "broadcast",
        }).ToList();
        _db.Notifications.AddRange(notifs);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse.Ok($"Notification sent to {userIds.Count} users."));
    }

    [HttpPost("users/{id}/notify")]
    public async Task<IActionResult> NotifyUser(Guid id, [FromBody] BroadcastRequest req)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = id,
            Title = req.Title,
            Body = req.Body,
            Type = "admin",
        });
        await _db.SaveChangesAsync();
        return Ok(ApiResponse.Ok("Notification sent."));
    }

    // ════════════════════════════════════════════════════════════════
    // COIN STATS
    // ════════════════════════════════════════════════════════════════
    [HttpGet("stats/coins")]
    public async Task<IActionResult> CoinStats()
    {
        var totalInCirculation = await _db.Users.SumAsync(u => (long)u.CoinBalance);
        var totalIssued = await _db.CoinTransactions.Where(t => t.Direction == "credit").SumAsync(t => (long)t.Coins);
        var totalBurned = await _db.CoinTransactions.Where(t => t.Direction == "debit").SumAsync(t => (long)t.Coins);
        var pendingDeps = await _db.DepositRequests.Where(d => d.Status == "pending").SumAsync(d => (long)(d.RequestedCoins ?? 0));
        var superChatRevenue = await _db.SuperChats.SumAsync(s => s.CompanyRevenue);
        var superChatCommission = await _db.SuperChats.SumAsync(s => s.GirlCommission);

        var topSpenders = await _db.CoinTransactions
            .Where(t => t.Direction == "debit")
            .GroupBy(t => t.UserId)
            .Select(g => new { userId = g.Key, total = g.Sum(t => (long)t.Coins) })
            .OrderByDescending(x => x.total).Take(5)
            .Join(_db.Users.IgnoreQueryFilters(), x => x.userId, u => u.Id,
                (x, u) => new { userId = x.userId.ToString(), u.FullName, u.Email, totalSpent = x.total })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new
        {
            totalInCirculation,
            totalIssued,
            totalBurned,
            pendingDeps,
            superChatRevenue,
            superChatCommission,
            topSpenders,
        }));
    }

    // ════════════════════════════════════════════════════════════════
    // CALL SESSIONS
    // ════════════════════════════════════════════════════════════════
    [HttpGet("calls")]
    public async Task<IActionResult> GetCalls([FromQuery] int page = 1, [FromQuery] string? callType = null)
    {
        var q = _db.CallSessions
            .Include(c => c.Caller)
            .Include(c => c.Receiver)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(callType))
            q = q.Where(c => c.CallType == callType);

        var total = await q.CountAsync();
        var calls = await q
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * 30).Take(30)
            .Select(c => new
            {
                id = c.Id.ToString(),
                c.CallType,
                c.Status,
                c.DurationSeconds,
                c.CoinsDeducted,
                c.AnsweredAt,
                c.EndedAt,
                c.CreatedAt,
                Caller = new { c.Caller!.FullName, c.Caller.Email },
                Receiver = new { c.Receiver!.FullName, c.Receiver.Email },
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new { calls, total, page }));
    }

    // ════════════════════════════════════════════════════════════════
    // SUBSCRIPTION PLANS MANAGEMENT
    // ════════════════════════════════════════════════════════════════
    [HttpGet("plans")]
    public async Task<IActionResult> GetPlans()
    {
        var plans = await _db.SubscriptionPlans
            .IgnoreQueryFilters()
            .OrderBy(p => p.Price)
            .Select(p => new
            {
                id = p.Id.ToString(),
                p.Name,
                p.Price,
                p.DurationDays,
                p.IsActive,
                p.IsPopular,
                p.SuperLikesPerDay,
                p.BoostsPerMonth,
                p.UnlimitedLikes,
                p.CanSeeWhoLiked,
                p.VideoCallEnabled,
                p.CreatedAt,
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new { plans }));
    }
}

// ════════════════════════════════════════════════════════════════
// REQUEST DTOs
// ════════════════════════════════════════════════════════════════
public class AdminNoteRequest
{
    public string? Note { get; set; }
}

public class GrantSubRequest
{
    public string PlanId { get; set; } = "";
    public int Days { get; set; }
}

public class AddCoinsRequest
{
    public int Coins { get; set; }
    public string? Description { get; set; }
}

public class CreateUserRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; }
    public int InitialCoins { get; set; } = 100;
    public DateTime? DateOfBirth { get; set; }
}

public class BroadcastRequest
{
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
}

public class SuspendRequest
{
    public string? Reason { get; set; }
}

//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Mingley.Application.DTOs.Common;
//using Mingley.Application.Interfaces;
//using Mingley.Domain.Entities;
//using Mingley.Infrastructure.Persistence;
//using System.Security.Claims;

//namespace Mingley.API.Controllers;

//[ApiController]
//[Route("v1/admin")]
//[Authorize(Roles = "admin")]
//[Produces("application/json")]
//public class AdminController : ControllerBase
//{
//    private readonly MingleyDbContext _db;
//    private readonly IWalletService _wallet;
//    private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

//    public AdminController(MingleyDbContext db, IWalletService wallet)
//    { _db = db; _wallet = wallet; }

//    // ── Dashboard ────────────────────────────────────────────────────────────
//    [HttpGet("dashboard")]
//    public async Task<IActionResult> Dashboard()
//    {
//        var now = DateTime.UtcNow;
//        var today = now.Date;

//        var totalUsers = await _db.Users.IgnoreQueryFilters().CountAsync();
//        var activeUsers = await _db.Users.CountAsync();
//        var premiumUsers = await _db.Users.CountAsync(u => u.IsPremium);
//        var onlineUsers = await _db.Users.CountAsync(u => u.IsOnline);
//        var totalMatches = await _db.Matches.IgnoreQueryFilters().CountAsync();
//        var totalMessages = await _db.Messages.IgnoreQueryFilters().CountAsync();
//        var totalSuperChats = await _db.SuperChats.CountAsync();
//        var superChatRevenue = await _db.SuperChats.SumAsync(s => s.CompanyRevenue);
//        var totalCommissions = await _db.SuperChats.SumAsync(s => s.GirlCommission);
//        var pendingDeposits = await _db.DepositRequests.CountAsync(d => d.Status == "pending");
//        var pendingWithdrawals = await _db.WithdrawalRequests.CountAsync(w => w.Status == "pending");
//        var newUsersToday = await _db.Users.CountAsync(u => u.CreatedAt >= today);
//        var matchesToday = await _db.Matches.CountAsync(m => m.CreatedAt >= today);
//        var callSessions = await _db.CallSessions.CountAsync();
//        var totalCoinsDeducted = await _db.CoinTransactions.Where(t => t.Direction == "debit").SumAsync(t => (long)t.Coins);

//        return Ok(ApiResponse<object>.Ok(new
//        {
//            users = new { totalUsers, activeUsers, premiumUsers, onlineUsers, newUsersToday },
//            activity = new { totalMatches, matchesToday, totalMessages, callSessions, totalSuperChats },
//            finance = new { superChatRevenue, totalCommissions, totalCoinsDeducted, pendingDeposits, pendingWithdrawals },
//        }));
//    }

//    // ── Users ────────────────────────────────────────────────────────────────
//    [HttpGet("users")]
//    public async Task<IActionResult> GetUsers(
//        [FromQuery] int page = 1, [FromQuery] int limit = 20,
//        [FromQuery] string? search = null, [FromQuery] string? gender = null,
//        [FromQuery] bool? isPremium = null, [FromQuery] bool? isActive = null)
//    {
//        var q = _db.Users.IgnoreQueryFilters().AsQueryable();
//        if (!string.IsNullOrWhiteSpace(search))
//            q = q.Where(u => u.FullName!.Contains(search) || u.Email!.Contains(search) || u.Phone!.Contains(search));
//        if (!string.IsNullOrWhiteSpace(gender)) q = q.Where(u => u.Gender == gender);
//        if (isPremium.HasValue) q = q.Where(u => u.IsPremium == isPremium.Value);
//        if (isActive.HasValue) q = q.Where(u => u.IsActive == isActive.Value);

//        var total = await q.CountAsync();
//        var users = await q.OrderByDescending(u => u.CreatedAt)
//            .Skip((page - 1) * limit).Take(limit)
//            .Select(u => new {
//                u.Id,
//                u.FullName,
//                u.Email,
//                u.Phone,
//                u.Gender,
//                u.IsVerified,
//                u.IsPremium,
//                u.IsActive,
//                u.IsOnline,
//                u.IsDeleted,
//                u.CoinBalance,
//                u.TotalEarned,
//                u.Role,
//                u.CreatedAt,
//                u.LastActiveAt,
//                u.ProfileComplete,
//            }).ToListAsync();
//        return Ok(ApiResponse<object>.Ok(new { users, total, page, limit }));
//    }

//    [HttpGet("users/{id}")]
//    public async Task<IActionResult> GetUser(Guid id)
//    {
//        var user = await _db.Users.IgnoreQueryFilters()
//            .Include(u => u.Location).Include(u => u.Preference)
//            .Include(u => u.Images).Include(u => u.Interests).ThenInclude(i => i.Interest)
//            .Include(u => u.Subscription).ThenInclude(s => s!.Plan)
//            .FirstOrDefaultAsync(u => u.Id == id);
//        if (user == null) return NotFound(ApiResponse<object>.Fail("User not found.", 404));

//        var matchCount = await _db.Matches.IgnoreQueryFilters().CountAsync(m => m.User1Id == id || m.User2Id == id);
//        var msgCount = await _db.Messages.IgnoreQueryFilters().CountAsync(m => m.SenderId == id);
//        var callCount = await _db.CallSessions.CountAsync(c => c.CallerId == id || c.ReceiverId == id);
//        var spentCoins = await _db.CoinTransactions.Where(t => t.UserId == id && t.Direction == "debit").SumAsync(t => (long)t.Coins);
//        var earnedCoins = await _db.CoinTransactions.Where(t => t.UserId == id && t.Direction == "credit").SumAsync(t => (long)t.Coins);
//        var recentTxns = await _db.CoinTransactions.Where(t => t.UserId == id).OrderByDescending(t => t.CreatedAt).Take(20)
//            .Select(t => new { t.Id, t.Coins, t.Direction, t.Description, t.TransactionType, t.CreatedAt }).ToListAsync();
//        var deposits = await _db.DepositRequests.Where(d => d.UserId == id).OrderByDescending(d => d.CreatedAt).Take(10).ToListAsync();
//        var withdrawals = await _db.WithdrawalRequests.Where(w => w.UserId == id).OrderByDescending(w => w.CreatedAt).Take(10).ToListAsync();

//        return Ok(ApiResponse<object>.Ok(new
//        {
//            user = new
//            {
//                user.Id,
//                user.FullName,
//                user.Email,
//                user.Phone,
//                user.Gender,
//                user.IsVerified,
//                user.IsPremium,
//                user.IsActive,
//                user.IsOnline,
//                user.IsDeleted,
//                user.CoinBalance,
//                user.TotalEarned,
//                user.Role,
//                user.CreatedAt,
//                user.LastActiveAt,
//                user.ProfileComplete,
//                user.Bio,
//                user.Avatar,
//                Location = user.Location == null ? null : new { user.Location.City, user.Location.Country, user.Location.Lat, user.Location.Lng },
//                Preference = user.Preference == null ? null : new { user.Preference.InterestedIn, user.Preference.MinAge, user.Preference.MaxAge },
//                Interests = user.Interests.Select(i => i.Interest?.Name),
//                Subscription = user.Subscription == null ? null : new
//                {
//                    PlanName = user.Subscription.Plan?.Name,
//                    user.Subscription.StartDate,
//                    user.Subscription.EndDate,
//                    user.Subscription.IsActive,
//                },
//            },
//            stats = new { matchCount, msgCount, callCount, spentCoins, earnedCoins },
//            recentTxns,
//            deposits,
//            withdrawals,
//        }));
//    }

//    [HttpPost("users/create")]
//    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest req)
//    {
//        if (await _db.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == req.Email))
//            return BadRequest(ApiResponse<object>.Fail("Email already exists."));
//        var user = new User
//        {
//            FullName = req.FullName,
//            Email = req.Email?.ToLower().Trim(),
//            Phone = req.Phone,
//            Gender = req.Gender,
//            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password ?? "Admin@123"),
//            IsVerified = true,
//            IsActive = true,
//            ProfileComplete = true,
//            Role = req.Role ?? "user",
//            CoinBalance = req.InitialCoins,
//        };
//        _db.Users.Add(user);
//        _db.UserPreferences.Add(new UserPreference { UserId = user.Id });
//        await _db.SaveChangesAsync();
//        return StatusCode(201, ApiResponse<object>.Created(new { user.Id, user.Email, user.FullName, user.Role }, "User created."));
//    }

//    [HttpPut("users/{id}/toggle-status")]
//    public async Task<IActionResult> ToggleStatus(Guid id)
//    {
//        var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id)
//            ?? throw new InvalidOperationException("User not found.");
//        user.IsActive = !user.IsActive;
//        user.UpdatedAt = DateTime.UtcNow;
//        await _db.SaveChangesAsync();
//        return Ok(ApiResponse.Ok($"User {(user.IsActive ? "activated" : "deactivated")}."));
//    }

//    [HttpPost("users/{id}/grant-subscription")]
//    public async Task<IActionResult> GrantSubscription(Guid id, [FromBody] GrantSubRequest req)
//    {
//        if (!Guid.TryParse(req.PlanId, out var planId))
//            return BadRequest(ApiResponse<object>.Fail("Invalid plan ID."));
//        var plan = await _db.SubscriptionPlans.FindAsync(planId)
//            ?? throw new InvalidOperationException("Plan not found.");

//        var old = await _db.UserSubscriptions.Where(s => s.UserId == id && s.IsActive).ToListAsync();
//        old.ForEach(s => { s.IsActive = false; s.UpdatedAt = DateTime.UtcNow; });

//        var days = req.Days > 0 ? req.Days : plan.DurationDays;
//        var sub = new UserSubscription { UserId = id, PlanId = planId, EndDate = DateTime.UtcNow.AddDays(days), IsActive = true };
//        _db.UserSubscriptions.Add(sub);
//        var user = await _db.Users.FindAsync(id);
//        if (user != null) { user.IsPremium = true; user.UpdatedAt = DateTime.UtcNow; }
//        await _db.SaveChangesAsync();
//        return Ok(ApiResponse.Ok($"{plan.Name} granted for {days} days."));
//    }

//    [HttpPost("users/{id}/add-coins")]
//    public async Task<IActionResult> AddCoins(Guid id, [FromBody] AddCoinsRequest req)
//    {
//        await _wallet.AddCoinsAsync(id, req.Coins, req.Description ?? "Admin credit", "admin_credit");
//        return Ok(ApiResponse.Ok($"{req.Coins} coins added."));
//    }

//    [HttpGet("users/{id}/chats")]
//    public async Task<IActionResult> GetUserChats(Guid id)
//    {
//        var matches = await _db.Matches.IgnoreQueryFilters()
//            .Include(m => m.Chat).ThenInclude(c => c!.Messages.OrderByDescending(msg => msg.CreatedAt).Take(1))
//            .Include(m => m.User1).Include(m => m.User2)
//            .Where(m => m.User1Id == id || m.User2Id == id).ToListAsync();
//        var result = matches.Select(m => new {
//            matchId = m.Id,
//            chatId = m.Chat?.Id,
//            participant = m.User1Id == id ? new { m.User2!.FullName, m.User2.Avatar } : new { m.User1!.FullName, m.User1.Avatar },
//            lastMessage = m.Chat?.Messages.FirstOrDefault()?.Text,
//            messageCount = m.Chat?.Messages.Count ?? 0,
//        });
//        return Ok(ApiResponse<object>.Ok(new { chats = result }));
//    }

//    [HttpGet("users/{id}/messages")]
//    public async Task<IActionResult> GetUserMessages(Guid id, [FromQuery] Guid? chatId)
//    {
//        var q = _db.Messages.IgnoreQueryFilters()
//            .Include(m => m.Sender).Where(m => m.SenderId == id);
//        if (chatId.HasValue) q = q.Where(m => m.ChatId == chatId.Value);
//        var msgs = await q.OrderByDescending(m => m.CreatedAt).Take(50)
//            .Select(m => new {
//                m.Id,
//                m.Text,
//                m.Type,
//                m.CreatedAt,
//                m.CoinsDeducted,
//                Sender = new { m.Sender!.FullName, m.Sender.Avatar }
//            }).ToListAsync();
//        return Ok(ApiResponse<object>.Ok(new { messages = msgs }));
//    }

//    // ── Deposits ─────────────────────────────────────────────────────────────
//    [HttpGet("deposits")]
//    public async Task<IActionResult> GetDeposits([FromQuery] string status = "pending")
//    {
//        var q = _db.DepositRequests.Include(d => d.User).AsQueryable();
//        if (status != "all") q = q.Where(d => d.Status == status);
//        var list = await q.OrderByDescending(d => d.CreatedAt).Take(100)
//            .Select(d => new {
//                d.Id,
//                d.UtrId,
//                d.ScreenshotUrl,
//                d.RequestedCoins,
//                d.Status,
//                d.AdminNote,
//                d.CreatedAt,
//                User = new { d.User!.FullName, d.User.Email, d.User.CoinBalance },
//            }).ToListAsync();
//        return Ok(ApiResponse<object>.Ok(new { deposits = list }));
//    }

//    [HttpPost("deposits/{id}/approve")]
//    public async Task<IActionResult> ApproveDeposit(Guid id, [FromBody] AdminNoteRequest req)
//    {
//        var dep = await _db.DepositRequests.Include(d => d.User)
//            .FirstOrDefaultAsync(d => d.Id == id && d.Status == "pending")
//            ?? throw new InvalidOperationException("Deposit not found or already processed.");

//        dep.Status = "approved";
//        dep.AdminNote = req.Note;
//        dep.User!.CoinBalance += dep.RequestedCoins ?? 0;
//        dep.User.UpdatedAt = DateTime.UtcNow;

//        _db.CoinTransactions.Add(new CoinTransaction
//        {
//            UserId = dep.UserId,
//            Coins = dep.RequestedCoins ?? 0,
//            Direction = "credit",
//            Description = $"Deposit approved (UTR: {dep.UtrId})",
//            TransactionType = "deposit",
//        });
//        await _db.SaveChangesAsync();
//        return Ok(ApiResponse.Ok($"✅ {dep.RequestedCoins} coins credited to {dep.User.FullName}."));
//    }

//    [HttpPost("deposits/{id}/reject")]
//    public async Task<IActionResult> RejectDeposit(Guid id, [FromBody] AdminNoteRequest req)
//    {
//        var dep = await _db.DepositRequests.FirstOrDefaultAsync(d => d.Id == id && d.Status == "pending")
//            ?? throw new InvalidOperationException("Deposit not found.");
//        dep.Status = "rejected";
//        dep.AdminNote = req.Note;
//        await _db.SaveChangesAsync();
//        return Ok(ApiResponse.Ok("Deposit rejected."));
//    }

//    // ── Withdrawals ──────────────────────────────────────────────────────────
//    [HttpGet("withdrawals")]
//    public async Task<IActionResult> GetWithdrawals([FromQuery] string status = "pending")
//    {
//        var q = _db.WithdrawalRequests.Include(w => w.User).AsQueryable();
//        if (status != "all") q = q.Where(w => w.Status == status);
//        var list = await q.OrderByDescending(w => w.CreatedAt).Take(100)
//            .Select(w => new {
//                w.Id,
//                w.Coins,
//                w.BankOrUpi,
//                w.Status,
//                w.AdminNote,
//                w.CreatedAt,
//                User = new { w.User!.FullName, w.User.Email, w.User.Gender },
//            }).ToListAsync();
//        return Ok(ApiResponse<object>.Ok(new { withdrawals = list }));
//    }

//    [HttpPost("withdrawals/{id}/approve")]
//    public async Task<IActionResult> ApproveWithdrawal(Guid id, [FromBody] AdminNoteRequest req)
//    {
//        var wr = await _db.WithdrawalRequests.FirstOrDefaultAsync(w => w.Id == id && w.Status == "pending")
//            ?? throw new InvalidOperationException("Withdrawal not found.");
//        wr.Status = "approved";
//        wr.AdminNote = req.Note;
//        await _db.SaveChangesAsync();
//        return Ok(ApiResponse.Ok("Withdrawal approved. Process the payment manually."));
//    }

//    [HttpPost("withdrawals/{id}/reject")]
//    public async Task<IActionResult> RejectWithdrawal(Guid id, [FromBody] AdminNoteRequest req)
//    {
//        var wr = await _db.WithdrawalRequests.Include(w => w.User)
//            .FirstOrDefaultAsync(w => w.Id == id && w.Status == "pending")
//            ?? throw new InvalidOperationException("Withdrawal not found.");
//        wr.Status = "rejected";
//        wr.AdminNote = req.Note;
//        wr.User!.CoinBalance += wr.Coins;
//        wr.User.UpdatedAt = DateTime.UtcNow;
//        _db.CoinTransactions.Add(new CoinTransaction
//        {
//            UserId = wr.UserId,
//            Coins = wr.Coins,
//            Direction = "credit",
//            Description = "Withdrawal rejected — coins refunded",
//            TransactionType = "refund",
//        });
//        await _db.SaveChangesAsync();
//        return Ok(ApiResponse.Ok("Withdrawal rejected and coins refunded."));
//    }

//    // ── Reports ──────────────────────────────────────────────────────────────
//    [HttpGet("reports")]
//    public async Task<IActionResult> GetReports([FromQuery] string status = "pending")
//    {
//        // FIX: correct nav property is ReportedUser (not Reported)
//        var q = _db.Reports.Include(r => r.Reporter).Include(r => r.ReportedUser).AsQueryable();
//        if (status != "all") q = q.Where(r => r.Status == status);
//        var list = await q.OrderByDescending(r => r.CreatedAt).Take(100)
//            .Select(r => new {
//                r.Id,
//                r.Reason,
//                r.Status,
//                r.CreatedAt,
//                Reporter = new { r.Reporter!.FullName, r.Reporter.Email },
//                ReportedUser = new { r.ReportedUser!.FullName, r.ReportedUser.Email },
//            }).ToListAsync();
//        return Ok(ApiResponse<object>.Ok(new { reports = list }));
//    }

//    [HttpPost("reports/{id}/resolve")]
//    public async Task<IActionResult> ResolveReport(Guid id, [FromBody] AdminNoteRequest req)
//    {
//        var report = await _db.Reports.FindAsync(id)
//            ?? throw new InvalidOperationException("Report not found.");
//        report.Status = "resolved";
//        report.UpdatedAt = DateTime.UtcNow;
//        await _db.SaveChangesAsync();
//        return Ok(ApiResponse.Ok("Report resolved."));
//    }

//    // ── SuperChats ───────────────────────────────────────────────────────────
//    [HttpGet("superchat")]
//    public async Task<IActionResult> GetSuperChats([FromQuery] int page = 1)
//    {
//        // FIX: correct nav properties are FromUser/ToUser (not Sender/Receiver)
//        // FIX: correct field names are CoinAmount (not CostCoins), IsResponded (not Status)
//        var list = await _db.SuperChats.Include(s => s.FromUser).Include(s => s.ToUser)
//            .OrderByDescending(s => s.CreatedAt).Skip((page - 1) * 30).Take(30)
//            .Select(s => new {
//                s.Id,
//                s.Message,
//                s.CoinAmount,
//                s.CompanyRevenue,
//                s.GirlCommission,
//                s.IsResponded,
//                s.CreatedAt,
//                FromUser = new { s.FromUser!.FullName },
//                ToUser = new { s.ToUser!.FullName },
//            }).ToListAsync();
//        return Ok(ApiResponse<object>.Ok(new { superChats = list }));
//    }

//    // ── Notifications ────────────────────────────────────────────────────────
//    [HttpPost("notifications/broadcast")]
//    public async Task<IActionResult> Broadcast([FromBody] BroadcastRequest req)
//    {
//        var users = await _db.Users.Select(u => u.Id).ToListAsync();
//        var notifs = users.Select(uid => new Notification
//        {
//            UserId = uid,
//            Title = req.Title,
//            Body = req.Body,
//            Type = "broadcast",
//        }).ToList();
//        _db.Notifications.AddRange(notifs);
//        await _db.SaveChangesAsync();
//        return Ok(ApiResponse.Ok($"Notification sent to {users.Count} users."));
//    }

//    [HttpPost("users/{id}/notify")]
//    public async Task<IActionResult> NotifyUser(Guid id, [FromBody] BroadcastRequest req)
//    {
//        _db.Notifications.Add(new Notification { UserId = id, Title = req.Title, Body = req.Body, Type = "admin" });
//        await _db.SaveChangesAsync();
//        return Ok(ApiResponse.Ok("Notification sent."));
//    }

//    // ── Coin Stats ───────────────────────────────────────────────────────────
//    [HttpGet("stats/coins")]
//    public async Task<IActionResult> CoinStats()
//    {
//        var totalInCirculation = await _db.Users.SumAsync(u => (long)u.CoinBalance);
//        var totalIssued = await _db.CoinTransactions.Where(t => t.Direction == "credit").SumAsync(t => (long)t.Coins);
//        var totalBurned = await _db.CoinTransactions.Where(t => t.Direction == "debit").SumAsync(t => (long)t.Coins);
//        var pendingDeps = await _db.DepositRequests.Where(d => d.Status == "pending").SumAsync(d => (long)(d.RequestedCoins ?? 0));
//        return Ok(ApiResponse<object>.Ok(new { totalInCirculation, totalIssued, totalBurned, pendingDeps }));
//    }
//}

//// ── Request DTOs ─────────────────────────────────────────────────────────────
//public class AdminNoteRequest { public string? Note { get; set; } }
//public class GrantSubRequest { public string PlanId { get; set; } = ""; public int Days { get; set; } }
//public class AddCoinsRequest { public int Coins { get; set; } public string? Description { get; set; } }
//public class CreateUserRequest
//{
//    public string? FullName { get; set; }
//    public string? Email { get; set; }
//    public string? Phone { get; set; }
//    public string? Gender { get; set; }
//    public string? Password { get; set; }
//    public string? Role { get; set; }
//    public int InitialCoins { get; set; } = 100;
//}
//public class BroadcastRequest { public string Title { get; set; } = ""; public string Body { get; set; } = ""; }
