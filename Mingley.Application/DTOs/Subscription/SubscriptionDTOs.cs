namespace Mingley.Application.DTOs.Subscription;

public class SubscriptionPlanDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public int DurationDays { get; set; }
    public string? Features { get; set; }
    public bool IsPopular { get; set; }
    public int SuperLikesPerDay { get; set; }
    public int BoostsPerMonth { get; set; }
    public bool UnlimitedLikes { get; set; }
    public bool CanSeeWhoLiked { get; set; }
    public bool VideoCallEnabled { get; set; }
}

public class SubscribeRequest
{
    public string PlanId { get; set; } = string.Empty;
    public bool AutoRenew { get; set; } = true;
    // NEW: optional payment fields — accept whatever frontend sends
    public string? PaymentMethod { get; set; }   // "razorpay" | "coins" | "manual"
    public string? PaymentId { get; set; }
    public string? OrderId { get; set; }
    public string? Signature { get; set; }
}

public class SubscribeResponse
{
    public string? SubscriptionId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? PlanName { get; set; }
    public int DaysRemaining { get; set; }
}

public class UserSubscriptionDto
{
    public string? Id { get; set; }
    public string? PlanName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool AutoRenew { get; set; }
    public int DaysRemaining { get; set; }
}


//namespace Mingley.Application.DTOs.Subscription;

//public class SubscriptionPlanDto
//{
//    public string? Id { get; set; }
//    public string? Name { get; set; }
//    public decimal Price { get; set; }
//    public int DurationDays { get; set; }
//    public string? Features { get; set; } // JSON
//    public bool IsPopular { get; set; }
//    public int SuperLikesPerDay { get; set; }
//    public int BoostsPerMonth { get; set; }
//    public bool UnlimitedLikes { get; set; }
//    public bool CanSeeWhoLiked { get; set; }
//    public bool VideoCallEnabled { get; set; }
//}

//public class SubscribeRequest
//{
//    public string PlanId { get; set; } = string.Empty;
//    public bool AutoRenew { get; set; } = true;
//}

//public class SubscribeResponse
//{
//    public string? SubscriptionId { get; set; }
//    public DateTime StartDate { get; set; }
//    public DateTime EndDate { get; set; }
//    public bool IsActive { get; set; }
//    public string? PlanName { get; set; }
//    public int DaysRemaining { get; set; }
//}

//public class UserSubscriptionDto
//{
//    public string? Id { get; set; }
//    public string? PlanName { get; set; }
//    public DateTime StartDate { get; set; }
//    public DateTime EndDate { get; set; }
//    public bool IsActive { get; set; }
//    public bool AutoRenew { get; set; }
//    public int DaysRemaining { get; set; }
//}
