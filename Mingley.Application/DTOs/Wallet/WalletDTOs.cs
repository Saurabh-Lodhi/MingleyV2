namespace Mingley.Application.DTOs.Wallet;

public class WalletBalanceDto
{
    public int CoinBalance { get; set; }
    public double TotalEarned { get; set; }
    public double TotalSpent { get; set; }
}

public class CoinPackageDto
{
    public string? Id { get; set; }
    public int Coins { get; set; }
    public decimal Price { get; set; }
    public bool IsPopular { get; set; }
    public string? Label { get; set; }
    public string? Badge { get; set; }  // e.g. "+20% BONUS"
}

public class CoinTransactionDto
{
    public string? Id { get; set; }
    public int Coins { get; set; }
    public string? Direction { get; set; }
    public string? Description { get; set; }
    public string? TransactionType { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DepositRequestDto
{
    public string? UtrId { get; set; }
    public string? ScreenshotUrl { get; set; }
    public int RequestedCoins { get; set; }
}

public class WithdrawalRequestDto
{
    public int Coins { get; set; }
    public string? BankOrUpi { get; set; }
}
