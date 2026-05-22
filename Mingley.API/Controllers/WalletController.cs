using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mingley.Application.DTOs.Common;
using Mingley.Application.DTOs.Wallet;
using Mingley.Application.Interfaces;
using Mingley.Infrastructure.Persistence;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Mingley.API.Controllers;

[ApiController]
[Route("v1/wallet")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletService _wallet;
    private readonly IConfiguration _config;
    private readonly MingleyDbContext _db;

    private Guid Me => Guid.Parse(
        User.FindFirst("sub")?.Value
        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? Guid.Empty.ToString());

    public WalletController(IWalletService wallet, IConfiguration config, MingleyDbContext db)
    {
        _wallet = wallet;
        _config = config;
        _db = db;
    }

    [HttpGet("balance")]
    public async Task<IActionResult> Balance()
    {
        var user = await _db.Users.FindAsync(Me);
        var bal = await _wallet.GetBalanceAsync(Me);

        // Return withdrawal limit info for female users
        int maxWithdrawal = 0;
        if (user?.Gender?.ToLower() == "female")
            maxWithdrawal = (int)(bal.CoinBalance * MingleyDbContext.FemaleWithdrawPct);

        return Ok(ApiResponse<object>.Ok(new
        {
            coinBalance = bal.CoinBalance,
            totalEarned = bal.TotalEarned,
            totalSpent = bal.TotalSpent,
            gender = user?.Gender,
            canTopUp = user?.Gender?.ToLower() == "male",
            canWithdraw = user?.Gender?.ToLower() == "female",
            maxWithdrawalCoins = maxWithdrawal,
        }));
    }

    [HttpGet("packages")]
    public async Task<IActionResult> Packages()
    {
        var user = await _db.Users.FindAsync(Me);

        // TASK 1: TopUp packages only visible/usable for male users
        if (user?.Gender?.ToLower() != "male")
            return BadRequest(ApiResponse<object>.Fail("TopUp is available for Male users only."));

        var pkgs = await _wallet.GetPackagesAsync();
        return Ok(ApiResponse<object>.Ok(new { packages = pkgs }));
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> Transactions([FromQuery] string type = "all")
    {
        var txns = await _wallet.GetTransactionsAsync(Me, type);
        return Ok(ApiResponse<object>.Ok(new { transactions = txns }));
    }

    // TASK 1: TopUp — Males only
    [HttpPost("razorpay/order")]
    public async Task<IActionResult> RazorpayOrder([FromBody] RazorpayOrderRequest req)
    {
        var user = await _db.Users.FindAsync(Me);
        if (user?.Gender?.ToLower() != "male")
            return BadRequest(ApiResponse<object>.Fail("TopUp is available for Male users only."));

        var orderId = $"order_{Guid.NewGuid():N}";
        return Ok(ApiResponse<object>.Ok(new
        {
            orderId,
            amount = req.Amount,
            currency = "INR",
            key = _config["Razorpay:KeyId"] ?? "rzp_test_Sq4maCpZVgCTeM",
        }));
    }

    // TASK 1: Payment verify — Males only
    [HttpPost("razorpay/verify")]
    public async Task<IActionResult> RazorpayVerify([FromBody] RazorpayVerifyRequest req)
    {
        var user = await _db.Users.FindAsync(Me);
        if (user?.Gender?.ToLower() != "male")
            return BadRequest(ApiResponse<object>.Fail("TopUp is available for Male users only."));

        var keySecret = _config["Razorpay:KeySecret"] ?? "";
        if (!string.IsNullOrEmpty(keySecret)
            && !string.IsNullOrEmpty(req.Signature)
            && !req.PaymentId.StartsWith("test_"))
        {
            var payload = $"{req.OrderId}|{req.PaymentId}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(keySecret));
            var hash = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)))
                .Replace("-", "").ToLower();
            if (hash != req.Signature)
                return BadRequest(ApiResponse<object>.Fail("Invalid payment signature. Please contact support."));
        }

        // 1 coin per ₹0.50 (₹499 = 100 coins)
        var coins = (int)(req.Amount / 50);
        if (coins > 0)
            await _wallet.AddCoinsAsync(Me, coins,
                $"Razorpay TopUp ₹{req.Amount / 100}", "razorpay", req.OrderId);

        var newBalance = (await _db.Users.FindAsync(Me))?.CoinBalance ?? 0;
        return Ok(ApiResponse<object>.Ok(new
        {
            success = true,
            coinsAdded = coins,
            newBalance,
        }));
    }

    // TASK 1: Manual deposit — Males only
    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] DepositRequestDto req)
    {
        var user = await _db.Users.FindAsync(Me);
        if (user?.Gender?.ToLower() != "male")
            return BadRequest(ApiResponse<object>.Fail("TopUp is available for Male users only."));

        await _wallet.SubmitDepositAsync(Me, req);
        return Ok(ApiResponse<object>.Ok(new { message = "Deposit request submitted. Admin will verify within 24 hours." }));
    }

    // TASK 2: Withdrawal — Females only, max 70%
    [HttpGet("withdrawal-limit")]
    public async Task<IActionResult> WithdrawalLimit()
    {
        var user = await _db.Users.FindAsync(Me);
        if (user?.Gender?.ToLower() != "female")
            return BadRequest(ApiResponse<object>.Fail("Withdrawal is available for Female users only."));

        var balance = user.CoinBalance;
        var maxWithdraw = (int)(balance * MingleyDbContext.FemaleWithdrawPct);
        var remainingAfter = balance - maxWithdraw;

        return Ok(ApiResponse<object>.Ok(new
        {
            currentBalance = balance,
            maxWithdrawalCoins = maxWithdraw,
            remainingInWallet = remainingAfter,
            withdrawalPct = (int)(MingleyDbContext.FemaleWithdrawPct * 100),
            inrEquivalent = maxWithdraw * MingleyDbContext.CoinToInrRate,
            note = $"You can withdraw up to {(int)(MingleyDbContext.FemaleWithdrawPct * 100)}% of your balance. " +
                   $"30% stays in your wallet.",
        }));
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawalRequestDto req)
    {
        await _wallet.SubmitWithdrawalAsync(Me, req);
        return Ok(ApiResponse<object>.Ok(new
        {
            message = $"Withdrawal of {req.Coins} coins submitted. Processing in 3-5 business days.",
            inrAmount = req.Coins * MingleyDbContext.CoinToInrRate,
        }));
    }
}

public record RazorpayOrderRequest(long Amount);
public record RazorpayVerifyRequest(string OrderId, string PaymentId, string Signature, long Amount = 0);

//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Mingley.Application.DTOs.Common;
//using Mingley.Application.DTOs.Wallet;
//using Mingley.Application.Interfaces;
//using Mingley.Infrastructure.Services;
//using System.Security.Cryptography;
//using System.Text;

//namespace Mingley.API.Controllers;

//[ApiController]
//[Route("v1/wallet")]
//[Authorize]
//public class WalletController : ControllerBase
//{
//    private readonly IWalletService _wallet;

//    private readonly IConfiguration _config;

//    private Guid Me => Guid.Parse(
//        User.FindFirst("sub")?.Value
//        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
//        ?? Guid.Empty.ToString());

//    public WalletController(IWalletService wallet, IConfiguration config)
//    {
//        _wallet = wallet;
//        _config = config;
//    }

//    [HttpGet("balance")]
//    public async Task<IActionResult> Balance()
//    {
//        var bal = await _wallet.GetBalanceAsync(Me);
//        return Ok(ApiResponse<object>.Ok(new { coinBalance = bal.CoinBalance, totalEarned = bal.TotalEarned }));
//    }

//    [HttpGet("packages")]
//    public async Task<IActionResult> Packages()
//    {
//        var pkgs = await _wallet.GetPackagesAsync();
//        return Ok(ApiResponse<object>.Ok(new { packages = pkgs }));
//    }

//    [HttpGet("transactions")]
//    public async Task<IActionResult> Transactions([FromQuery] string type = "all")
//    {
//        var txns = await _wallet.GetTransactionsAsync(Me, type);
//        return Ok(ApiResponse<object>.Ok(new { transactions = txns }));
//    }

//    [HttpPost("razorpay/order")]
//    public IActionResult RazorpayOrder([FromBody] RazorpayOrderRequest req)
//    {
//        var orderId = $"order_{Guid.NewGuid():N}";
//        return Ok(ApiResponse<object>.Ok(new
//        {
//            orderId,
//            amount = req.Amount,
//            currency = "INR",
//            key = _config["Razorpay:KeyId"] ?? "rzp_test_Sq4maCpZVgCTeM"
//        }));
//    }

//    [HttpPost("razorpay/verify")]
//    public async Task<IActionResult> RazorpayVerify([FromBody] RazorpayVerifyRequest req)
//    {
//        var keySecret = _config["Razorpay:KeySecret"] ?? "";
//        if (!string.IsNullOrEmpty(keySecret) && !string.IsNullOrEmpty(req.Signature) && !req.PaymentId.StartsWith("test_"))
//        {
//            var payload = $"{req.OrderId}|{req.PaymentId}";
//            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(keySecret));
//            var hash = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).Replace("-", "").ToLower();
//            if (hash != req.Signature)
//                return BadRequest(ApiResponse<object>.Fail("Invalid payment signature"));
//        }

//        var coins = (int)(req.Amount / 50);
//        if (coins > 0)
//            await _wallet.AddCoinsAsync(Me, coins, $"Razorpay payment {req.PaymentId}", "razorpay", req.OrderId);

//        return Ok(ApiResponse<object>.Ok(new { success = true, coinsAdded = coins }));
//    }

//    [HttpPost("deposit")]
//    public async Task<IActionResult> Deposit([FromBody] DepositRequestDto req)
//    {
//        await _wallet.SubmitDepositAsync(Me, req);
//        return Ok(ApiResponse<object>.Ok(new { message = "Deposit request submitted" }));
//    }

//    [HttpPost("withdraw")]
//    public async Task<IActionResult> Withdraw([FromBody] WithdrawalRequestDto req)
//    {
//        await _wallet.SubmitWithdrawalAsync(Me, req);
//        return Ok(ApiResponse<object>.Ok(new { message = "Withdrawal request submitted" }));
//    }
//}

//public record RazorpayOrderRequest(long Amount);
//public record RazorpayVerifyRequest(string OrderId, string PaymentId, string Signature, long Amount = 0);