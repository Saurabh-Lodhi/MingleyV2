using Microsoft.EntityFrameworkCore;
using Mingley.Application.DTOs.Subscription;
using Mingley.Application.Interfaces;
using Mingley.Domain.Entities;
using Mingley.Infrastructure.Persistence;

namespace Mingley.Infrastructure.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly MingleyDbContext _db;
    private readonly INotificationService _notifs;
    private readonly IWalletService _wallet;

    public SubscriptionService(MingleyDbContext db, INotificationService notifs, IWalletService wallet)
    { _db = db; _notifs = notifs; _wallet = wallet; }

    public async Task<List<SubscriptionPlanDto>> GetPlansAsync()
    {
        return await _db.SubscriptionPlans
            .Where(p => p.IsActive && !p.IsDeleted)
            .OrderBy(p => p.Price)
            .Select(p => new SubscriptionPlanDto
            {
                Id = p.Id.ToString(),
                Name = p.Name,
                Price = p.Price,
                DurationDays = p.DurationDays,
                Features = p.Features,
                IsPopular = p.IsPopular,
                SuperLikesPerDay = p.SuperLikesPerDay,
                BoostsPerMonth = p.BoostsPerMonth,
                UnlimitedLikes = p.UnlimitedLikes,
                CanSeeWhoLiked = p.CanSeeWhoLiked,
                VideoCallEnabled = p.VideoCallEnabled,
            }).ToListAsync();
    }

    public async Task<UserSubscriptionDto?> GetStatusAsync(Guid userId)
    {
        var sub = await _db.UserSubscriptions.Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive && s.EndDate > DateTime.UtcNow);
        if (sub == null) return null;
        return new UserSubscriptionDto
        {
            Id = sub.Id.ToString(),
            PlanName = sub.Plan?.Name,
            StartDate = sub.StartDate,
            EndDate = sub.EndDate,
            IsActive = sub.IsActive,
            AutoRenew = sub.AutoRenew,
            DaysRemaining = (int)(sub.EndDate - DateTime.UtcNow).TotalDays,
        };
    }

    public async Task<SubscribeResponse> SubscribeAsync(Guid userId, SubscribeRequest req)
    {
        // FIX: look up by GUID first, then fall back to plan name match
        SubscriptionPlan? plan = null;

        if (Guid.TryParse(req.PlanId, out var planGuid))
            plan = await _db.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Id == planGuid && p.IsActive && !p.IsDeleted);

        if (plan == null)
            plan = await _db.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.IsActive && !p.IsDeleted &&
                    p.Name.ToLower() == req.PlanId.ToLower().Trim());

        if (plan == null)
            throw new InvalidOperationException(
                $"Plan '{req.PlanId}' not found. Call GET /v1/subscriptions/plans for valid IDs.");

        // Handle coin-based payment
        var method = req.PaymentMethod?.ToLower() ?? "manual";
        if (method == "coins")
        {
            var coinCost = (int)(plan.Price * 10); // 1 INR = 10 coins
            var coinUser = await _db.Users.FindAsync(userId)
                ?? throw new InvalidOperationException("User not found.");
            if (coinUser.CoinBalance < coinCost)
                throw new InvalidOperationException(
                    $"Insufficient coins. Need {coinCost} coins for {plan.Name}. You have {coinUser.CoinBalance}.");
            await _wallet.DeductCoinsAsync(userId, coinCost,
                $"Subscription: {plan.Name}", "subscription", plan.Id.ToString());
        }

        // Deactivate any existing active subscription
        var old = await _db.UserSubscriptions
            .Where(s => s.UserId == userId && s.IsActive).ToListAsync();
        old.ForEach(s => { s.IsActive = false; s.UpdatedAt = DateTime.UtcNow; });

        var endDate = DateTime.UtcNow.AddDays(plan.DurationDays);
        var sub = new UserSubscription
        {
            UserId = userId,
            PlanId = plan.Id,
            EndDate = endDate,
            AutoRenew = req.AutoRenew,
            IsActive = true,
        };
        _db.UserSubscriptions.Add(sub);

        var user = await _db.Users.FindAsync(userId);
        if (user != null) { user.IsPremium = true; user.UpdatedAt = DateTime.UtcNow; }

        await _db.SaveChangesAsync();

        await _notifs.CreateAsync(userId, $"🌟 {plan.Name} Activated!",
            $"Your {plan.Name} is active until {endDate:dd MMM yyyy}. Enjoy premium features!", "subscription");

        return new SubscribeResponse
        {
            SubscriptionId = sub.Id.ToString(),
            PlanName = plan.Name,
            StartDate = sub.StartDate,
            EndDate = endDate,
            IsActive = true,
            DaysRemaining = (int)(endDate - DateTime.UtcNow).TotalDays,
        };
    }

    public async Task CancelAsync(Guid userId, Guid subscriptionId, string? reason)
    {
        var sub = await _db.UserSubscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.UserId == userId && s.IsActive)
            ?? throw new InvalidOperationException("Active subscription not found.");
        sub.IsActive = false; sub.AutoRenew = false;
        sub.CancelReason = reason; sub.UpdatedAt = DateTime.UtcNow;
        var user = await _db.Users.FindAsync(userId);
        if (user != null) { user.IsPremium = false; user.UpdatedAt = DateTime.UtcNow; }
        await _db.SaveChangesAsync();
    }
}

//using Microsoft.EntityFrameworkCore;
//using Mingley.Application.DTOs.Subscription;
//using Mingley.Application.Interfaces;
//using Mingley.Domain.Entities;
//using Mingley.Infrastructure.Persistence;

//namespace Mingley.Infrastructure.Services;

//public class SubscriptionService : ISubscriptionService
//{
//    private readonly MingleyDbContext _db;
//    private readonly INotificationService _notifs;

//    public SubscriptionService(MingleyDbContext db, INotificationService notifs)
//    { _db = db; _notifs = notifs; }

//    public async Task<List<SubscriptionPlanDto>> GetPlansAsync()
//    {
//        return await _db.SubscriptionPlans
//            .Where(p => p.IsActive && !p.IsDeleted)
//            .OrderBy(p => p.Price)
//            .Select(p => new SubscriptionPlanDto
//            {
//                Id = p.Id.ToString(), Name = p.Name, Price = p.Price, DurationDays = p.DurationDays,
//                Features = p.Features, IsPopular = p.IsPopular, SuperLikesPerDay = p.SuperLikesPerDay,
//                BoostsPerMonth = p.BoostsPerMonth, UnlimitedLikes = p.UnlimitedLikes,
//                CanSeeWhoLiked = p.CanSeeWhoLiked, VideoCallEnabled = p.VideoCallEnabled,
//            }).ToListAsync();
//    }

//    public async Task<UserSubscriptionDto?> GetStatusAsync(Guid userId)
//    {
//        var sub = await _db.UserSubscriptions.Include(s => s.Plan)
//            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive && s.EndDate > DateTime.UtcNow);
//        if (sub == null) return null;
//        return new UserSubscriptionDto
//        {
//            Id = sub.Id.ToString(), PlanName = sub.Plan?.Name, StartDate = sub.StartDate, EndDate = sub.EndDate,
//            IsActive = sub.IsActive, AutoRenew = sub.AutoRenew,
//            DaysRemaining = (int)(sub.EndDate - DateTime.UtcNow).TotalDays,
//        };
//    }

//    public async Task<SubscribeResponse> SubscribeAsync(Guid userId, SubscribeRequest req)
//    {
//        if (!Guid.TryParse(req.PlanId, out var planId))
//            throw new InvalidOperationException("Invalid plan ID.");
//        var plan = await _db.SubscriptionPlans.FindAsync(planId)
//            ?? throw new InvalidOperationException("Plan not found.");

//        // Deactivate existing subs
//        var old = await _db.UserSubscriptions.Where(s => s.UserId == userId && s.IsActive).ToListAsync();
//        old.ForEach(s => { s.IsActive = false; s.UpdatedAt = DateTime.UtcNow; });

//        var endDate = DateTime.UtcNow.AddDays(plan.DurationDays);
//        var sub = new UserSubscription { UserId = userId, PlanId = planId, EndDate = endDate, AutoRenew = req.AutoRenew, IsActive = true };
//        _db.UserSubscriptions.Add(sub);

//        var user = await _db.Users.FindAsync(userId);
//        if (user != null) { user.IsPremium = true; user.UpdatedAt = DateTime.UtcNow; }
//        await _db.SaveChangesAsync();

//        await _notifs.CreateAsync(userId, $"🌟 {plan.Name} Activated!",
//            $"Your {plan.Name} plan is active until {endDate:dd MMM yyyy}. Enjoy premium features!", "subscription");

//        return new SubscribeResponse
//        {
//            SubscriptionId = sub.Id.ToString(), PlanName = plan.Name,
//            StartDate = sub.StartDate, EndDate = endDate, IsActive = true,
//            DaysRemaining = (int)(endDate - DateTime.UtcNow).TotalDays,
//        };
//    }

//    public async Task CancelAsync(Guid userId, Guid subscriptionId, string? reason)
//    {
//        var sub = await _db.UserSubscriptions
//            .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.UserId == userId && s.IsActive)
//            ?? throw new InvalidOperationException("Active subscription not found.");
//        sub.IsActive = false; sub.AutoRenew = false; sub.CancelReason = reason; sub.UpdatedAt = DateTime.UtcNow;
//        var user = await _db.Users.FindAsync(userId);
//        if (user != null) { user.IsPremium = false; user.UpdatedAt = DateTime.UtcNow; }
//        await _db.SaveChangesAsync();
//    }
//}
