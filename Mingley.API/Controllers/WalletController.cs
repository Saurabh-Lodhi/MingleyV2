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
using System.Text.Json;

namespace Mingley.API.Controllers;

[ApiController]
[Route("v1/wallet")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletService _wallet;
    private readonly IConfiguration _config;
    private readonly MingleyDbContext _db;
    private readonly IHttpClientFactory _http;

    private Guid Me => Guid.Parse(
        User.FindFirst("sub")?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? Guid.Empty.ToString());

    public WalletController(IWalletService wallet, IConfiguration config,
                            MingleyDbContext db, IHttpClientFactory http)
    {
        _wallet = wallet;
        _config = config;
        _db = db;
        _http = http;
    }

    // ── Balance ───────────────────────────────────────────────────────
    [HttpGet("balance")]
    public async Task<IActionResult> Balance()
    {
        var user = await _db.Users.FindAsync(Me);
        var bal = await _wallet.GetBalanceAsync(Me);

        var maxWithdrawal = user?.Gender?.ToLower() == "female"
            ? (int)(bal.CoinBalance * MingleyDbContext.FemaleWithdrawPct)
            : 0;

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

    // ── Packages (males only) ─────────────────────────────────────────
    [HttpGet("packages")]
    public async Task<IActionResult> Packages()
    {
        var user = await _db.Users.FindAsync(Me);
        if (user?.Gender?.ToLower() != "male")
            return BadRequest(ApiResponse<object>.Fail("TopUp is available for Male users only."));

        var pkgs = await _wallet.GetPackagesAsync();
        return Ok(ApiResponse<object>.Ok(new { packages = pkgs }));
    }

    // ── Transactions ──────────────────────────────────────────────────
    [HttpGet("transactions")]
    public async Task<IActionResult> Transactions([FromQuery] string type = "all")
    {
        var txns = await _wallet.GetTransactionsAsync(Me, type);
        return Ok(ApiResponse<object>.Ok(new { transactions = txns }));
    }

    // ── Razorpay: Create real order via Razorpay Orders API ───────────
    // Returns a real order_id that the Razorpay SDK on the client needs.
    [HttpPost("razorpay/order")]
    public async Task<IActionResult> RazorpayOrder([FromBody] RazorpayOrderRequest req)
    {
        var user = await _db.Users.FindAsync(Me);
        if (user?.Gender?.ToLower() != "male")
            return BadRequest(ApiResponse<object>.Fail("TopUp is available for Male users only."));

        if (req.PackageId is null)
            return BadRequest(ApiResponse<object>.Fail("packageId is required."));

        // Resolve package — amount comes from server, not client (prevents price tampering)
        var pkg = GetPackage(req.PackageId);
        if (pkg is null)
            return BadRequest(ApiResponse<object>.Fail("Invalid package."));

        var keyId = _config["Razorpay:KeyId"] ?? throw new Exception("Razorpay:KeyId missing");
        var keySecret = _config["Razorpay:KeySecret"] ?? throw new Exception("Razorpay:KeySecret missing");

        // Call Razorpay Orders API
        var client = _http.CreateClient();
        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{keyId}:{keySecret}"));
        client.DefaultRequestHeaders.Add("Authorization", $"Basic {auth}");

        var body = JsonSerializer.Serialize(new
        {
            amount = pkg.PriceInPaise,          // paise, server-side
            currency = "INR",
            receipt = $"rcpt_{Me}_{Guid.NewGuid():N}".Substring(0, 40),
            notes = new { userId = Me.ToString(), packageId = req.PackageId, coins = pkg.Coins }
        });

        var response = await client.PostAsync(
            "https://api.razorpay.com/v1/orders",
            new StringContent(body, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            return StatusCode(502, ApiResponse<object>.Fail($"Razorpay order creation failed: {err}"));
        }

        var json = await response.Content.ReadAsStringAsync();
        var rzpResp = JsonSerializer.Deserialize<JsonElement>(json);
        var orderId = rzpResp.GetProperty("id").GetString();

        return Ok(ApiResponse<object>.Ok(new
        {
            orderId,
            amount = pkg.PriceInPaise,
            currency = "INR",
            coins = pkg.Coins,
            packageId = req.PackageId,
            key = keyId,
        }));
    }

    // ── Razorpay: Verify payment + credit coins ────────────────────────
    [HttpPost("razorpay/verify")]
    public async Task<IActionResult> RazorpayVerify([FromBody] RazorpayVerifyRequest req)
    {
        var user = await _db.Users.FindAsync(Me);
        if (user?.Gender?.ToLower() != "male")
            return BadRequest(ApiResponse<object>.Fail("TopUp is available for Male users only."));

        // ── 1. Idempotency: prevent double-credit for same paymentId ───
        var alreadyProcessed = await _db.CoinTransactions
            .AnyAsync(t => t.ReferenceId == req.PaymentId && t.TransactionType == "razorpay");
        if (alreadyProcessed)
            return BadRequest(ApiResponse<object>.Fail("Payment already processed."));

        // ── 2. HMAC-SHA256 signature verification (always enforced) ────
        var keySecret = _config["Razorpay:KeySecret"]
            ?? throw new Exception("Razorpay:KeySecret missing");

        var payload = $"{req.OrderId}|{req.PaymentId}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(keySecret));
        var expected = BitConverter.ToString(
            hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)))
            .Replace("-", "").ToLower();

        if (expected != req.Signature?.ToLower())
            return BadRequest(ApiResponse<object>.Fail("Payment signature verification failed. Do not retry — contact support."));

        // ── 3. Resolve coins from packageId (server-side, tamper-proof) ─
        var pkg = GetPackage(req.PackageId);
        if (pkg is null)
            return BadRequest(ApiResponse<object>.Fail("Invalid package."));

        // ── 4. Credit coins atomically ─────────────────────────────────
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            await _wallet.AddCoinsAsync(Me, pkg.Coins,
                $"TopUp — {pkg.Coins} coins via Razorpay (₹{pkg.PriceInPaise / 100})",
                "razorpay", req.PaymentId);

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        var newBalance = (await _db.Users.FindAsync(Me))?.CoinBalance ?? 0;

        return Ok(ApiResponse<object>.Ok(new
        {
            success = true,
            coinsAdded = pkg.Coins,
            newBalance,
            paymentId = req.PaymentId,
        }));
    }

    // ── Manual deposit (UPI screenshot) — Males only ──────────────────
    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] DepositRequestDto req)
    {
        var user = await _db.Users.FindAsync(Me);
        if (user?.Gender?.ToLower() != "male")
            return BadRequest(ApiResponse<object>.Fail("TopUp is available for Male users only."));

        await _wallet.SubmitDepositAsync(Me, req);
        return Ok(ApiResponse<object>.Ok(new
        {
            message = "Deposit request submitted. Admin will verify and credit coins within 24 hours."
        }));
    }

    // ── Withdrawal limit info — Females only ──────────────────────────
    [HttpGet("withdrawal-limit")]
    public async Task<IActionResult> WithdrawalLimit()
    {
        var user = await _db.Users.FindAsync(Me);
        if (user?.Gender?.ToLower() != "female")
            return BadRequest(ApiResponse<object>.Fail("Withdrawal is available for Female users only."));

        var balance = user.CoinBalance;
        var maxWithdraw = (int)(balance * MingleyDbContext.FemaleWithdrawPct);
        var pct = (int)(MingleyDbContext.FemaleWithdrawPct * 100);

        return Ok(ApiResponse<object>.Ok(new
        {
            currentBalance = balance,
            maxWithdrawalCoins = maxWithdraw,
            remainingInWallet = balance - maxWithdraw,
            withdrawalPct = pct,
            inrEquivalent = Math.Round(maxWithdraw * MingleyDbContext.CoinToInrRate, 2),
            note = $"You can withdraw up to {pct}% of your balance. {100 - pct}% stays in your wallet.",
        }));
    }

    // ── Withdraw — Females only ───────────────────────────────────────
    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawalRequestDto req)
    {
        await _wallet.SubmitWithdrawalAsync(Me, req);
        return Ok(ApiResponse<object>.Ok(new
        {
            message = $"Withdrawal of {req.Coins} coins submitted. Processing in 3–5 business days.",
            inrAmount = Math.Round(req.Coins * MingleyDbContext.CoinToInrRate, 2),
        }));
    }

    // ── Package lookup — server-side only, never trust client ─────────
    private static CoinPackageInternal? GetPackage(string? id) => id switch
    {
        "pkg_100" => new("pkg_100", 100, 49, 4900),
        "pkg_300" => new("pkg_300", 300, 129, 12900),
        "pkg_700" => new("pkg_700", 700, 249, 24900),
        "pkg_1500" => new("pkg_1500", 1500, 499, 49900),
        "pkg_5000" => new("pkg_5000", 5000, 999, 99900),
        _ => null
    };

    private record CoinPackageInternal(string Id, int Coins, int Price, long PriceInPaise);
}

// ── Request/Response DTOs ─────────────────────────────────────────────────────
public record RazorpayOrderRequest(string? PackageId);
public record RazorpayVerifyRequest(string OrderId, string PaymentId, string Signature, string PackageId);
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Mingley.Application.DTOs.Common;
//using Mingley.Application.DTOs.Wallet;
//using Mingley.Application.Interfaces;
//using Mingley.Infrastructure.Persistence;
//using System.Security.Claims;
//using System.Security.Cryptography;
//using System.Text;
//using System.Text.Json;

//namespace Mingley.API.Controllers;

//[ApiController]
//[Route("v1/wallet")]
//[Authorize]
//public class WalletController : ControllerBase
//{
//    private readonly IWalletService _wallet;
//    private readonly IConfiguration _config;
//    private readonly MingleyDbContext _db;
//    private readonly IHttpClientFactory _http;

//    private Guid Me => Guid.Parse(
//        User.FindFirst("sub")?.Value
//        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
//        ?? Guid.Empty.ToString());

//    public WalletController(IWalletService wallet, IConfiguration config,
//                            MingleyDbContext db, IHttpClientFactory http)
//    {
//        _wallet = wallet;
//        _config = config;
//        _db = db;
//        _http = http;
//    }

//    // ── Balance ───────────────────────────────────────────────────────
//    [HttpGet("balance")]
//    public async Task<IActionResult> Balance()
//    {
//        var user = await _db.Users.FindAsync(Me);
//        var bal = await _wallet.GetBalanceAsync(Me);

//        var maxWithdrawal = user?.Gender?.ToLower() == "female"
//            ? (int)(bal.CoinBalance * MingleyDbContext.FemaleWithdrawPct)
//            : 0;

//        return Ok(ApiResponse<object>.Ok(new
//        {
//            coinBalance = bal.CoinBalance,
//            totalEarned = bal.TotalEarned,
//            totalSpent = bal.TotalSpent,
//            gender = user?.Gender,
//            canTopUp = user?.Gender?.ToLower() == "male",
//            canWithdraw = user?.Gender?.ToLower() == "female",
//            maxWithdrawalCoins = maxWithdrawal,
//        }));
//    }

//    // ── Packages (males only) ─────────────────────────────────────────
//    [HttpGet("packages")]
//    public async Task<IActionResult> Packages()
//    {
//        var user = await _db.Users.FindAsync(Me);
//        if (user?.Gender?.ToLower() != "male")
//            return BadRequest(ApiResponse<object>.Fail("TopUp is available for Male users only."));

//        var pkgs = await _wallet.GetPackagesAsync();
//        return Ok(ApiResponse<object>.Ok(new { packages = pkgs }));
//    }

//    // ── Transactions ──────────────────────────────────────────────────
//    [HttpGet("transactions")]
//    public async Task<IActionResult> Transactions([FromQuery] string type = "all")
//    {
//        var txns = await _wallet.GetTransactionsAsync(Me, type);
//        return Ok(ApiResponse<object>.Ok(new { transactions = txns }));
//    }

//    // ── Razorpay: Create real order via Razorpay Orders API ───────────
//    // Returns a real order_id that the Razorpay SDK on the client needs.
//    [HttpPost("razorpay/order")]
//    public async Task<IActionResult> RazorpayOrder([FromBody] RazorpayOrderRequest req)
//    {
//        var user = await _db.Users.FindAsync(Me);
//        if (user?.Gender?.ToLower() != "male")
//            return BadRequest(ApiResponse<object>.Fail("TopUp is available for Male users only."));

//        if (req.PackageId is null)
//            return BadRequest(ApiResponse<object>.Fail("packageId is required."));

//        // Resolve package — amount comes from server, not client (prevents price tampering)
//        var pkg = GetPackage(req.PackageId);
//        if (pkg is null)
//            return BadRequest(ApiResponse<object>.Fail("Invalid package."));

//        var keyId = _config["Razorpay:KeyId"] ?? throw new Exception("Razorpay:KeyId missing");
//        var keySecret = _config["Razorpay:KeySecret"] ?? throw new Exception("Razorpay:KeySecret missing");

//        // Call Razorpay Orders API
//        var client = _http.CreateClient();
//        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{keyId}:{keySecret}"));
//        client.DefaultRequestHeaders.Add("Authorization", $"Basic {auth}");

//        var body = JsonSerializer.Serialize(new
//        {
//            amount = pkg.PriceInPaise,          // paise, server-side
//            currency = "INR",
//            receipt = $"rcpt_{Me}_{Guid.NewGuid():N}".Substring(0, 40),
//            notes = new { userId = Me.ToString(), packageId = req.PackageId, coins = pkg.Coins }
//        });

//        var response = await client.PostAsync(
//            "https://api.razorpay.com/v1/orders",
//            new StringContent(body, Encoding.UTF8, "application/json"));

//        if (!response.IsSuccessStatusCode)
//        {
//            var err = await response.Content.ReadAsStringAsync();
//            return StatusCode(502, ApiResponse<object>.Fail($"Razorpay order creation failed: {err}"));
//        }

//        var json = await response.Content.ReadAsStringAsync();
//        var rzpResp = JsonSerializer.Deserialize<JsonElement>(json);
//        var orderId = rzpResp.GetProperty("id").GetString();

//        return Ok(ApiResponse<object>.Ok(new
//        {
//            orderId,
//            amount = pkg.PriceInPaise,
//            currency = "INR",
//            coins = pkg.Coins,
//            packageId = req.PackageId,
//            key = keyId,
//        }));
//    }

//    // ── Razorpay: Verify payment + credit coins ────────────────────────
//    [HttpPost("razorpay/verify")]
//    public async Task<IActionResult> RazorpayVerify([FromBody] RazorpayVerifyRequest req)
//    {
//        var user = await _db.Users.FindAsync(Me);
//        if (user?.Gender?.ToLower() != "male")
//            return BadRequest(ApiResponse<object>.Fail("TopUp is available for Male users only."));

//        // ── 1. Idempotency: prevent double-credit for same paymentId ───
//        var alreadyProcessed = await _db.CoinTransactions
//            .AnyAsync(t => t.ReferenceId == req.PaymentId && t.TransactionType == "razorpay");
//        if (alreadyProcessed)
//            return BadRequest(ApiResponse<object>.Fail("Payment already processed."));

//        // ── 2. HMAC-SHA256 signature verification (always enforced) ────
//        var keySecret = _config["Razorpay:KeySecret"]
//            ?? throw new Exception("Razorpay:KeySecret missing");

//        var payload = $"{req.OrderId}|{req.PaymentId}";
//        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(keySecret));
//        var expected = BitConverter.ToString(
//            hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)))
//            .Replace("-", "").ToLower();

//        if (expected != req.Signature?.ToLower())
//            return BadRequest(ApiResponse<object>.Fail("Payment signature verification failed. Do not retry — contact support."));

//        // ── 3. Resolve coins from packageId (server-side, tamper-proof) ─
//        var pkg = GetPackage(req.PackageId);
//        if (pkg is null)
//            return BadRequest(ApiResponse<object>.Fail("Invalid package."));

//        // ── 4. Credit coins atomically ─────────────────────────────────
//        await using var tx = await _db.Database.BeginTransactionAsync();
//        try
//        {
//            await _wallet.AddCoinsAsync(Me, pkg.Coins,
//                $"TopUp — {pkg.Coins} coins via Razorpay (₹{pkg.PriceInPaise / 100})",
//                "razorpay", req.PaymentId);

//            await tx.CommitAsync();
//        }
//        catch
//        {
//            await tx.RollbackAsync();
//            throw;
//        }

//        var newBalance = (await _db.Users.FindAsync(Me))?.CoinBalance ?? 0;

//        return Ok(ApiResponse<object>.Ok(new
//        {
//            success = true,
//            coinsAdded = pkg.Coins,
//            newBalance,
//            paymentId = req.PaymentId,
//        }));
//    }

//    // ── Manual deposit (UPI screenshot) — Males only ──────────────────
//    [HttpPost("deposit")]
//    public async Task<IActionResult> Deposit([FromBody] DepositRequestDto req)
//    {
//        var user = await _db.Users.FindAsync(Me);
//        if (user?.Gender?.ToLower() != "male")
//            return BadRequest(ApiResponse<object>.Fail("TopUp is available for Male users only."));

//        await _wallet.SubmitDepositAsync(Me, req);
//        return Ok(ApiResponse<object>.Ok(new
//        {
//            message = "Deposit request submitted. Admin will verify and credit coins within 24 hours."
//        }));
//    }

//    // ── Withdrawal limit info — Females only ──────────────────────────
//    [HttpGet("withdrawal-limit")]
//    public async Task<IActionResult> WithdrawalLimit()
//    {
//        var user = await _db.Users.FindAsync(Me);
//        if (user?.Gender?.ToLower() != "female")
//            return BadRequest(ApiResponse<object>.Fail("Withdrawal is available for Female users only."));

//        var balance = user.CoinBalance;
//        var maxWithdraw = (int)(balance * MingleyDbContext.FemaleWithdrawPct);
//        var pct = (int)(MingleyDbContext.FemaleWithdrawPct * 100);

//        return Ok(ApiResponse<object>.Ok(new
//        {
//            currentBalance = balance,
//            maxWithdrawalCoins = maxWithdraw,
//            remainingInWallet = balance - maxWithdraw,
//            withdrawalPct = pct,
//            inrEquivalent = Math.Round(maxWithdraw * MingleyDbContext.CoinToInrRate, 2),
//            note = $"You can withdraw up to {pct}% of your balance. {100 - pct}% stays in your wallet.",
//        }));
//    }

//    // ── Withdraw — Females only ───────────────────────────────────────
//    [HttpPost("withdraw")]
//    public async Task<IActionResult> Withdraw([FromBody] WithdrawalRequestDto req)
//    {
//        await _wallet.SubmitWithdrawalAsync(Me, req);
//        return Ok(ApiResponse<object>.Ok(new
//        {
//            message = $"Withdrawal of {req.Coins} coins submitted. Processing in 3–5 business days.",
//            inrAmount = Math.Round(req.Coins * MingleyDbContext.CoinToInrRate, 2),
//        }));
//    }

//    // ── Package lookup — server-side only, never trust client ─────────
//    private static CoinPackageInternal? GetPackage(string? id) => id switch
//    {
//        "pkg_100" => new("pkg_100", 100, 49, 4900),
//        "pkg_300" => new("pkg_300", 300, 129, 12900),
//        "pkg_700" => new("pkg_700", 700, 249, 24900),
//        "pkg_1500" => new("pkg_1500", 1500, 499, 49900),
//        "pkg_5000" => new("pkg_5000", 5000, 999, 99900),
//        _ => null
//    };

//    private record CoinPackageInternal(string Id, int Coins, int Price, long PriceInPaise);
//}

//// ── Request/Response DTOs ─────────────────────────────────────────────────────
//public record RazorpayOrderRequest(string? PackageId);
//public record RazorpayVerifyRequest(string OrderId, string PaymentId, string Signature, string PackageId);

////using Microsoft.AspNetCore.Authorization;
////using Microsoft.AspNetCore.Mvc;
////using Microsoft.EntityFrameworkCore;
////using Mingley.Application.DTOs.Common;
////using Mingley.Application.DTOs.Wallet;
////using Mingley.Application.Interfaces;
////using Mingley.Infrastructure.Persistence;
////using System.Security.Claims;
////using System.Security.Cryptography;
////using System.Text;

////namespace Mingley.API.Controllers;

////[ApiController]
////[Route("v1/wallet")]
////[Authorize]
////public class WalletController : ControllerBase
////{
////    private readonly IWalletService _wallet;
////    private readonly IConfiguration _config;
////    private readonly MingleyDbContext _db;

////    private Guid Me => Guid.Parse(
////        User.FindFirst("sub")?.Value
////        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
////        ?? Guid.Empty.ToString());

////    public WalletController(IWalletService wallet, IConfiguration config, MingleyDbContext db)
////    {
////        _wallet = wallet;
////        _config = config;
////        _db = db;
////    }

////    [HttpGet("balance")]
////    public async Task<IActionResult> Balance()
////    {
////        var user = await _db.Users.FindAsync(Me);
////        var bal = await _wallet.GetBalanceAsync(Me);

////        // Return withdrawal limit info for female users
////        int maxWithdrawal = 0;
////        if (user?.Gender?.ToLower() == "female")
////            maxWithdrawal = (int)(bal.CoinBalance * MingleyDbContext.FemaleWithdrawPct);

////        return Ok(ApiResponse<object>.Ok(new
////        {
////            coinBalance = bal.CoinBalance,
////            totalEarned = bal.TotalEarned,
////            totalSpent = bal.TotalSpent,
////            gender = user?.Gender,
////            canTopUp = user?.Gender?.ToLower() == "male",
////            canWithdraw = user?.Gender?.ToLower() == "female",
////            maxWithdrawalCoins = maxWithdrawal,
////        }));
////    }

////    [HttpGet("packages")]
////    public async Task<IActionResult> Packages()
////    {
////        var user = await _db.Users.FindAsync(Me);

////        // TASK 1: TopUp packages only visible/usable for male users
////        if (user?.Gender?.ToLower() != "male")
////            return BadRequest(ApiResponse<object>.Fail("TopUp is available for Male users only."));

////        var pkgs = await _wallet.GetPackagesAsync();
////        return Ok(ApiResponse<object>.Ok(new { packages = pkgs }));
////    }

////    [HttpGet("transactions")]
////    public async Task<IActionResult> Transactions([FromQuery] string type = "all")
////    {
////        var txns = await _wallet.GetTransactionsAsync(Me, type);
////        return Ok(ApiResponse<object>.Ok(new { transactions = txns }));
////    }

////    // TASK 1: TopUp — Males only
////    [HttpPost("razorpay/order")]
////    public async Task<IActionResult> RazorpayOrder([FromBody] RazorpayOrderRequest req)
////    {
////        var user = await _db.Users.FindAsync(Me);
////        if (user?.Gender?.ToLower() != "male")
////            return BadRequest(ApiResponse<object>.Fail("TopUp is available for Male users only."));

////        var orderId = $"order_{Guid.NewGuid():N}";
////        return Ok(ApiResponse<object>.Ok(new
////        {
////            orderId,
////            amount = req.Amount,
////            currency = "INR",
////            key = _config["Razorpay:KeyId"] ?? "rzp_test_Sq4maCpZVgCTeM",
////        }));
////    }

////    // TASK 1: Payment verify — Males only
////    [HttpPost("razorpay/verify")]
////    public async Task<IActionResult> RazorpayVerify([FromBody] RazorpayVerifyRequest req)
////    {
////        var user = await _db.Users.FindAsync(Me);
////        if (user?.Gender?.ToLower() != "male")
////            return BadRequest(ApiResponse<object>.Fail("TopUp is available for Male users only."));

////        var keySecret = _config["Razorpay:KeySecret"] ?? "";
////        if (!string.IsNullOrEmpty(keySecret)
////            && !string.IsNullOrEmpty(req.Signature)
////            && !req.PaymentId.StartsWith("test_"))
////        {
////            var payload = $"{req.OrderId}|{req.PaymentId}";
////            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(keySecret));
////            var hash = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)))
////                .Replace("-", "").ToLower();
////            if (hash != req.Signature)
////                return BadRequest(ApiResponse<object>.Fail("Invalid payment signature. Please contact support."));
////        }

////        // 1 coin per ₹0.50 (₹499 = 100 coins)
////        var coins = (int)(req.Amount / 50);
////        if (coins > 0)
////            await _wallet.AddCoinsAsync(Me, coins,
////                $"Razorpay TopUp ₹{req.Amount / 100}", "razorpay", req.OrderId);

////        var newBalance = (await _db.Users.FindAsync(Me))?.CoinBalance ?? 0;
////        return Ok(ApiResponse<object>.Ok(new
////        {
////            success = true,
////            coinsAdded = coins,
////            newBalance,
////        }));
////    }

////    // TASK 1: Manual deposit — Males only
////    [HttpPost("deposit")]
////    public async Task<IActionResult> Deposit([FromBody] DepositRequestDto req)
////    {
////        var user = await _db.Users.FindAsync(Me);
////        if (user?.Gender?.ToLower() != "male")
////            return BadRequest(ApiResponse<object>.Fail("TopUp is available for Male users only."));

////        await _wallet.SubmitDepositAsync(Me, req);
////        return Ok(ApiResponse<object>.Ok(new { message = "Deposit request submitted. Admin will verify within 24 hours." }));
////    }

////    // TASK 2: Withdrawal — Females only, max 70%
////    [HttpGet("withdrawal-limit")]
////    public async Task<IActionResult> WithdrawalLimit()
////    {
////        var user = await _db.Users.FindAsync(Me);
////        if (user?.Gender?.ToLower() != "female")
////            return BadRequest(ApiResponse<object>.Fail("Withdrawal is available for Female users only."));

////        var balance = user.CoinBalance;
////        var maxWithdraw = (int)(balance * MingleyDbContext.FemaleWithdrawPct);
////        var remainingAfter = balance - maxWithdraw;

////        return Ok(ApiResponse<object>.Ok(new
////        {
////            currentBalance = balance,
////            maxWithdrawalCoins = maxWithdraw,
////            remainingInWallet = remainingAfter,
////            withdrawalPct = (int)(MingleyDbContext.FemaleWithdrawPct * 100),
////            inrEquivalent = maxWithdraw * MingleyDbContext.CoinToInrRate,
////            note = $"You can withdraw up to {(int)(MingleyDbContext.FemaleWithdrawPct * 100)}% of your balance. " +
////                   $"30% stays in your wallet.",
////        }));
////    }

////    [HttpPost("withdraw")]
////    public async Task<IActionResult> Withdraw([FromBody] WithdrawalRequestDto req)
////    {
////        await _wallet.SubmitWithdrawalAsync(Me, req);
////        return Ok(ApiResponse<object>.Ok(new
////        {
////            message = $"Withdrawal of {req.Coins} coins submitted. Processing in 3-5 business days.",
////            inrAmount = req.Coins * MingleyDbContext.CoinToInrRate,
////        }));
////    }
////}

////public record RazorpayOrderRequest(long Amount);
////public record RazorpayVerifyRequest(string OrderId, string PaymentId, string Signature, long Amount = 0);

//////using Microsoft.AspNetCore.Authorization;
//////using Microsoft.AspNetCore.Mvc;
//////using Mingley.Application.DTOs.Common;
//////using Mingley.Application.DTOs.Wallet;
//////using Mingley.Application.Interfaces;
//////using Mingley.Infrastructure.Services;
//////using System.Security.Cryptography;
//////using System.Text;

//////namespace Mingley.API.Controllers;

//////[ApiController]
//////[Route("v1/wallet")]
//////[Authorize]
//////public class WalletController : ControllerBase
//////{
//////    private readonly IWalletService _wallet;

//////    private readonly IConfiguration _config;

//////    private Guid Me => Guid.Parse(
//////        User.FindFirst("sub")?.Value
//////        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
//////        ?? Guid.Empty.ToString());

//////    public WalletController(IWalletService wallet, IConfiguration config)
//////    {
//////        _wallet = wallet;
//////        _config = config;
//////    }

//////    [HttpGet("balance")]
//////    public async Task<IActionResult> Balance()
//////    {
//////        var bal = await _wallet.GetBalanceAsync(Me);
//////        return Ok(ApiResponse<object>.Ok(new { coinBalance = bal.CoinBalance, totalEarned = bal.TotalEarned }));
//////    }

//////    [HttpGet("packages")]
//////    public async Task<IActionResult> Packages()
//////    {
//////        var pkgs = await _wallet.GetPackagesAsync();
//////        return Ok(ApiResponse<object>.Ok(new { packages = pkgs }));
//////    }

//////    [HttpGet("transactions")]
//////    public async Task<IActionResult> Transactions([FromQuery] string type = "all")
//////    {
//////        var txns = await _wallet.GetTransactionsAsync(Me, type);
//////        return Ok(ApiResponse<object>.Ok(new { transactions = txns }));
//////    }

//////    [HttpPost("razorpay/order")]
//////    public IActionResult RazorpayOrder([FromBody] RazorpayOrderRequest req)
//////    {
//////        var orderId = $"order_{Guid.NewGuid():N}";
//////        return Ok(ApiResponse<object>.Ok(new
//////        {
//////            orderId,
//////            amount = req.Amount,
//////            currency = "INR",
//////            key = _config["Razorpay:KeyId"] ?? "rzp_test_Sq4maCpZVgCTeM"
//////        }));
//////    }

//////    [HttpPost("razorpay/verify")]
//////    public async Task<IActionResult> RazorpayVerify([FromBody] RazorpayVerifyRequest req)
//////    {
//////        var keySecret = _config["Razorpay:KeySecret"] ?? "";
//////        if (!string.IsNullOrEmpty(keySecret) && !string.IsNullOrEmpty(req.Signature) && !req.PaymentId.StartsWith("test_"))
//////        {
//////            var payload = $"{req.OrderId}|{req.PaymentId}";
//////            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(keySecret));
//////            var hash = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).Replace("-", "").ToLower();
//////            if (hash != req.Signature)
//////                return BadRequest(ApiResponse<object>.Fail("Invalid payment signature"));
//////        }

//////        var coins = (int)(req.Amount / 50);
//////        if (coins > 0)
//////            await _wallet.AddCoinsAsync(Me, coins, $"Razorpay payment {req.PaymentId}", "razorpay", req.OrderId);

//////        return Ok(ApiResponse<object>.Ok(new { success = true, coinsAdded = coins }));
//////    }

//////    [HttpPost("deposit")]
//////    public async Task<IActionResult> Deposit([FromBody] DepositRequestDto req)
//////    {
//////        await _wallet.SubmitDepositAsync(Me, req);
//////        return Ok(ApiResponse<object>.Ok(new { message = "Deposit request submitted" }));
//////    }

//////    [HttpPost("withdraw")]
//////    public async Task<IActionResult> Withdraw([FromBody] WithdrawalRequestDto req)
//////    {
//////        await _wallet.SubmitWithdrawalAsync(Me, req);
//////        return Ok(ApiResponse<object>.Ok(new { message = "Withdrawal request submitted" }));
//////    }
//////}

//////public record RazorpayOrderRequest(long Amount);
//////public record RazorpayVerifyRequest(string OrderId, string PaymentId, string Signature, long Amount = 0);