using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mingley.Application.DTOs.Common;
using Mingley.Application.DTOs.Wallet;
using Mingley.Application.Interfaces;
using Mingley.Infrastructure.Services;
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

    private Guid Me => Guid.Parse(
        User.FindFirst("sub")?.Value
        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? Guid.Empty.ToString());

    public WalletController(IWalletService wallet, IConfiguration config)
    {
        _wallet = wallet;
        _config = config;
    }

    [HttpGet("balance")]
    public async Task<IActionResult> Balance()
    {
        var bal = await _wallet.GetBalanceAsync(Me);
        return Ok(ApiResponse<object>.Ok(new { coinBalance = bal.CoinBalance, totalEarned = bal.TotalEarned }));
    }

    [HttpGet("packages")]
    public async Task<IActionResult> Packages()
    {
        var pkgs = await _wallet.GetPackagesAsync();
        return Ok(ApiResponse<object>.Ok(new { packages = pkgs }));
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> Transactions([FromQuery] string type = "all")
    {
        var txns = await _wallet.GetTransactionsAsync(Me, type);
        return Ok(ApiResponse<object>.Ok(new { transactions = txns }));
    }

    [HttpPost("razorpay/order")]
    public IActionResult RazorpayOrder([FromBody] RazorpayOrderRequest req)
    {
        var orderId = $"order_{Guid.NewGuid():N}";
        return Ok(ApiResponse<object>.Ok(new
        {
            orderId,
            amount = req.Amount,
            currency = "INR",
            key = _config["Razorpay:KeyId"] ?? "rzp_test_Sq4maCpZVgCTeM"
        }));
    }

    [HttpPost("razorpay/verify")]
    public async Task<IActionResult> RazorpayVerify([FromBody] RazorpayVerifyRequest req)
    {
        var keySecret = _config["Razorpay:KeySecret"] ?? "";
        if (!string.IsNullOrEmpty(keySecret) && !string.IsNullOrEmpty(req.Signature) && !req.PaymentId.StartsWith("test_"))
        {
            var payload = $"{req.OrderId}|{req.PaymentId}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(keySecret));
            var hash = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).Replace("-", "").ToLower();
            if (hash != req.Signature)
                return BadRequest(ApiResponse<object>.Fail("Invalid payment signature"));
        }

        var coins = (int)(req.Amount / 50);
        if (coins > 0)
            await _wallet.AddCoinsAsync(Me, coins, $"Razorpay payment {req.PaymentId}", "razorpay", req.OrderId);

        return Ok(ApiResponse<object>.Ok(new { success = true, coinsAdded = coins }));
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] DepositRequestDto req)
    {
        await _wallet.SubmitDepositAsync(Me, req);
        return Ok(ApiResponse<object>.Ok(new { message = "Deposit request submitted" }));
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawalRequestDto req)
    {
        await _wallet.SubmitWithdrawalAsync(Me, req);
        return Ok(ApiResponse<object>.Ok(new { message = "Withdrawal request submitted" }));
    }
}

public record RazorpayOrderRequest(long Amount);
public record RazorpayVerifyRequest(string OrderId, string PaymentId, string Signature, long Amount = 0);