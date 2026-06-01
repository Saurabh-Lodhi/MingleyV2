using Mingley.Domain.Common;

namespace Mingley.Domain.Entities;

// ════════════════════════════════════════════════════════════
// USER & PROFILE
// ════════════════════════════════════════════════════════════
public class User : BaseEntity
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? PasswordHash { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Bio { get; set; }
    public string? Profession { get; set; }  // Working Professional / Student / Business / Freelancer / Other
    public string? Avatar { get; set; }
    public string Role { get; set; } = "user";
    public bool IsVerified { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public bool IsPremium { get; set; } = false;
    public int CoinBalance { get; set; } = 0;
    public double TotalEarned { get; set; } = 0;
    public bool TwoFactorEnabled { get; set; } = false;
    public string? TwoFactorSecret { get; set; }
    public DateTime? LastActiveAt { get; set; }
    public bool IsOnline { get; set; } = false;
    public string? OtpCode { get; set; }
    public DateTime? OtpExpiry { get; set; }
    public string? OtpPurpose { get; set; }
    public string? FcmToken { get; set; }
    public bool ProfileComplete { get; set; } = false;

    // Location lock + Travel Mode (Premium feature)
    public bool IsLocationLocked { get; set; } = true;
    public bool IsTravelMode { get; set; } = false;
    public string? TravelCity { get; set; }
    public double? TravelLat { get; set; }
    public double? TravelLng { get; set; }

    // Admin management
    public bool IsCreatedByAdmin { get; set; } = false;
    public bool IsSuspended { get; set; } = false;
    public DateTime? SuspendedAt { get; set; }
    public string? SuspendReason { get; set; }
    public string? SuspendedBy { get; set; }

    // Navigation
    public UserLocation? Location { get; set; }
    public UserPreference? Preference { get; set; }
    public UserSubscription? Subscription { get; set; }
    public ICollection<UserImage> Images { get; set; } = new List<UserImage>();
    public ICollection<UserInterest> Interests { get; set; } = new List<UserInterest>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

public class UserLocation : BaseEntity
{
    public Guid UserId { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public User? User { get; set; }
}

public class UserPreference : BaseEntity
{
    public Guid UserId { get; set; }
    public string InterestedIn { get; set; } = "both";
    public int MinAge { get; set; } = 18;
    public int MaxAge { get; set; } = 40;
    public int MaxDistance { get; set; } = 50;
    public string RelationshipType { get; set; } = "both";
    public bool NearbyOnly { get; set; } = false;
    public bool OnlineOnly { get; set; } = false;
    public bool VerifiedOnly { get; set; } = false;
    public string? Location { get; set; }
    public User? User { get; set; }
}

public class UserImage : BaseEntity
{
    public Guid UserId { get; set; }
    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
    public bool IsPrimary { get; set; } = false;
    public User? User { get; set; }
}

public class UserInterest
{
    public Guid UserId { get; set; }
    public Guid InterestId { get; set; }
    public User? User { get; set; }
    public Interest? Interest { get; set; }
}

public class Interest : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Emoji { get; set; }
    public ICollection<UserInterest> UserInterests { get; set; } = new List<UserInterest>();
}

// ════════════════════════════════════════════════════════════
// AUTH — REFRESH TOKENS (DB-backed, survives restarts)
// ════════════════════════════════════════════════════════════
public class RefreshToken
{
    public int Id { get; set; }          // auto-increment PK
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User? User { get; set; }
}

// ════════════════════════════════════════════════════════════
// MATCHING & SWIPING
// ════════════════════════════════════════════════════════════
public class Swipe : BaseEntity
{
    public Guid SwiperId { get; set; }
    public Guid TargetId { get; set; }
    public string Action { get; set; } = "like";     // like | dislike | superlike
    public User? Swiper { get; set; }
    public User? Target { get; set; }
}

public class Match : BaseEntity
{
    public Guid User1Id { get; set; }
    public Guid User2Id { get; set; }
    public bool IsActive { get; set; } = true;
    public User? User1 { get; set; }
    public User? User2 { get; set; }
    public Chat? Chat { get; set; }
}

// ════════════════════════════════════════════════════════════
// CHAT & MESSAGES
// ════════════════════════════════════════════════════════════
public class Chat : BaseEntity
{
    public Guid MatchId { get; set; }
    public Match? Match { get; set; }
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

public class Message : BaseEntity
{
    public Guid ChatId { get; set; }
    public Guid SenderId { get; set; }
    public string? Text { get; set; }
    public string Type { get; set; } = "text";       // text | image | gift | coins | system
    public string? ImageUrl { get; set; }
    public string? GiftName { get; set; }
    public int? GiftCost { get; set; }
    public int? CoinAmount { get; set; }
    public DateTime? ReadAt { get; set; }
    public int CoinsDeducted { get; set; } = 0;
    public Guid? ReplyToMessageId { get; set; }
    public Chat? Chat { get; set; }
    public User? Sender { get; set; }
    public Message? ReplyToMessage { get; set; }
}

// ════════════════════════════════════════════════════════════
// CALLS
// ════════════════════════════════════════════════════════════
public class CallSession : BaseEntity
{
    public Guid CallerId { get; set; }
    public Guid ReceiverId { get; set; }
    public Guid MatchId { get; set; }
    public string CallType { get; set; } = "audio";  // audio | video
    public string Status { get; set; } = "ringing";  // ringing | active | ended | declined | timeout | cancelled
    public DateTime? AnsweredAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int? DurationSeconds { get; set; }
    public int CoinsDeducted { get; set; } = 0;
    public string? EndReason { get; set; }
    public User? Caller { get; set; }
    public User? Receiver { get; set; }
    public Match? Match { get; set; }
}

// ════════════════════════════════════════════════════════════
// SUPERCHAT
// ════════════════════════════════════════════════════════════
public class SuperChat : BaseEntity
{
    public Guid FromUserId { get; set; }
    public Guid ToUserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public int CoinAmount { get; set; } = 500;
    public double GirlCommission { get; set; }
    public double CompanyRevenue { get; set; }
    public bool IsResponded { get; set; } = false;
    public DateTime? RespondedAt { get; set; }
    public Guid? MatchCreatedId { get; set; }
    public User? FromUser { get; set; }
    public User? ToUser { get; set; }
    public Match? MatchCreated { get; set; }
}

// ════════════════════════════════════════════════════════════
// COINS & WALLET
// ════════════════════════════════════════════════════════════
public class CoinTransaction : BaseEntity
{
    public Guid UserId { get; set; }
    public int Coins { get; set; }
    public string Direction { get; set; } = "credit"; // credit | debit
    public string? Description { get; set; }
    public string? TransactionType { get; set; }      // message | gift | deposit | withdrawal | call | superlike | superchat | admin | verification
    public string? ReferenceId { get; set; }
    public User? User { get; set; }
}

public class DepositRequest : BaseEntity
{
    public Guid UserId { get; set; }
    public string? UtrId { get; set; }
    public string? ScreenshotUrl { get; set; }
    public int? RequestedCoins { get; set; }
    public string Status { get; set; } = "pending";   // pending | approved | rejected
    public string? AdminNote { get; set; }
    public User? User { get; set; }
}

public class WithdrawalRequest : BaseEntity
{
    public Guid UserId { get; set; }
    public int Coins { get; set; }
    public string? BankOrUpi { get; set; }
    public string Status { get; set; } = "pending";   // pending | approved | rejected
    public string? AdminNote { get; set; }
    public User? User { get; set; }
}

// ════════════════════════════════════════════════════════════
// SUBSCRIPTIONS
// ════════════════════════════════════════════════════════════
public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationDays { get; set; }
    public string? Features { get; set; }
    public bool IsPopular { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int SuperLikesPerDay { get; set; } = 1;
    public int BoostsPerMonth { get; set; } = 0;
    public bool UnlimitedLikes { get; set; } = false;
    public bool CanSeeWhoLiked { get; set; } = false;
    public bool VideoCallEnabled { get; set; } = false;
}

public class UserSubscription : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid PlanId { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool AutoRenew { get; set; } = true;
    public string? CancelReason { get; set; }
    public Guid? GrantedBy { get; set; }
    public User? User { get; set; }
    public SubscriptionPlan? Plan { get; set; }
}

// ════════════════════════════════════════════════════════════
// NOTIFICATIONS
// ════════════════════════════════════════════════════════════
public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? Type { get; set; }                 // match | message | like | gift | call | superchat | system
    public bool IsRead { get; set; } = false;
    public string? ReferenceId { get; set; }
    public User? User { get; set; }
}

// ════════════════════════════════════════════════════════════
// SAFETY & ADMIN
// ════════════════════════════════════════════════════════════
public class Report : BaseEntity
{
    public Guid ReporterId { get; set; }
    public Guid ReportedUserId { get; set; }
    public string? Reason { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = "pending";   // pending | reviewed | dismissed
    public string? AdminNote { get; set; }
    public User? Reporter { get; set; }
    public User? ReportedUser { get; set; }
}

public class Block : BaseEntity
{
    public Guid BlockerId { get; set; }
    public Guid BlockedUserId { get; set; }
    public User? Blocker { get; set; }
    public User? BlockedUser { get; set; }
}

public class PrivacyAgreement : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid MatchId { get; set; }
    public bool Accepted { get; set; } = true;
    public User? User { get; set; }
}

// ════════════════════════════════════════════════════════════
// GIFTS
// ════════════════════════════════════════════════════════════
public class Gift : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Emoji { get; set; }
    public int CoinCost { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Category { get; set; }             // standard | romantic | fun | animated | luxury | vip
    public string? ImageUrl { get; set; }
    public bool IsAnimated { get; set; } = false;
}

//using Mingley.Domain.Common;

//namespace Mingley.Domain.Entities;

//// ════════════════════════════════════════════════════════════
//// USER & PROFILE
//// ════════════════════════════════════════════════════════════
//public class User : BaseEntity
//{
//    public string? FullName { get; set; }
//    public string? Email { get; set; }
//    public string? Phone { get; set; }
//    public string? PasswordHash { get; set; }
//    public string? Gender { get; set; }
//    public DateTime? DateOfBirth { get; set; }
//    public string? Bio { get; set; }
//    public string? Avatar { get; set; }
//    public string Role { get; set; } = "user";
//    public bool IsVerified { get; set; } = false;
//    public bool IsActive { get; set; } = true;
//    public bool IsPremium { get; set; } = false;
//    public int CoinBalance { get; set; } = 0;
//    public double TotalEarned { get; set; } = 0;
//    public bool TwoFactorEnabled { get; set; } = false;
//    public string? TwoFactorSecret { get; set; }
//    public DateTime? LastActiveAt { get; set; }
//    public bool IsOnline { get; set; } = false;
//    public string? OtpCode { get; set; }
//    public DateTime? OtpExpiry { get; set; }
//    public string? OtpPurpose { get; set; }
//    public string? FcmToken { get; set; }
//    public bool ProfileComplete { get; set; } = false;

//    // Admin management
//    public bool IsCreatedByAdmin { get; set; } = false;
//    public bool IsSuspended { get; set; } = false;
//    public DateTime? SuspendedAt { get; set; }
//    public string? SuspendReason { get; set; }
//    public string? SuspendedBy { get; set; }

//    // Navigation
//    public UserLocation? Location { get; set; }
//    public UserPreference? Preference { get; set; }
//    public UserSubscription? Subscription { get; set; }
//    public ICollection<UserImage> Images { get; set; } = new List<UserImage>();
//    public ICollection<UserInterest> Interests { get; set; } = new List<UserInterest>();
//    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
//}

//public class UserLocation : BaseEntity
//{
//    public Guid UserId { get; set; }
//    public double? Lat { get; set; }
//    public double? Lng { get; set; }
//    public string? City { get; set; }
//    public string? Country { get; set; }
//    public User? User { get; set; }
//}

//public class UserPreference : BaseEntity
//{
//    public Guid UserId { get; set; }
//    public string InterestedIn { get; set; } = "both";
//    public int MinAge { get; set; } = 18;
//    public int MaxAge { get; set; } = 40;
//    public int MaxDistance { get; set; } = 50;
//    public string RelationshipType { get; set; } = "both";
//    public bool NearbyOnly { get; set; } = false;
//    public bool OnlineOnly { get; set; } = false;
//    public bool VerifiedOnly { get; set; } = false;
//    public string? Location { get; set; }
//    public User? User { get; set; }
//}

//public class UserImage : BaseEntity
//{
//    public Guid UserId { get; set; }
//    public string Url { get; set; } = string.Empty;
//    public int SortOrder { get; set; } = 0;
//    public bool IsPrimary { get; set; } = false;
//    public User? User { get; set; }
//}

//public class UserInterest
//{
//    public Guid UserId { get; set; }
//    public Guid InterestId { get; set; }
//    public User? User { get; set; }
//    public Interest? Interest { get; set; }
//}

//public class Interest : BaseEntity
//{
//    public string Name { get; set; } = string.Empty;
//    public string? Icon { get; set; }
//    public string? Emoji { get; set; }
//    public ICollection<UserInterest> UserInterests { get; set; } = new List<UserInterest>();
//}

//// ════════════════════════════════════════════════════════════
//// AUTH — REFRESH TOKENS (DB-backed, survives restarts)
//// ════════════════════════════════════════════════════════════
//public class RefreshToken
//{
//    public int Id { get; set; }          // auto-increment PK
//    public string Token { get; set; } = string.Empty;
//    public Guid UserId { get; set; }
//    public DateTime ExpiresAt { get; set; }
//    public bool IsRevoked { get; set; } = false;
//    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

//    // Navigation
//    public User? User { get; set; }
//}

//// ════════════════════════════════════════════════════════════
//// MATCHING & SWIPING
//// ════════════════════════════════════════════════════════════
//public class Swipe : BaseEntity
//{
//    public Guid SwiperId { get; set; }
//    public Guid TargetId { get; set; }
//    public string Action { get; set; } = "like";     // like | dislike | superlike
//    public User? Swiper { get; set; }
//    public User? Target { get; set; }
//}

//public class Match : BaseEntity
//{
//    public Guid User1Id { get; set; }
//    public Guid User2Id { get; set; }
//    public bool IsActive { get; set; } = true;
//    public User? User1 { get; set; }
//    public User? User2 { get; set; }
//    public Chat? Chat { get; set; }
//}

//// ════════════════════════════════════════════════════════════
//// CHAT & MESSAGES
//// ════════════════════════════════════════════════════════════
//public class Chat : BaseEntity
//{
//    public Guid MatchId { get; set; }
//    public Match? Match { get; set; }
//    public ICollection<Message> Messages { get; set; } = new List<Message>();
//}

//public class Message : BaseEntity
//{
//    public Guid ChatId { get; set; }
//    public Guid SenderId { get; set; }
//    public string? Text { get; set; }
//    public string Type { get; set; } = "text";       // text | image | gift | coins | system
//    public string? ImageUrl { get; set; }
//    public string? GiftName { get; set; }
//    public int? GiftCost { get; set; }
//    public int? CoinAmount { get; set; }
//    public DateTime? ReadAt { get; set; }
//    public int CoinsDeducted { get; set; } = 0;
//    public Guid? ReplyToMessageId { get; set; }
//    public Chat? Chat { get; set; }
//    public User? Sender { get; set; }
//    public Message? ReplyToMessage { get; set; }
//}

//// ════════════════════════════════════════════════════════════
//// CALLS
//// ════════════════════════════════════════════════════════════
//public class CallSession : BaseEntity
//{
//    public Guid CallerId { get; set; }
//    public Guid ReceiverId { get; set; }
//    public Guid MatchId { get; set; }
//    public string CallType { get; set; } = "audio";  // audio | video
//    public string Status { get; set; } = "ringing";  // ringing | active | ended | declined | timeout | cancelled
//    public DateTime? AnsweredAt { get; set; }
//    public DateTime? EndedAt { get; set; }
//    public int? DurationSeconds { get; set; }
//    public int CoinsDeducted { get; set; } = 0;
//    public string? EndReason { get; set; }
//    public User? Caller { get; set; }
//    public User? Receiver { get; set; }
//    public Match? Match { get; set; }
//}

//// ════════════════════════════════════════════════════════════
//// SUPERCHAT
//// ════════════════════════════════════════════════════════════
//public class SuperChat : BaseEntity
//{
//    public Guid FromUserId { get; set; }
//    public Guid ToUserId { get; set; }
//    public string Message { get; set; } = string.Empty;
//    public int CoinAmount { get; set; } = 500;
//    public double GirlCommission { get; set; }
//    public double CompanyRevenue { get; set; }
//    public bool IsResponded { get; set; } = false;
//    public DateTime? RespondedAt { get; set; }
//    public Guid? MatchCreatedId { get; set; }
//    public User? FromUser { get; set; }
//    public User? ToUser { get; set; }
//    public Match? MatchCreated { get; set; }
//}

//// ════════════════════════════════════════════════════════════
//// COINS & WALLET
//// ════════════════════════════════════════════════════════════
//public class CoinTransaction : BaseEntity
//{
//    public Guid UserId { get; set; }
//    public int Coins { get; set; }
//    public string Direction { get; set; } = "credit"; // credit | debit
//    public string? Description { get; set; }
//    public string? TransactionType { get; set; }      // message | gift | deposit | withdrawal | call | superlike | superchat | admin | verification
//    public string? ReferenceId { get; set; }
//    public User? User { get; set; }
//}

//public class DepositRequest : BaseEntity
//{
//    public Guid UserId { get; set; }
//    public string? UtrId { get; set; }
//    public string? ScreenshotUrl { get; set; }
//    public int? RequestedCoins { get; set; }
//    public string Status { get; set; } = "pending";   // pending | approved | rejected
//    public string? AdminNote { get; set; }
//    public User? User { get; set; }
//}

//public class WithdrawalRequest : BaseEntity
//{
//    public Guid UserId { get; set; }
//    public int Coins { get; set; }
//    public string? BankOrUpi { get; set; }
//    public string Status { get; set; } = "pending";   // pending | approved | rejected
//    public string? AdminNote { get; set; }
//    public User? User { get; set; }
//}

//// ════════════════════════════════════════════════════════════
//// SUBSCRIPTIONS
//// ════════════════════════════════════════════════════════════
//public class SubscriptionPlan : BaseEntity
//{
//    public string Name { get; set; } = string.Empty;
//    public decimal Price { get; set; }
//    public int DurationDays { get; set; }
//    public string? Features { get; set; }
//    public bool IsPopular { get; set; } = false;
//    public bool IsActive { get; set; } = true;
//    public int SuperLikesPerDay { get; set; } = 1;
//    public int BoostsPerMonth { get; set; } = 0;
//    public bool UnlimitedLikes { get; set; } = false;
//    public bool CanSeeWhoLiked { get; set; } = false;
//    public bool VideoCallEnabled { get; set; } = false;
//}

//public class UserSubscription : BaseEntity
//{
//    public Guid UserId { get; set; }
//    public Guid PlanId { get; set; }
//    public DateTime StartDate { get; set; } = DateTime.UtcNow;
//    public DateTime EndDate { get; set; }
//    public bool IsActive { get; set; } = true;
//    public bool AutoRenew { get; set; } = true;
//    public string? CancelReason { get; set; }
//    public Guid? GrantedBy { get; set; }
//    public User? User { get; set; }
//    public SubscriptionPlan? Plan { get; set; }
//}

//// ════════════════════════════════════════════════════════════
//// NOTIFICATIONS
//// ════════════════════════════════════════════════════════════
//public class Notification : BaseEntity
//{
//    public Guid UserId { get; set; }
//    public string? Title { get; set; }
//    public string? Body { get; set; }
//    public string? Type { get; set; }                 // match | message | like | gift | call | superchat | system
//    public bool IsRead { get; set; } = false;
//    public string? ReferenceId { get; set; }
//    public User? User { get; set; }
//}

//// ════════════════════════════════════════════════════════════
//// SAFETY & ADMIN
//// ════════════════════════════════════════════════════════════
//public class Report : BaseEntity
//{
//    public Guid ReporterId { get; set; }
//    public Guid ReportedUserId { get; set; }
//    public string? Reason { get; set; }
//    public string? Description { get; set; }
//    public string Status { get; set; } = "pending";   // pending | reviewed | dismissed
//    public string? AdminNote { get; set; }
//    public User? Reporter { get; set; }
//    public User? ReportedUser { get; set; }
//}

//public class Block : BaseEntity
//{
//    public Guid BlockerId { get; set; }
//    public Guid BlockedUserId { get; set; }
//    public User? Blocker { get; set; }
//    public User? BlockedUser { get; set; }
//}

//public class PrivacyAgreement : BaseEntity
//{
//    public Guid UserId { get; set; }
//    public Guid MatchId { get; set; }
//    public bool Accepted { get; set; } = true;
//    public User? User { get; set; }
//}

//// ════════════════════════════════════════════════════════════
//// GIFTS
//// ════════════════════════════════════════════════════════════
//public class Gift : BaseEntity
//{
//    public string Name { get; set; } = string.Empty;
//    public string? Icon { get; set; }
//    public string? Emoji { get; set; }
//    public int CoinCost { get; set; }
//    public bool IsActive { get; set; } = true;
//    public string? Category { get; set; }             // standard | romantic | fun | animated | luxury | vip
//    public string? ImageUrl { get; set; }
//    public bool IsAnimated { get; set; } = false;
//}
////using Mingley.Domain.Common;

////namespace Mingley.Domain.Entities;

////// ════════════════════════════════════════════════════════════
////// USER & PROFILE
////// ════════════════════════════════════════════════════════════
////public class User : BaseEntity
////{
////    public string? FullName { get; set; }
////    public string? Email { get; set; }
////    public string? Phone { get; set; }
////    public string? PasswordHash { get; set; }
////    public string? Gender { get; set; }
////    public DateTime? DateOfBirth { get; set; }
////    public string? Bio { get; set; }
////    public string? Avatar { get; set; }
////    public string Role { get; set; } = "user";
////    public bool IsVerified { get; set; } = false;
////    public bool IsActive { get; set; } = true;
////    public bool IsPremium { get; set; } = false;
////    public int CoinBalance { get; set; } = 0;
////    public double TotalEarned { get; set; } = 0;
////    public bool TwoFactorEnabled { get; set; } = false;
////    public string? TwoFactorSecret { get; set; }
////    public DateTime? LastActiveAt { get; set; }
////    public bool IsOnline { get; set; } = false;
////    public string? OtpCode { get; set; }
////    public DateTime? OtpExpiry { get; set; }
////    public string? OtpPurpose { get; set; }
////    public string? FcmToken { get; set; }
////    public bool ProfileComplete { get; set; } = false;

////    // Admin management
////    public bool IsCreatedByAdmin { get; set; } = false;
////    public bool IsSuspended { get; set; } = false;
////    public DateTime? SuspendedAt { get; set; }
////    public string? SuspendReason { get; set; }
////    public string? SuspendedBy { get; set; }

////    // Navigation
////    public UserLocation? Location { get; set; }
////    public UserPreference? Preference { get; set; }
////    public UserSubscription? Subscription { get; set; }
////    public ICollection<UserImage> Images { get; set; } = new List<UserImage>();
////    public ICollection<UserInterest> Interests { get; set; } = new List<UserInterest>();
////    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
////}

////public class UserLocation : BaseEntity
////{
////    public Guid UserId { get; set; }
////    public double? Lat { get; set; }
////    public double? Lng { get; set; }
////    public string? City { get; set; }
////    public string? Country { get; set; }
////    public User? User { get; set; }
////}

////public class UserPreference : BaseEntity
////{
////    public Guid UserId { get; set; }
////    public string InterestedIn { get; set; } = "both";
////    public int MinAge { get; set; } = 18;
////    public int MaxAge { get; set; } = 40;
////    public int MaxDistance { get; set; } = 50;
////    public string RelationshipType { get; set; } = "both";
////    public bool NearbyOnly { get; set; } = false;
////    public bool OnlineOnly { get; set; } = false;
////    public bool VerifiedOnly { get; set; } = false;
////    public string? Location { get; set; }
////    public User? User { get; set; }
////}

////public class UserImage : BaseEntity
////{
////    public Guid UserId { get; set; }
////    public string Url { get; set; } = string.Empty;
////    public int SortOrder { get; set; } = 0;
////    public bool IsPrimary { get; set; } = false;
////    public User? User { get; set; }
////}

////public class UserInterest
////{
////    public Guid UserId { get; set; }
////    public Guid InterestId { get; set; }
////    public User? User { get; set; }
////    public Interest? Interest { get; set; }
////}

////public class Interest : BaseEntity
////{
////    public string Name { get; set; } = string.Empty;
////    public string? Icon { get; set; }
////    public string? Emoji { get; set; }
////    public ICollection<UserInterest> UserInterests { get; set; } = new List<UserInterest>();
////}

////// ════════════════════════════════════════════════════════════
////// AUTH — REFRESH TOKENS (DB-backed, survives restarts)
////// ════════════════════════════════════════════════════════════
////public class RefreshToken
////{
////    public int Id { get; set; }          // auto-increment PK
////    public string Token { get; set; } = string.Empty;
////    public Guid UserId { get; set; }
////    public DateTime ExpiresAt { get; set; }
////    public bool IsRevoked { get; set; } = false;
////    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

////    // Navigation
////    public User? User { get; set; }
////}

////// ════════════════════════════════════════════════════════════
////// MATCHING & SWIPING
////// ════════════════════════════════════════════════════════════
////public class Swipe : BaseEntity
////{
////    public Guid SwiperId { get; set; }
////    public Guid TargetId { get; set; }
////    public string Action { get; set; } = "like";     // like | dislike | superlike
////    public User? Swiper { get; set; }
////    public User? Target { get; set; }
////}

////public class Match : BaseEntity
////{
////    public Guid User1Id { get; set; }
////    public Guid User2Id { get; set; }
////    public bool IsActive { get; set; } = true;
////    public User? User1 { get; set; }
////    public User? User2 { get; set; }
////    public Chat? Chat { get; set; }
////}

////// ════════════════════════════════════════════════════════════
////// CHAT & MESSAGES
////// ════════════════════════════════════════════════════════════
////public class Chat : BaseEntity
////{
////    public Guid MatchId { get; set; }
////    public Match? Match { get; set; }
////    public ICollection<Message> Messages { get; set; } = new List<Message>();
////}

////public class Message : BaseEntity
////{
////    public Guid ChatId { get; set; }
////    public Guid SenderId { get; set; }
////    public string? Text { get; set; }
////    public string Type { get; set; } = "text";       // text | image | gift | coins | system
////    public string? ImageUrl { get; set; }
////    public string? GiftName { get; set; }
////    public int? GiftCost { get; set; }
////    public int? CoinAmount { get; set; }
////    public DateTime? ReadAt { get; set; }
////    public int CoinsDeducted { get; set; } = 0;
////    public Guid? ReplyToMessageId { get; set; }
////    public Chat? Chat { get; set; }
////    public User? Sender { get; set; }
////    public Message? ReplyToMessage { get; set; }
////}

////// ════════════════════════════════════════════════════════════
////// CALLS
////// ════════════════════════════════════════════════════════════
////public class CallSession : BaseEntity
////{
////    public Guid CallerId { get; set; }
////    public Guid ReceiverId { get; set; }
////    public Guid MatchId { get; set; }
////    public string CallType { get; set; } = "audio";  // audio | video
////    public string Status { get; set; } = "ringing";  // ringing | active | ended | declined | timeout | cancelled
////    public DateTime? AnsweredAt { get; set; }
////    public DateTime? EndedAt { get; set; }
////    public int? DurationSeconds { get; set; }
////    public int CoinsDeducted { get; set; } = 0;
////    public string? EndReason { get; set; }
////    public User? Caller { get; set; }
////    public User? Receiver { get; set; }
////    public Match? Match { get; set; }
////}

////// ════════════════════════════════════════════════════════════
////// SUPERCHAT
////// ════════════════════════════════════════════════════════════
////public class SuperChat : BaseEntity
////{
////    public Guid FromUserId { get; set; }
////    public Guid ToUserId { get; set; }
////    public string Message { get; set; } = string.Empty;
////    public int CoinAmount { get; set; } = 500;
////    public double GirlCommission { get; set; }
////    public double CompanyRevenue { get; set; }
////    public bool IsResponded { get; set; } = false;
////    public DateTime? RespondedAt { get; set; }
////    public Guid? MatchCreatedId { get; set; }
////    public User? FromUser { get; set; }
////    public User? ToUser { get; set; }
////    public Match? MatchCreated { get; set; }
////}

////// ════════════════════════════════════════════════════════════
////// COINS & WALLET
////// ════════════════════════════════════════════════════════════
////public class CoinTransaction : BaseEntity
////{
////    public Guid UserId { get; set; }
////    public int Coins { get; set; }
////    public string Direction { get; set; } = "credit"; // credit | debit
////    public string? Description { get; set; }
////    public string? TransactionType { get; set; }      // message | gift | deposit | withdrawal | call | superlike | superchat | admin | verification
////    public string? ReferenceId { get; set; }
////    public User? User { get; set; }
////}

////public class DepositRequest : BaseEntity
////{
////    public Guid UserId { get; set; }
////    public string? UtrId { get; set; }
////    public string? ScreenshotUrl { get; set; }
////    public int? RequestedCoins { get; set; }
////    public string Status { get; set; } = "pending";   // pending | approved | rejected
////    public string? AdminNote { get; set; }
////    public User? User { get; set; }
////}

////public class WithdrawalRequest : BaseEntity
////{
////    public Guid UserId { get; set; }
////    public int Coins { get; set; }
////    public string? BankOrUpi { get; set; }
////    public string Status { get; set; } = "pending";   // pending | approved | rejected
////    public string? AdminNote { get; set; }
////    public User? User { get; set; }
////}

////// ════════════════════════════════════════════════════════════
////// SUBSCRIPTIONS
////// ════════════════════════════════════════════════════════════
////public class SubscriptionPlan : BaseEntity
////{
////    public string Name { get; set; } = string.Empty;
////    public decimal Price { get; set; }
////    public int DurationDays { get; set; }
////    public string? Features { get; set; }
////    public bool IsPopular { get; set; } = false;
////    public bool IsActive { get; set; } = true;
////    public int SuperLikesPerDay { get; set; } = 1;
////    public int BoostsPerMonth { get; set; } = 0;
////    public bool UnlimitedLikes { get; set; } = false;
////    public bool CanSeeWhoLiked { get; set; } = false;
////    public bool VideoCallEnabled { get; set; } = false;
////}

////public class UserSubscription : BaseEntity
////{
////    public Guid UserId { get; set; }
////    public Guid PlanId { get; set; }
////    public DateTime StartDate { get; set; } = DateTime.UtcNow;
////    public DateTime EndDate { get; set; }
////    public bool IsActive { get; set; } = true;
////    public bool AutoRenew { get; set; } = true;
////    public string? CancelReason { get; set; }
////    public Guid? GrantedBy { get; set; }
////    public User? User { get; set; }
////    public SubscriptionPlan? Plan { get; set; }
////}

////// ════════════════════════════════════════════════════════════
////// NOTIFICATIONS
////// ════════════════════════════════════════════════════════════
////public class Notification : BaseEntity
////{
////    public Guid UserId { get; set; }
////    public string? Title { get; set; }
////    public string? Body { get; set; }
////    public string? Type { get; set; }                 // match | message | like | gift | call | superchat | system
////    public bool IsRead { get; set; } = false;
////    public string? ReferenceId { get; set; }
////    public User? User { get; set; }
////}

////// ════════════════════════════════════════════════════════════
////// SAFETY & ADMIN
////// ════════════════════════════════════════════════════════════
////public class Report : BaseEntity
////{
////    public Guid ReporterId { get; set; }
////    public Guid ReportedUserId { get; set; }
////    public string? Reason { get; set; }
////    public string? Description { get; set; }
////    public string Status { get; set; } = "pending";   // pending | reviewed | dismissed
////    public string? AdminNote { get; set; }
////    public User? Reporter { get; set; }
////    public User? ReportedUser { get; set; }
////}

////public class Block : BaseEntity
////{
////    public Guid BlockerId { get; set; }
////    public Guid BlockedUserId { get; set; }
////    public User? Blocker { get; set; }
////    public User? BlockedUser { get; set; }
////}

////public class PrivacyAgreement : BaseEntity
////{
////    public Guid UserId { get; set; }
////    public Guid MatchId { get; set; }
////    public bool Accepted { get; set; } = true;
////    public User? User { get; set; }
////}

////// ════════════════════════════════════════════════════════════
////// GIFTS
////// ════════════════════════════════════════════════════════════
////public class Gift : BaseEntity
////{
////    public string Name { get; set; } = string.Empty;
////    public string? Icon { get; set; }
////    public string? Emoji { get; set; }
////    public int CoinCost { get; set; }
////    public bool IsActive { get; set; } = true;
////    public string? Category { get; set; }             // standard | romantic | fun | animated | luxury | vip
////    public string? ImageUrl { get; set; }
////    public bool IsAnimated { get; set; } = false;
////}

//////using Mingley.Domain.Common;

//////namespace Mingley.Domain.Entities;

//////// ════════════════════════════════════════════════════════════
//////// USER & PROFILE
//////// ════════════════════════════════════════════════════════════
////////public class User : BaseEntity
////////{
////////    public string? FullName { get; set; }
////////    public string? Email { get; set; }
////////    public string? Phone { get; set; }
////////    public string? PasswordHash { get; set; }
////////    public string? Gender { get; set; }               // male | female | other
////////    public DateTime? DateOfBirth { get; set; }
////////    public string? Bio { get; set; }
////////    public string? Avatar { get; set; }
////////    public string Role { get; set; } = "user";        // user | admin | moderator
////////    public bool IsVerified { get; set; } = false;
////////    public bool IsActive { get; set; } = true;
////////    public bool IsPremium { get; set; } = false;
////////    public int CoinBalance { get; set; } = 0;
////////    public double TotalEarned { get; set; } = 0;       // INR earned via SuperChat commissions
////////    public bool TwoFactorEnabled { get; set; } = false;
////////    public string? TwoFactorSecret { get; set; }
////////    public DateTime? LastActiveAt { get; set; }
////////    public bool IsOnline { get; set; } = false;
////////    public string? OtpCode { get; set; }
////////    public DateTime? OtpExpiry { get; set; }
////////    public string? OtpPurpose { get; set; }            // registration | forgot_password
////////    public string? FcmToken { get; set; }              // push notification token
////////    public bool ProfileComplete { get; set; } = false;

////////    // Navigation
////////    public UserLocation? Location { get; set; }
////////    public UserPreference? Preference { get; set; }
////////    public UserSubscription? Subscription { get; set; }
////////    public ICollection<UserImage> Images { get; set; } = new List<UserImage>();
////////    public ICollection<UserInterest> Interests { get; set; } = new List<UserInterest>();
////////}


//////public class User : BaseEntity
//////{
//////    public string? FullName { get; set; }
//////    public string? Email { get; set; }
//////    public string? Phone { get; set; }
//////    public string? PasswordHash { get; set; }
//////    public string? Gender { get; set; }
//////    public DateTime? DateOfBirth { get; set; }
//////    public string? Bio { get; set; }
//////    public string? Avatar { get; set; }
//////    public string Role { get; set; } = "user";
//////    public bool IsVerified { get; set; } = false;
//////    public bool IsActive { get; set; } = true;
//////    public bool IsPremium { get; set; } = false;
//////    public int CoinBalance { get; set; } = 0;
//////    public double TotalEarned { get; set; } = 0;
//////    public bool TwoFactorEnabled { get; set; } = false;
//////    public string? TwoFactorSecret { get; set; }
//////    public DateTime? LastActiveAt { get; set; }
//////    public bool IsOnline { get; set; } = false;
//////    public string? OtpCode { get; set; }
//////    public DateTime? OtpExpiry { get; set; }
//////    public string? OtpPurpose { get; set; }
//////    public string? FcmToken { get; set; }
//////    public bool ProfileComplete { get; set; } = false;

//////    // Admin management — NEW
//////    public bool IsCreatedByAdmin { get; set; } = false;
//////    public bool IsSuspended { get; set; } = false;
//////    public DateTime? SuspendedAt { get; set; }
//////    public string? SuspendReason { get; set; }
//////    public string? SuspendedBy { get; set; }

//////    // Navigation
//////    public UserLocation? Location { get; set; }
//////    public UserPreference? Preference { get; set; }
//////    public UserSubscription? Subscription { get; set; }
//////    public ICollection<UserImage> Images { get; set; } = new List<UserImage>();
//////    public ICollection<UserInterest> Interests { get; set; } = new List<UserInterest>();
//////}
//////public class UserLocation : BaseEntity
//////{
//////    public Guid UserId { get; set; }
//////    public double? Lat { get; set; }
//////    public double? Lng { get; set; }
//////    public string? City { get; set; }
//////    public string? Country { get; set; }
//////    public User? User { get; set; }
//////}

//////public class UserPreference : BaseEntity
//////{
//////    public Guid UserId { get; set; }
//////    public string InterestedIn { get; set; } = "both"; // girls | boys | both
//////    public int MinAge { get; set; } = 18;
//////    public int MaxAge { get; set; } = 40;
//////    public int MaxDistance { get; set; } = 50;         // km
//////    public string RelationshipType { get; set; } = "both"; // casual | serious | both
//////    public bool NearbyOnly { get; set; } = false;
//////    public bool OnlineOnly { get; set; } = false;
//////    public bool VerifiedOnly { get; set; } = false;
//////    public string? Location { get; set; }
//////    public User? User { get; set; }
//////}

//////public class UserImage : BaseEntity
//////{
//////    public Guid UserId { get; set; }
//////    public string Url { get; set; } = string.Empty;
//////    public int SortOrder { get; set; } = 0;
//////    public bool IsPrimary { get; set; } = false;
//////    public User? User { get; set; }
//////}

//////public class UserInterest
//////{
//////    public Guid UserId { get; set; }
//////    public Guid InterestId { get; set; }
//////    public User? User { get; set; }
//////    public Interest? Interest { get; set; }
//////}

//////public class Interest : BaseEntity
//////{
//////    public string Name { get; set; } = string.Empty;
//////    public string? Icon { get; set; }
//////    public string? Emoji { get; set; }
//////    public ICollection<UserInterest> UserInterests { get; set; } = new List<UserInterest>();
//////}

//////// ════════════════════════════════════════════════════════════
//////// MATCHING & SWIPING
//////// ════════════════════════════════════════════════════════════
//////public class Swipe : BaseEntity
//////{
//////    public Guid SwiperId { get; set; }
//////    public Guid TargetId { get; set; }
//////    public string Action { get; set; } = "like";       // like | dislike | superlike
//////    public User? Swiper { get; set; }
//////    public User? Target { get; set; }
//////}

//////public class Match : BaseEntity
//////{
//////    public Guid User1Id { get; set; }
//////    public Guid User2Id { get; set; }
//////    public bool IsActive { get; set; } = true;
//////    public User? User1 { get; set; }
//////    public User? User2 { get; set; }
//////    public Chat? Chat { get; set; }
//////}

//////// ════════════════════════════════════════════════════════════
//////// CHAT & MESSAGES
//////// ════════════════════════════════════════════════════════════
//////public class Chat : BaseEntity
//////{
//////    public Guid MatchId { get; set; }
//////    public Match? Match { get; set; }
//////    public ICollection<Message> Messages { get; set; } = new List<Message>();
//////}

//////public class Message : BaseEntity
//////{
//////    public Guid ChatId { get; set; }
//////    public Guid SenderId { get; set; }
//////    public string? Text { get; set; }
//////    public string Type { get; set; } = "text";         // text | image | gift | coins | system
//////    public string? ImageUrl { get; set; }
//////    public string? GiftName { get; set; }
//////    public int? GiftCost { get; set; }
//////    public int? CoinAmount { get; set; }
//////    public DateTime? ReadAt { get; set; }
//////    public int CoinsDeducted { get; set; } = 0;
//////    public Guid? ReplyToMessageId { get; set; }        // for reply feature
//////    public Chat? Chat { get; set; }
//////    public User? Sender { get; set; }
//////    public Message? ReplyToMessage { get; set; }
//////}

//////// ════════════════════════════════════════════════════════════
//////// CALLS
//////// ════════════════════════════════════════════════════════════
//////public class CallSession : BaseEntity
//////{
//////    public Guid CallerId { get; set; }
//////    public Guid ReceiverId { get; set; }
//////    public Guid MatchId { get; set; }
//////    public string CallType { get; set; } = "audio";    // audio | video
//////    public string Status { get; set; } = "ringing";    // ringing | active | ended | declined | missed
//////    public DateTime? AnsweredAt { get; set; }
//////    public DateTime? EndedAt { get; set; }
//////    public int? DurationSeconds { get; set; }
//////    public int CoinsDeducted { get; set; } = 0;
//////    public string? EndReason { get; set; }
//////    public User? Caller { get; set; }
//////    public User? Receiver { get; set; }
//////    public Match? Match { get; set; }
//////}

//////// ════════════════════════════════════════════════════════════
//////// SUPERCHAT
//////// ════════════════════════════════════════════════════════════
//////public class SuperChat : BaseEntity
//////{
//////    public Guid FromUserId { get; set; }
//////    public Guid ToUserId { get; set; }
//////    public string Message { get; set; } = string.Empty;
//////    public int CoinAmount { get; set; } = 500;
//////    public double GirlCommission { get; set; }         // 50% of coin value in INR
//////    public double CompanyRevenue { get; set; }         // 50% of coin value in INR
//////    public bool IsResponded { get; set; } = false;
//////    public DateTime? RespondedAt { get; set; }
//////    public Guid? MatchCreatedId { get; set; }          // match created when girl responds
//////    public User? FromUser { get; set; }
//////    public User? ToUser { get; set; }
//////    public Match? MatchCreated { get; set; }
//////}

//////// ════════════════════════════════════════════════════════════
//////// COINS & WALLET
//////// ════════════════════════════════════════════════════════════
//////public class CoinTransaction : BaseEntity
//////{
//////    public Guid UserId { get; set; }
//////    public int Coins { get; set; }
//////    public string Direction { get; set; } = "credit";  // credit | debit
//////    public string? Description { get; set; }
//////    public string? TransactionType { get; set; }       // message | gift | deposit | withdrawal | call | superlike | superchat | admin | verification
//////    public string? ReferenceId { get; set; }
//////    public User? User { get; set; }
//////}

//////public class DepositRequest : BaseEntity
//////{
//////    public Guid UserId { get; set; }
//////    public string? UtrId { get; set; }
//////    public string? ScreenshotUrl { get; set; }
//////    public int? RequestedCoins { get; set; }
//////    public string Status { get; set; } = "pending";    // pending | approved | rejected
//////    public string? AdminNote { get; set; }
//////    public User? User { get; set; }
//////}

//////public class WithdrawalRequest : BaseEntity
//////{
//////    public Guid UserId { get; set; }
//////    public int Coins { get; set; }
//////    public string? BankOrUpi { get; set; }
//////    public string Status { get; set; } = "pending";    // pending | approved | rejected
//////    public string? AdminNote { get; set; }
//////    public User? User { get; set; }
//////}

//////// ════════════════════════════════════════════════════════════
//////// SUBSCRIPTIONS
//////// ════════════════════════════════════════════════════════════
//////public class SubscriptionPlan : BaseEntity
//////{
//////    public string Name { get; set; } = string.Empty;
//////    public decimal Price { get; set; }
//////    public int DurationDays { get; set; }
//////    public string? Features { get; set; }              // JSON array string
//////    public bool IsPopular { get; set; } = false;
//////    public bool IsActive { get; set; } = true;
//////    public int SuperLikesPerDay { get; set; } = 1;
//////    public int BoostsPerMonth { get; set; } = 0;
//////    public bool UnlimitedLikes { get; set; } = false;
//////    public bool CanSeeWhoLiked { get; set; } = false;
//////    public bool VideoCallEnabled { get; set; } = false;
//////}

//////public class UserSubscription : BaseEntity
//////{
//////    public Guid UserId { get; set; }
//////    public Guid PlanId { get; set; }
//////    public DateTime StartDate { get; set; } = DateTime.UtcNow;
//////    public DateTime EndDate { get; set; }
//////    public bool IsActive { get; set; } = true;
//////    public bool AutoRenew { get; set; } = true;
//////    public string? CancelReason { get; set; }
//////    public Guid? GrantedBy { get; set; }
//////    public User? User { get; set; }
//////    public SubscriptionPlan? Plan { get; set; }
//////}

//////// ════════════════════════════════════════════════════════════
//////// NOTIFICATIONS
//////// ════════════════════════════════════════════════════════════
//////public class Notification : BaseEntity
//////{
//////    public Guid UserId { get; set; }
//////    public string? Title { get; set; }
//////    public string? Body { get; set; }
//////    public string? Type { get; set; }                  // match | message | like | gift | call | superchat | system
//////    public bool IsRead { get; set; } = false;
//////    public string? ReferenceId { get; set; }
//////    public User? User { get; set; }
//////}

//////// ════════════════════════════════════════════════════════════
//////// SAFETY & ADMIN
//////// ════════════════════════════════════════════════════════════
//////public class Report : BaseEntity
//////{
//////    public Guid ReporterId { get; set; }
//////    public Guid ReportedUserId { get; set; }
//////    public string? Reason { get; set; }
//////    public string? Description { get; set; }
//////    public string Status { get; set; } = "pending";    // pending | reviewed | dismissed
//////    public string? AdminNote { get; set; }
//////    public User? Reporter { get; set; }
//////    public User? ReportedUser { get; set; }
//////}

//////public class Block : BaseEntity
//////{
//////    public Guid BlockerId { get; set; }
//////    public Guid BlockedUserId { get; set; }
//////    public User? Blocker { get; set; }
//////    public User? BlockedUser { get; set; }
//////}

//////public class PrivacyAgreement : BaseEntity
//////{
//////    public Guid UserId { get; set; }
//////    public Guid MatchId { get; set; }
//////    public bool Accepted { get; set; } = true;
//////    public User? User { get; set; }
//////}


////////public class Gift : BaseEntity
////////{
////////    public string Name { get; set; } = string.Empty;
////////    public string? Icon { get; set; }
////////    public string? Emoji { get; set; }
////////    public int CoinCost { get; set; }
////////    public bool IsActive { get; set; } = true;
////////}
