using System.Text.Json.Serialization;

namespace Mingley.Application.DTOs.Chat;

public class ChatListItemDto
{
    public string? ChatId    { get; set; }
    public string? MatchId   { get; set; }
    // Frontend expects 'user' and 'participant'
    [JsonPropertyName("user")]
    public ChatParticipantDto? Participant { get; set; }
    public ChatMessageDto? LastMessage { get; set; }
    public int UnreadCount { get; set; }
}

public class ChatParticipantDto
{
    public string? Id          { get; set; }
    public string? FullName    { get; set; }
    public string? Avatar      { get; set; }
    public bool IsOnline       { get; set; }
    public DateTime? LastActiveAt { get; set; }
}

public class ChatMessageDto
{
    public string? Id         { get; set; }
    public string? ChatId     { get; set; }

    // Frontend uses 'senderId'
    public string? SenderId   { get; set; }
    public string? SenderName { get; set; }
    public string? SenderAvatar { get; set; }

    // Frontend uses 'content', backend stores in 'text' — expose both
    [JsonPropertyName("content")]
    public string? Text       { get; set; }

    // Frontend uses 'messageType', backend uses 'type'
    [JsonPropertyName("messageType")]
    public string MessageType { get; set; } = "TEXT";

    public string? ImageUrl   { get; set; }
    public string? GiftName   { get; set; }
    public int? GiftCost      { get; set; }
    public int? CoinAmount    { get; set; }

    // Frontend uses 'createdAt'
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    public DateTime? ReadAt   { get; set; }
    public int CoinsDeducted  { get; set; }

    public string? ReplyToMessageId { get; set; }
    public string? ReplyToText      { get; set; }
    public bool IsDeleted           { get; set; }

    // Keep 'isRead' boolean for frontend
    [JsonPropertyName("isRead")]
    public bool IsRead => ReadAt.HasValue;
}

public class SendMessageRequest
{
    // Accept both 'content' and 'text' from frontend
    [JsonPropertyName("content")]
    public string? Content   { get; set; }
    public string? Text      { get; set; }
    public string ContentText => Content ?? Text ?? string.Empty;

    // Accept both 'messageType' and 'type'
    [JsonPropertyName("messageType")]
    public string? MessageType { get; set; }
    public string Type         => (MessageType ?? "text").ToLower();

    public string? ImageUrl           { get; set; }
    public string? ReplyToMessageId   { get; set; }
}

public class SendMessageResponse
{
    public string? Id           { get; set; }
    public int CoinsDeducted    { get; set; }
    public int NewBalance       { get; set; }
    public int Remaining        { get; set; }
    public ChatMessageDto? Message { get; set; }
}

public class ChatQuotaDto
{
    public int FreeRemaining    { get; set; }
    public int Remaining        { get; set; }
    public bool IsPremium       { get; set; }
    public int CostPerMessage   { get; set; }
}
