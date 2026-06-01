namespace Mingley.Application.DTOs.Users;

public class UserProfileDto
{
    public string? Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public int? Age { get; set; }
    public string? Bio { get; set; }
    public string? Profession { get; set; }
    public string? Avatar { get; set; }
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
    public bool IsPremium { get; set; }
    public bool IsOnline { get; set; }
    public bool ProfileComplete { get; set; }
    public int CoinBalance { get; set; }
    public double TotalEarned { get; set; }
    public string? Role { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTime? LastActiveAt { get; set; }
    public LocationDto? Location { get; set; }
    public PreferenceDto? Preference { get; set; }
    public List<string> Interests { get; set; } = new();
    public List<ImageDto> Images { get; set; } = new();
    public SubscriptionInfoDto? Subscription { get; set; }
}

public class LocationDto
{
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
}

public class PreferenceDto
{
    public string InterestedIn { get; set; } = "both";
    public int MinAge { get; set; } = 18;
    public int MaxAge { get; set; } = 40;
    public int MaxDistance { get; set; } = 50;
    public string RelationshipType { get; set; } = "both";
    public bool NearbyOnly { get; set; }
    public bool OnlineOnly { get; set; }
    public bool VerifiedOnly { get; set; }
    public string? Location { get; set; }
}

public class ImageDto
{
    public string? Id { get; set; }
    public string? Url { get; set; }
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
}

public class SubscriptionInfoDto
{
    public string? Id { get; set; }
    public string? PlanName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool AutoRenew { get; set; }
    public int DaysRemaining { get; set; }
}

// Update requests (all nullable for partial updates)
public class UpdateProfileRequest
{
    public string? FullName { get; set; }
    public string? Bio { get; set; }
    public string? Profession { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Avatar { get; set; }
}

public class UpdateInterestsRequest
{
    public List<string> Interests { get; set; } = new();
}

public class UpdateLocationRequest
{
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
}

public class SetTravelModeRequest
{
    public bool Enabled { get; set; }
    public string? City { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }
}

public class UpdatePreferencesRequest
{
    public string? InterestedIn { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public int? MaxDistance { get; set; }
    public string? RelationshipType { get; set; }
    public bool? NearbyOnly { get; set; }
    public bool? OnlineOnly { get; set; }
    public bool? VerifiedOnly { get; set; }
    public string? Location { get; set; }
}

public class AddImageRequest
{
    public string Url { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = false;
}

public class ReorderImagesRequest
{
    public List<ImageOrderItem> Order { get; set; } = new();
}
public class ImageOrderItem
{
    public string ImageId { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class DeleteAccountRequest
{
    public string Password { get; set; } = string.Empty;
    public string? Reason { get; set; }
}


// NEW: Phone contact found on Mingley
public class ContactOnAppDto
{
    public string? UserId { get; set; }
    public string? FullName { get; set; }
    public string? Avatar { get; set; }
    public string? Phone { get; set; }
    public bool IsOnline { get; set; }
    public bool IsVerified { get; set; }
}

public class ContactsRequest
{
    public List<string> PhoneNumbers { get; set; } = new();
}