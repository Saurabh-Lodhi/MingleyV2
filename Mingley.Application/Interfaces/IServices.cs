using Mingley.Application.DTOs.Auth;
using Mingley.Application.DTOs.Chat;
using Mingley.Application.DTOs.Common;
using Mingley.Application.DTOs.Discover;
using Mingley.Application.DTOs.Subscription;
using Mingley.Application.DTOs.SuperChat;
using Mingley.Application.DTOs.Users;
using Mingley.Application.DTOs.Wallet;
using Mingley.Domain.Entities;

namespace Mingley.Application.Interfaces;

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request);
    Task ResendOtpAsync(ResendOtpRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(Guid userId);
    Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();

    // DB-backed async (use these in new code)
    Task<Guid?> ValidateRefreshTokenAsync(string refreshToken);
    Task StoreRefreshTokenAsync(Guid userId, string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);

    // Sync shims kept for backward compat (avoid in new code — deadlock risk)
    Guid? ValidateRefreshToken(string refreshToken);
    void StoreRefreshToken(Guid userId, string refreshToken);
    void RevokeRefreshToken(string refreshToken);
}

public interface IUserService
{
    Task<UserProfileDto?> GetMeAsync(Guid userId);
    Task<UserProfileDto?> GetUserAsync(Guid userId, Guid requesterId);
    Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task UpdateInterestsAsync(Guid userId, List<string> interests);
    Task UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest request);
    Task UpdateLocationAsync(Guid userId, UpdateLocationRequest request);
    Task SetTravelModeAsync(Guid userId, SetTravelModeRequest request);
    Task UpdateCoverPhotoAsync(Guid userId, string coverPhotoUrl);
    Task<ImageDto> AddImageAsync(Guid userId, AddImageRequest request);
    Task DeleteImageAsync(Guid userId, Guid imageId);
    Task ReorderImagesAsync(Guid userId, ReorderImagesRequest request);
    Task SetPrimaryImageAsync(Guid userId, Guid imageId);
    Task BlockUserAsync(Guid blockerId, Guid targetId);
    Task UnblockUserAsync(Guid blockerId, Guid targetId);
    Task<List<UserProfileDto>> GetBlockedUsersAsync(Guid userId);
    Task ReportUserAsync(Guid reporterId, string targetId, string reason, string? description);
    Task DeleteAccountAsync(Guid userId, DeleteAccountRequest request);
    Task SetOnlineStatusAsync(Guid userId, bool isOnline);
    Task<List<ContactOnAppDto>> GetContactsOnAppAsync(List<string> phoneNumbers);
    Task UpdateFcmTokenAsync(Guid userId, string token);
}

public interface IDiscoverService
{
    Task<(List<DiscoverUserDto> Users, PaginationDto Pagination)> GetFeedAsync(Guid userId, int page, int limit, DiscoverFilters? filters = null);
    Task<SwipeResponse> SwipeAsync(Guid swiperId, SwipeRequest request);
    Task<List<MatchListItemDto>> GetMatchesAsync(Guid userId, int page, int limit);
    Task UnmatchAsync(Guid userId, Guid matchId);
    Task<List<DiscoverUserDto>> GetWhoLikedMeAsync(Guid userId);
    // TASK 7: Trending section
    Task<List<TrendingSection>> GetTrendingAsync(Guid userId);
}

public interface IChatService
{
    Task<List<ChatListItemDto>> GetChatsAsync(Guid userId);
    Task<List<ChatMessageDto>> GetMessagesAsync(Guid userId, Guid chatId, int page);
    Task<SendMessageResponse> SendMessageAsync(Guid senderId, Guid chatId, SendMessageRequest request);
    Task MarkReadAsync(Guid userId, Guid chatId);
    Task DeleteMessageAsync(Guid userId, Guid chatId, Guid messageId);
    Task<ChatQuotaDto> GetQuotaAsync(Guid userId, Guid chatId);
    Task<SendCoinsResponse> SendCoinsAsync(Guid senderId, Guid chatId, SendCoinsRequest request);
}

public interface ICallService
{
    Task<object> InitiateCallAsync(Guid callerId, string targetId, string callType);
    Task<object> AnswerCallAsync(Guid receiverId, Guid callId);
    Task<object> EndCallAsync(Guid userId, Guid callId);
    Task DeclineCallAsync(Guid receiverId, Guid callId);
    Task<object> TimeoutCallAsync(Guid callId);   // TASK 4: handle missed calls
    Task<List<object>> GetHistoryAsync(Guid userId);
}

public interface ISuperChatService
{
    Task<SendSuperChatResponse> SendAsync(Guid fromUserId, SendSuperChatRequest request);
    Task<SuperChatDto> RespondAsync(Guid toUserId, Guid superChatId);
    Task<List<SuperChatDto>> GetReceivedAsync(Guid userId);
    Task<List<SuperChatDto>> GetSentAsync(Guid userId);
}

public interface ISubscriptionService
{
    Task<List<SubscriptionPlanDto>> GetPlansAsync();
    Task<UserSubscriptionDto?> GetStatusAsync(Guid userId);
    Task<SubscribeResponse> SubscribeAsync(Guid userId, SubscribeRequest request);
    Task CancelAsync(Guid userId, Guid subscriptionId, string? reason);
}

public interface IWalletService
{
    Task<WalletBalanceDto> GetBalanceAsync(Guid userId);
    Task<List<CoinPackageDto>> GetPackagesAsync();
    Task<List<CoinTransactionDto>> GetTransactionsAsync(Guid userId, string type);
    Task SubmitDepositAsync(Guid userId, DepositRequestDto request);
    Task SubmitWithdrawalAsync(Guid userId, WithdrawalRequestDto request);
    Task AddCoinsAsync(Guid userId, int coins, string description, string transactionType, string? referenceId = null);
    Task<bool> DeductCoinsAsync(Guid userId, int coins, string description, string transactionType, string? referenceId = null);
}

public interface INotificationService
{
    Task CreateAsync(Guid userId, string title, string body, string type, string? referenceId = null);
    Task<List<object>> GetAllAsync(Guid userId, int page);
    Task MarkReadAsync(Guid notificationId, Guid userId);
    Task MarkAllReadAsync(Guid userId);
}

public interface IHubNotifier
{
    Task SendToUserAsync(string userId, string method, object data);
    Task SendToGroupAsync(string group, string method, object data);
}