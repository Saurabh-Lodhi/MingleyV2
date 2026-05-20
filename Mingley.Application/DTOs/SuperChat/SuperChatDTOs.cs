namespace Mingley.Application.DTOs.SuperChat;

public class SuperChatDto
{
    public string? Id { get; set; }
    public string? FromUserId { get; set; }
    public string? FromUserName { get; set; }
    public string? FromUserAvatar { get; set; }
    public string? ToUserId { get; set; }
    public string? ToUserName { get; set; }
    public string? ToUserAvatar { get; set; }
    public string? Message { get; set; }
    public int CoinAmount { get; set; }
    public int CoinsAwarded { get; set; }
    public double GirlCommission { get; set; }
    public double CompanyRevenue { get; set; }
    public bool IsResponded { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string? MatchCreatedId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SendSuperChatRequest
{
    public string ToUserId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int CoinAmount { get; set; } = 500;
}

public class SendSuperChatResponse
{
    public string? SuperChatId { get; set; }
    public int CoinsDeducted { get; set; }
    public int NewBalance { get; set; }
    public double GirlCommission { get; set; }
    public double CompanyRevenue { get; set; }
    public int CoinsReceiverWillGet { get; set; }
}
//namespace Mingley.Application.DTOs.SuperChat;

//public class SuperChatDto
//{
//    public string? Id { get; set; }
//    public string? FromUserId { get; set; }
//    public string? FromUserName { get; set; }
//    public string? FromUserAvatar { get; set; }
//    public string? ToUserId { get; set; }
//    public string? ToUserName { get; set; }
//    public string? ToUserAvatar { get; set; }
//    public string? Message { get; set; }
//    public int CoinAmount { get; set; }
//    public double GirlCommission { get; set; }
//    public double CompanyRevenue { get; set; }
//    public bool IsResponded { get; set; }
//    public DateTime? RespondedAt { get; set; }
//    public string? MatchCreatedId { get; set; }
//    public DateTime CreatedAt { get; set; }
//}

//public class SendSuperChatRequest
//{
//    public string ToUserId { get; set; } = string.Empty;
//    public string Message { get; set; } = string.Empty;
//}

//public class SendSuperChatResponse
//{
//    public string? SuperChatId { get; set; }
//    public int CoinsDeducted { get; set; }
//    public int NewBalance { get; set; }
//    public double GirlCommission { get; set; }
//    public double CompanyRevenue { get; set; }
//}
