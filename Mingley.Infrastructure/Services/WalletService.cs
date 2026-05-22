using Microsoft.EntityFrameworkCore;
using Mingley.Application.DTOs.Wallet;
using Mingley.Application.Interfaces;
using Mingley.Domain.Entities;
using Mingley.Infrastructure.Persistence;

namespace Mingley.Infrastructure.Services;

public class WalletService : IWalletService
{
    private readonly MingleyDbContext _db;
    public WalletService(MingleyDbContext db) => _db = db;

    public async Task<WalletBalanceDto> GetBalanceAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        var spent = await _db.CoinTransactions
            .Where(t => t.UserId == userId && t.Direction == "debit")
            .SumAsync(t => (long)t.Coins);
        return new WalletBalanceDto
        {
            CoinBalance = user?.CoinBalance ?? 0,
            TotalEarned = user?.TotalEarned ?? 0,
            TotalSpent = spent,
        };
    }

    public Task<List<CoinPackageDto>> GetPackagesAsync() => Task.FromResult(new List<CoinPackageDto>
    {
        new() { Id = "pkg_100",  Coins = 100,  Price = 49,   Label = "Starter",  Badge = null,           IsPopular = false },
        new() { Id = "pkg_300",  Coins = 300,  Price = 129,  Label = "Popular",  Badge = null,           IsPopular = true  },
        new() { Id = "pkg_700",  Coins = 700,  Price = 249,  Label = "Value",    Badge = "+10% Bonus",   IsPopular = false },
        new() { Id = "pkg_1500", Coins = 1500, Price = 499,  Label = "Pro",      Badge = "+20% Bonus",   IsPopular = false },
        new() { Id = "pkg_5000", Coins = 5000, Price = 999,  Label = "Elite",    Badge = "Best Value!",  IsPopular = false },
    });

    public async Task<List<CoinTransactionDto>> GetTransactionsAsync(Guid userId, string type)
    {
        var q = _db.CoinTransactions.Where(t => t.UserId == userId);
        if (type != "all") q = q.Where(t => t.Direction == type);
        var txns = await q.OrderByDescending(t => t.CreatedAt).Take(100).ToListAsync();
        return txns.Select(t => new CoinTransactionDto
        {
            Id = t.Id.ToString(),
            Coins = t.Coins,
            Direction = t.Direction,
            Description = t.Description,
            TransactionType = t.TransactionType,
            CreatedAt = t.CreatedAt,
        }).ToList();
    }

    public async Task SubmitDepositAsync(Guid userId, DepositRequestDto req)
    {
        var user = await _db.Users.FindAsync(userId) ?? throw new InvalidOperationException("User not found.");

        // TASK 1: Only male users can deposit
        if (user.Gender?.ToLower() != "male")
            throw new InvalidOperationException("TopUp is available for Male users only.");

        _db.DepositRequests.Add(new DepositRequest
        {
            UserId = userId,
            UtrId = req.UtrId,
            ScreenshotUrl = req.ScreenshotUrl,
            RequestedCoins = req.RequestedCoins,
            Status = "pending",
        });
        await _db.SaveChangesAsync();
    }

    public async Task SubmitWithdrawalAsync(Guid userId, WithdrawalRequestDto req)
    {
        var user = await _db.Users.FindAsync(userId) ?? throw new InvalidOperationException("User not found.");

        // TASK 2: Only female users can withdraw
        if (user.Gender?.ToLower() != "female")
            throw new InvalidOperationException("Withdrawal is available for Female users only.");

        if (req.Coins <= 0)
            throw new InvalidOperationException("Withdrawal amount must be greater than 0.");

        if (string.IsNullOrWhiteSpace(req.BankOrUpi))
            throw new InvalidOperationException("Bank account or UPI ID is required.");

        // TASK 2: Maximum 70% of balance can be withdrawn
        var maxAllowed = (int)(user.CoinBalance * MingleyDbContext.FemaleWithdrawPct);
        if (req.Coins > maxAllowed)
            throw new InvalidOperationException(
                $"Maximum withdrawal is {maxAllowed} coins ({(int)(MingleyDbContext.FemaleWithdrawPct * 100)}% of your {user.CoinBalance} balance). " +
                $"30% must remain in your wallet.");

        if (user.CoinBalance < req.Coins)
            throw new InvalidOperationException($"Insufficient coins. You have {user.CoinBalance} coins.");

        user.CoinBalance -= req.Coins;
        user.UpdatedAt = DateTime.UtcNow;

        _db.WithdrawalRequests.Add(new WithdrawalRequest
        {
            UserId = userId,
            Coins = req.Coins,
            BankOrUpi = req.BankOrUpi,
            Status = "pending",
        });

        _db.CoinTransactions.Add(new CoinTransaction
        {
            UserId = userId,
            Coins = req.Coins,
            Direction = "debit",
            Description = $"Withdrawal request to {req.BankOrUpi}",
            TransactionType = "withdrawal",
        });

        await _db.SaveChangesAsync();
    }

    public async Task AddCoinsAsync(Guid userId, int coins, string description, string transactionType, string? referenceId = null)
    {
        var user = await _db.Users.FindAsync(userId) ?? throw new InvalidOperationException("User not found.");
        user.CoinBalance += coins;
        user.UpdatedAt = DateTime.UtcNow;
        _db.CoinTransactions.Add(new CoinTransaction
        {
            UserId = userId,
            Coins = coins,
            Direction = "credit",
            Description = description,
            TransactionType = transactionType,
            ReferenceId = referenceId,
        });
        await _db.SaveChangesAsync();
    }

    public async Task<bool> DeductCoinsAsync(Guid userId, int coins, string description, string transactionType, string? referenceId = null)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null || user.CoinBalance < coins) return false;
        user.CoinBalance -= coins;
        user.UpdatedAt = DateTime.UtcNow;
        _db.CoinTransactions.Add(new CoinTransaction
        {
            UserId = userId,
            Coins = coins,
            Direction = "debit",
            Description = description,
            TransactionType = transactionType,
            ReferenceId = referenceId,
        });
        await _db.SaveChangesAsync();
        return true;
    }
}

//using Microsoft.EntityFrameworkCore;
//using Mingley.Application.DTOs.Wallet;
//using Mingley.Application.Interfaces;
//using Mingley.Domain.Entities;
//using Mingley.Infrastructure.Persistence;

//namespace Mingley.Infrastructure.Services;

//public class WalletService : IWalletService
//{
//    private readonly MingleyDbContext _db;
//    public WalletService(MingleyDbContext db) => _db = db;

//    public async Task<WalletBalanceDto> GetBalanceAsync(Guid userId)
//    {
//        var user = await _db.Users.FindAsync(userId);
//        var spent = await _db.CoinTransactions
//            .Where(t => t.UserId == userId && t.Direction == "debit")
//            .SumAsync(t => (long)t.Coins);
//        return new WalletBalanceDto
//        {
//            CoinBalance  = user?.CoinBalance ?? 0,
//            TotalEarned  = user?.TotalEarned ?? 0,
//            TotalSpent   = spent,
//        };
//    }

//    public Task<List<CoinPackageDto>> GetPackagesAsync() => Task.FromResult(new List<CoinPackageDto>
//    {
//        new() { Id = "pkg_100",  Coins = 100,  Price = 49,  Label = "Starter", Badge = null,          IsPopular = false },
//        new() { Id = "pkg_300",  Coins = 300,  Price = 129, Label = "Popular", Badge = null,          IsPopular = true  },
//        new() { Id = "pkg_700",  Coins = 700,  Price = 249, Label = "Value",   Badge = "+10% Bonus",  IsPopular = false },
//        new() { Id = "pkg_1500", Coins = 1500, Price = 499, Label = "Pro",     Badge = "+20% Bonus",  IsPopular = false },
//        new() { Id = "pkg_5000", Coins = 5000, Price = 999, Label = "Elite",   Badge = "Best Value!", IsPopular = false },
//    });

//    public async Task<List<CoinTransactionDto>> GetTransactionsAsync(Guid userId, string type)
//    {
//        var q = _db.CoinTransactions.Where(t => t.UserId == userId);
//        if (type != "all") q = q.Where(t => t.Direction == type);
//        var txns = await q.OrderByDescending(t => t.CreatedAt).Take(100).ToListAsync();
//        return txns.Select(t => new CoinTransactionDto
//        {
//            Id = t.Id.ToString(), Coins = t.Coins, Direction = t.Direction,
//            Description = t.Description, TransactionType = t.TransactionType, CreatedAt = t.CreatedAt,
//        }).ToList();
//    }

//    public async Task SubmitDepositAsync(Guid userId, DepositRequestDto req)
//    {
//        _db.DepositRequests.Add(new DepositRequest
//        {
//            UserId = userId, UtrId = req.UtrId,
//            ScreenshotUrl = req.ScreenshotUrl, RequestedCoins = req.RequestedCoins, Status = "pending",
//        });
//        await _db.SaveChangesAsync();
//    }

//    public async Task SubmitWithdrawalAsync(Guid userId, WithdrawalRequestDto req)
//    {
//        var user = await _db.Users.FindAsync(userId) ?? throw new InvalidOperationException("User not found.");
//        if (user.Gender?.ToLower() != "female")
//            throw new InvalidOperationException("Only female users can withdraw coins.");
//        if (req.Coins <= 0)
//            throw new InvalidOperationException("Withdrawal amount must be positive.");
//        if (user.CoinBalance < req.Coins)
//            throw new InvalidOperationException($"Insufficient coins. You have {user.CoinBalance}.");

//        user.CoinBalance -= req.Coins;
//        user.UpdatedAt   = DateTime.UtcNow;

//        _db.WithdrawalRequests.Add(new WithdrawalRequest
//        { UserId = userId, Coins = req.Coins, BankOrUpi = req.BankOrUpi, Status = "pending" });

//        _db.CoinTransactions.Add(new CoinTransaction
//        { UserId = userId, Coins = req.Coins, Direction = "debit", Description = "Withdrawal request", TransactionType = "withdrawal" });

//        await _db.SaveChangesAsync();
//    }

//    public async Task AddCoinsAsync(Guid userId, int coins, string description, string transactionType, string? referenceId = null)
//    {
//        var user = await _db.Users.FindAsync(userId) ?? throw new InvalidOperationException("User not found.");
//        user.CoinBalance += coins;
//        user.UpdatedAt    = DateTime.UtcNow;
//        _db.CoinTransactions.Add(new CoinTransaction
//        { UserId = userId, Coins = coins, Direction = "credit", Description = description, TransactionType = transactionType, ReferenceId = referenceId });
//        await _db.SaveChangesAsync();
//    }

//    public async Task<bool> DeductCoinsAsync(Guid userId, int coins, string description, string transactionType, string? referenceId = null)
//    {
//        var user = await _db.Users.FindAsync(userId);
//        if (user == null || user.CoinBalance < coins) return false;
//        user.CoinBalance -= coins;
//        user.UpdatedAt    = DateTime.UtcNow;
//        _db.CoinTransactions.Add(new CoinTransaction
//        { UserId = userId, Coins = coins, Direction = "debit", Description = description, TransactionType = transactionType, ReferenceId = referenceId });
//        await _db.SaveChangesAsync();
//        return true;
//    }
//}
