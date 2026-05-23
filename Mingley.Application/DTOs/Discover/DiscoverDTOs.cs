// FILE: Mingley.Application/DTOs/Discover/DiscoverDTOs.cs
// Changes: Added TrendingUserDto, TrendingSection, updated DiscoverFilters with city/VIP

namespace Mingley.Application.DTOs.Discover;

public class DiscoverUserDto
{
    public string? Id { get; set; }
    public string? FullName { get; set; }
    public int? Age { get; set; }
    public string? Bio { get; set; }
    public string? Profession { get; set; }
    public string? Gender { get; set; }
    public string? Avatar { get; set; }
    public bool IsVerified { get; set; }
    public bool IsPremium { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastActiveAt { get; set; }
    public double? Distance { get; set; }
    public string? City { get; set; }
    public int MatchScore { get; set; }
    public List<string> Interests { get; set; } = new();
    public List<string> Images { get; set; } = new();
}

public class SwipeRequest
{
    public string TargetId { get; set; } = string.Empty;
    public string Action { get; set; } = "like";
}

public class SwipeResponse
{
    public bool IsMatch { get; set; }
    public MatchDto? Match { get; set; }
    public int? RemainingSuperlikes { get; set; }
    public int? CoinsDeducted { get; set; }
}

public class MatchDto
{
    public string? MatchId { get; set; }
    public string? ChatId { get; set; }
    public MatchedUserDto? User { get; set; }
    public DateTime MatchedAt { get; set; }
}

public class MatchedUserDto
{
    public string? Id { get; set; }
    public string? FullName { get; set; }
    public string? Avatar { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastActiveAt { get; set; }
}

public class MatchFullProfileDto
{
    public string? Id { get; set; }
    public string? FullName { get; set; }
    public int? Age { get; set; }
    public string? Bio { get; set; }
    public string? Profession { get; set; }
    public string? Gender { get; set; }
    public string? Avatar { get; set; }
    public bool IsVerified { get; set; }
    public bool IsPremium { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastActiveAt { get; set; }
    public string? City { get; set; }
    public List<string> Interests { get; set; } = new();
    public List<string> Images { get; set; } = new();
}

public class MatchListItemDto
{
    public string? MatchId { get; set; }
    public string? ChatId { get; set; }
    public MatchedUserDto? User { get; set; }
    public MatchedUserDto? MatchedUser { get; set; }
    public DateTime MatchedAt { get; set; }
    public int UnreadCount { get; set; }
    public LastMessageDto? LastMessage { get; set; }
    public MatchFullProfileDto? FullProfile { get; set; }
}

public class LastMessageDto
{
    public string? Text { get; set; }
    public string? Type { get; set; }
    public DateTime SentAt { get; set; }
}

public class PaginationDto
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
    public bool HasNext { get; set; }
}

// TASK 6: Updated filters with VIP location search
public class DiscoverFilters
{
    public string? Gender { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public int? MaxDistance { get; set; }    // km — VIP only if > 100
    public bool? OnlineOnly { get; set; }
    public string? City { get; set; }        // VIP only — search by city
    public bool? Global { get; set; }        // VIP only — search globally
}

// TASK 7: Trending section DTOs
public class TrendingUserDto
{
    public string? Id { get; set; }
    public string? FullName { get; set; }
    public int? Age { get; set; }
    public string? Bio { get; set; }
    public string? Avatar { get; set; }
    public bool IsVerified { get; set; }
    public bool IsPremium { get; set; }
    public bool IsOnline { get; set; }
    public string? City { get; set; }
    public int TotalMatches { get; set; }
    public int TotalGiftsReceived { get; set; }
    public int TotalSuperChats { get; set; }
    public double TotalEarned { get; set; }
    public int TrendingScore { get; set; }
    public List<string> Images { get; set; } = new();
    public List<string> Interests { get; set; } = new();
}

public class TrendingSection
{
    public string Section { get; set; } = string.Empty;     // trending | most_gifted | most_popular | recommended
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public List<TrendingUserDto> Users { get; set; } = new();
}
//namespace Mingley.Application.DTOs.Discover;

//public class DiscoverUserDto
//{
//    public string? Id { get; set; }
//    public string? FullName { get; set; }
//    public int? Age { get; set; }
//    public string? Bio { get; set; }
//    public string? Gender { get; set; }
//    public string? Avatar { get; set; }
//    public bool IsVerified { get; set; }
//    public bool IsPremium { get; set; }
//    public bool IsOnline { get; set; }
//    public DateTime? LastActiveAt { get; set; }
//    public double? Distance { get; set; }
//    public string? City { get; set; }
//    public int MatchScore { get; set; }
//    public List<string> Interests { get; set; } = new();
//    public List<string> Images { get; set; } = new();
//}

//public class SwipeRequest
//{
//    public string TargetId { get; set; } = string.Empty;
//    public string Action { get; set; } = "like";
//}

//public class SwipeResponse
//{
//    public bool IsMatch { get; set; }
//    public MatchDto? Match { get; set; }
//    public int? RemainingSuperlikes { get; set; }
//    public int? CoinsDeducted { get; set; }
//}

//public class MatchDto
//{
//    public string? MatchId { get; set; }
//    public string? ChatId { get; set; }
//    public MatchedUserDto? User { get; set; }
//    public DateTime MatchedAt { get; set; }
//}

//public class MatchedUserDto
//{
//    public string? Id { get; set; }
//    public string? FullName { get; set; }
//    public string? Avatar { get; set; }
//    public bool IsOnline { get; set; }
//    public DateTime? LastActiveAt { get; set; }
//}

//// Full profile embedded in each match item — same detail as Discover screen
//public class MatchFullProfileDto
//{
//    public string? Id { get; set; }
//    public string? FullName { get; set; }
//    public int? Age { get; set; }
//    public string? Bio { get; set; }
//    public string? Gender { get; set; }
//    public string? Avatar { get; set; }
//    public bool IsVerified { get; set; }
//    public bool IsPremium { get; set; }
//    public bool IsOnline { get; set; }
//    public DateTime? LastActiveAt { get; set; }
//    public string? City { get; set; }
//    public List<string> Interests { get; set; } = new();
//    public List<string> Images { get; set; } = new();
//}

//public class MatchListItemDto
//{
//    public string? MatchId { get; set; }
//    public string? ChatId { get; set; }
//    public MatchedUserDto? User { get; set; }
//    public MatchedUserDto? MatchedUser { get; set; }
//    public DateTime MatchedAt { get; set; }
//    public int UnreadCount { get; set; }
//    public LastMessageDto? LastMessage { get; set; }
//    public MatchFullProfileDto? FullProfile { get; set; }
//}

//public class LastMessageDto
//{
//    public string? Text { get; set; }
//    public string? Type { get; set; }
//    public DateTime SentAt { get; set; }
//}

//public class PaginationDto
//{
//    public int Page { get; set; }
//    public int Limit { get; set; }
//    public int Total { get; set; }
//    public bool HasNext { get; set; }
//}

//public class DiscoverFilters
//{
//    public string? Gender { get; set; }
//    public int? MinAge { get; set; }
//    public int? MaxAge { get; set; }
//    public int? MaxDistance { get; set; }
//    public bool? OnlineOnly { get; set; }
//}


////namespace Mingley.Application.DTOs.Discover;

////public class DiscoverUserDto
////{
////    public string? Id { get; set; }
////    public string? FullName { get; set; }
////    public int? Age { get; set; }
////    public string? Bio { get; set; }
////    public string? Gender { get; set; }
////    public string? Avatar { get; set; }
////    public bool IsVerified { get; set; }
////    public bool IsPremium { get; set; }
////    public bool IsOnline { get; set; }
////    public DateTime? LastActiveAt { get; set; }
////    public double? Distance { get; set; }
////    public string? City { get; set; }
////    public int MatchScore { get; set; }
////    public List<string> Interests { get; set; } = new();
////    public List<string> Images { get; set; } = new();
////}

////public class SwipeRequest
////{
////    public string TargetId { get; set; } = string.Empty;
////    public string Action { get; set; } = "like"; // like | dislike | superlike
////}

////public class SwipeResponse
////{
////    public bool IsMatch { get; set; }
////    public MatchDto? Match { get; set; }
////    public int? RemainingSuperlikes { get; set; }
////    public int? CoinsDeducted { get; set; }
////}

////public class MatchDto
////{
////    public string? MatchId { get; set; }
////    public string? ChatId { get; set; }
////    public MatchedUserDto? User { get; set; }
////    public DateTime MatchedAt { get; set; }
////}

////public class MatchedUserDto
////{
////    public string? Id { get; set; }
////    public string? FullName { get; set; }
////    public string? Avatar { get; set; }
////    public bool IsOnline { get; set; }
////    public DateTime? LastActiveAt { get; set; }
////}

////public class MatchListItemDto
////{
////    public string? MatchId { get; set; }
////    public string? ChatId { get; set; }
////    public MatchedUserDto? User { get; set; }
////    public MatchedUserDto? MatchedUser { get; set; }
////    public DateTime MatchedAt { get; set; }
////    public int UnreadCount { get; set; }
////    public LastMessageDto? LastMessage { get; set; }
////}

////public class LastMessageDto
////{
////    public string? Text { get; set; }
////    public string? Type { get; set; }
////    public DateTime SentAt { get; set; }
////}

////public class PaginationDto
////{
////    public int Page { get; set; }
////    public int Limit { get; set; }
////    public int Total { get; set; }
////    public bool HasNext { get; set; }
////}

////public class DiscoverFilters
////{
////    public string? Gender { get; set; }
////    public int? MinAge { get; set; }
////    public int? MaxAge { get; set; }
////    public int? MaxDistance { get; set; }
////    public bool? OnlineOnly { get; set; }
////}

//// FILE: Mingley.Application/DTOs/Discover/DiscoverDTOs.cs
//// Changes: Added TrendingUserDto, TrendingSection, updated DiscoverFilters with city/VIP

//namespace Mingley.Application.DTOs.Discover;

//public class DiscoverUserDto
//{
//    public string? Id { get; set; }
//    public string? FullName { get; set; }
//    public int? Age { get; set; }
//    public string? Bio { get; set; }
//    public string? Gender { get; set; }
//    public string? Avatar { get; set; }
//    public bool IsVerified { get; set; }
//    public bool IsPremium { get; set; }
//    public bool IsOnline { get; set; }
//    public DateTime? LastActiveAt { get; set; }
//    public double? Distance { get; set; }
//    public string? City { get; set; }
//    public int MatchScore { get; set; }
//    public List<string> Interests { get; set; } = new();
//    public List<string> Images { get; set; } = new();
//}

//public class SwipeRequest
//{
//    public string TargetId { get; set; } = string.Empty;
//    public string Action { get; set; } = "like";
//}

//public class SwipeResponse
//{
//    public bool IsMatch { get; set; }
//    public MatchDto? Match { get; set; }
//    public int? RemainingSuperlikes { get; set; }
//    public int? CoinsDeducted { get; set; }
//}

//public class MatchDto
//{
//    public string? MatchId { get; set; }
//    public string? ChatId { get; set; }
//    public MatchedUserDto? User { get; set; }
//    public DateTime MatchedAt { get; set; }
//}

//public class MatchedUserDto
//{
//    public string? Id { get; set; }
//    public string? FullName { get; set; }
//    public string? Avatar { get; set; }
//    public bool IsOnline { get; set; }
//    public DateTime? LastActiveAt { get; set; }
//}

//public class MatchFullProfileDto
//{
//    public string? Id { get; set; }
//    public string? FullName { get; set; }
//    public int? Age { get; set; }
//    public string? Bio { get; set; }
//    public string? Gender { get; set; }
//    public string? Avatar { get; set; }
//    public bool IsVerified { get; set; }
//    public bool IsPremium { get; set; }
//    public bool IsOnline { get; set; }
//    public DateTime? LastActiveAt { get; set; }
//    public string? City { get; set; }
//    public List<string> Interests { get; set; } = new();
//    public List<string> Images { get; set; } = new();
//}

//public class MatchListItemDto
//{
//    public string? MatchId { get; set; }
//    public string? ChatId { get; set; }
//    public MatchedUserDto? User { get; set; }
//    public MatchedUserDto? MatchedUser { get; set; }
//    public DateTime MatchedAt { get; set; }
//    public int UnreadCount { get; set; }
//    public LastMessageDto? LastMessage { get; set; }
//    public MatchFullProfileDto? FullProfile { get; set; }
//}

//public class LastMessageDto
//{
//    public string? Text { get; set; }
//    public string? Type { get; set; }
//    public DateTime SentAt { get; set; }
//}

//public class PaginationDto
//{
//    public int Page { get; set; }
//    public int Limit { get; set; }
//    public int Total { get; set; }
//    public bool HasNext { get; set; }
//}

//// TASK 6: Updated filters with VIP location search
//public class DiscoverFilters
//{
//    public string? Gender { get; set; }
//    public int? MinAge { get; set; }
//    public int? MaxAge { get; set; }
//    public int? MaxDistance { get; set; }    // km — VIP only if > 100
//    public bool? OnlineOnly { get; set; }
//    public string? City { get; set; }        // VIP only — search by city
//    public bool? Global { get; set; }        // VIP only — search globally
//}

//// TASK 7: Trending section DTOs
//public class TrendingUserDto
//{
//    public string? Id { get; set; }
//    public string? FullName { get; set; }
//    public int? Age { get; set; }
//    public string? Bio { get; set; }
//    public string? Avatar { get; set; }
//    public bool IsVerified { get; set; }
//    public bool IsPremium { get; set; }
//    public bool IsOnline { get; set; }
//    public string? City { get; set; }
//    public int TotalMatches { get; set; }
//    public int TotalGiftsReceived { get; set; }
//    public int TotalSuperChats { get; set; }
//    public double TotalEarned { get; set; }
//    public int TrendingScore { get; set; }
//    public List<string> Images { get; set; } = new();
//    public List<string> Interests { get; set; } = new();
//}

//public class TrendingSection
//{
//    public string Section { get; set; } = string.Empty;     // trending | most_gifted | most_popular | recommended
//    public string Title { get; set; } = string.Empty;
//    public string Subtitle { get; set; } = string.Empty;
//    public List<TrendingUserDto> Users { get; set; } = new();
//}
////namespace Mingley.Application.DTOs.Discover;

////public class DiscoverUserDto
////{
////    public string? Id { get; set; }
////    public string? FullName { get; set; }
////    public int? Age { get; set; }
////    public string? Bio { get; set; }
////    public string? Gender { get; set; }
////    public string? Avatar { get; set; }
////    public bool IsVerified { get; set; }
////    public bool IsPremium { get; set; }
////    public bool IsOnline { get; set; }
////    public DateTime? LastActiveAt { get; set; }
////    public double? Distance { get; set; }
////    public string? City { get; set; }
////    public int MatchScore { get; set; }
////    public List<string> Interests { get; set; } = new();
////    public List<string> Images { get; set; } = new();
////}

////public class SwipeRequest
////{
////    public string TargetId { get; set; } = string.Empty;
////    public string Action { get; set; } = "like";
////}

////public class SwipeResponse
////{
////    public bool IsMatch { get; set; }
////    public MatchDto? Match { get; set; }
////    public int? RemainingSuperlikes { get; set; }
////    public int? CoinsDeducted { get; set; }
////}

////public class MatchDto
////{
////    public string? MatchId { get; set; }
////    public string? ChatId { get; set; }
////    public MatchedUserDto? User { get; set; }
////    public DateTime MatchedAt { get; set; }
////}

////public class MatchedUserDto
////{
////    public string? Id { get; set; }
////    public string? FullName { get; set; }
////    public string? Avatar { get; set; }
////    public bool IsOnline { get; set; }
////    public DateTime? LastActiveAt { get; set; }
////}

////// Full profile embedded in each match item — same detail as Discover screen
////public class MatchFullProfileDto
////{
////    public string? Id { get; set; }
////    public string? FullName { get; set; }
////    public int? Age { get; set; }
////    public string? Bio { get; set; }
////    public string? Gender { get; set; }
////    public string? Avatar { get; set; }
////    public bool IsVerified { get; set; }
////    public bool IsPremium { get; set; }
////    public bool IsOnline { get; set; }
////    public DateTime? LastActiveAt { get; set; }
////    public string? City { get; set; }
////    public List<string> Interests { get; set; } = new();
////    public List<string> Images { get; set; } = new();
////}

////public class MatchListItemDto
////{
////    public string? MatchId { get; set; }
////    public string? ChatId { get; set; }
////    public MatchedUserDto? User { get; set; }
////    public MatchedUserDto? MatchedUser { get; set; }
////    public DateTime MatchedAt { get; set; }
////    public int UnreadCount { get; set; }
////    public LastMessageDto? LastMessage { get; set; }
////    public MatchFullProfileDto? FullProfile { get; set; }
////}

////public class LastMessageDto
////{
////    public string? Text { get; set; }
////    public string? Type { get; set; }
////    public DateTime SentAt { get; set; }
////}

////public class PaginationDto
////{
////    public int Page { get; set; }
////    public int Limit { get; set; }
////    public int Total { get; set; }
////    public bool HasNext { get; set; }
////}

////public class DiscoverFilters
////{
////    public string? Gender { get; set; }
////    public int? MinAge { get; set; }
////    public int? MaxAge { get; set; }
////    public int? MaxDistance { get; set; }
////    public bool? OnlineOnly { get; set; }
////}


//////namespace Mingley.Application.DTOs.Discover;

//////public class DiscoverUserDto
//////{
//////    public string? Id { get; set; }
//////    public string? FullName { get; set; }
//////    public int? Age { get; set; }
//////    public string? Bio { get; set; }
//////    public string? Gender { get; set; }
//////    public string? Avatar { get; set; }
//////    public bool IsVerified { get; set; }
//////    public bool IsPremium { get; set; }
//////    public bool IsOnline { get; set; }
//////    public DateTime? LastActiveAt { get; set; }
//////    public double? Distance { get; set; }
//////    public string? City { get; set; }
//////    public int MatchScore { get; set; }
//////    public List<string> Interests { get; set; } = new();
//////    public List<string> Images { get; set; } = new();
//////}

//////public class SwipeRequest
//////{
//////    public string TargetId { get; set; } = string.Empty;
//////    public string Action { get; set; } = "like"; // like | dislike | superlike
//////}

//////public class SwipeResponse
//////{
//////    public bool IsMatch { get; set; }
//////    public MatchDto? Match { get; set; }
//////    public int? RemainingSuperlikes { get; set; }
//////    public int? CoinsDeducted { get; set; }
//////}

//////public class MatchDto
//////{
//////    public string? MatchId { get; set; }
//////    public string? ChatId { get; set; }
//////    public MatchedUserDto? User { get; set; }
//////    public DateTime MatchedAt { get; set; }
//////}

//////public class MatchedUserDto
//////{
//////    public string? Id { get; set; }
//////    public string? FullName { get; set; }
//////    public string? Avatar { get; set; }
//////    public bool IsOnline { get; set; }
//////    public DateTime? LastActiveAt { get; set; }
//////}

//////public class MatchListItemDto
//////{
//////    public string? MatchId { get; set; }
//////    public string? ChatId { get; set; }
//////    public MatchedUserDto? User { get; set; }
//////    public MatchedUserDto? MatchedUser { get; set; }
//////    public DateTime MatchedAt { get; set; }
//////    public int UnreadCount { get; set; }
//////    public LastMessageDto? LastMessage { get; set; }
//////}

//////public class LastMessageDto
//////{
//////    public string? Text { get; set; }
//////    public string? Type { get; set; }
//////    public DateTime SentAt { get; set; }
//////}

//////public class PaginationDto
//////{
//////    public int Page { get; set; }
//////    public int Limit { get; set; }
//////    public int Total { get; set; }
//////    public bool HasNext { get; set; }
//////}

//////public class DiscoverFilters
//////{
//////    public string? Gender { get; set; }
//////    public int? MinAge { get; set; }
//////    public int? MaxAge { get; set; }
//////    public int? MaxDistance { get; set; }
//////    public bool? OnlineOnly { get; set; }
//////}
