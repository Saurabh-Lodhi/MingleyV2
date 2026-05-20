using Mingley.Application.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mingley.Application.DTOs.Auth;
using Mingley.Application.DTOs.Common;
using Mingley.Application.Interfaces;
using System.Security.Claims;

namespace Mingley.API.Controllers;

[ApiController]
[Route("v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly IWebHostEnvironment _env;
    public AuthController(IAuthService auth, IWebHostEnvironment env)
    {
        _auth = auth;
        _env = env;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var result = await _auth.RegisterAsync(req);
        return StatusCode(201, ApiResponse<RegisterResponse>.Created(result, "Registration successful. OTP sent."));
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest req)
    {
        var result = await _auth.VerifyOtpAsync(req);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Verified successfully."));
    }

    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest req)
    {
        await _auth.ResendOtpAsync(req);
        return Ok(ApiResponse.Ok("OTP resent."));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        try
        {
            var result = await _auth.LoginAsync(req);
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful."));
        }
        catch (InvalidOperationException ex) when (ex.Message.StartsWith("UNVERIFIED:"))
        {
            var parts = ex.Message.Split(':');
            var userId = parts.Length > 1 ? parts[1] : "";
            var devOtp = parts.Length > 2 ? parts[2] : null;

            return Ok(ApiResponse<object>.Ok(new
            {
                requiresVerification = true,
                userId,
                //devOtp,

            }, "Account not verified. Please verify OTP."));
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest req)
    {
        var result = await _auth.RefreshTokenAsync(req.RefreshToken);
        return Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _auth.LogoutAsync(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));
        return Ok(ApiResponse.Ok("Logged out."));
    }

    // UPDATED: Now returns userId + devOtp for auto-redirect to OTP screen
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        var result = await _auth.ForgotPasswordAsync(req);
        return Ok(ApiResponse<ForgotPasswordResponse>.Ok(result, result.Message));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        await _auth.ResetPasswordAsync(req);
        return Ok(ApiResponse.Ok("Password reset successfully."));
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
    {
        await _auth.ChangePasswordAsync(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!), req);
        return Ok(ApiResponse.Ok("Password changed successfully."));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me([FromServices] IUserService users)
    {
        var profile = await users.GetMeAsync(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));
        return profile == null
            ? NotFound(ApiResponse<object>.Fail("User not found.", 404))
            : Ok(ApiResponse<object>.Ok(profile));
    }
}

//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Mingley.Application.DTOs.Auth;
//using Mingley.Application.DTOs.Common;
//using Mingley.Application.Interfaces;
//using System.Security.Claims;

//namespace Mingley.API.Controllers;

//[ApiController]
//[Route("v1/auth")]
//[Produces("application/json")]
//public class AuthController : ControllerBase
//{
//    private readonly IAuthService _auth;
//    public AuthController(IAuthService auth) => _auth = auth;

//    [HttpPost("register")]
//    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
//    {
//        var result = await _auth.RegisterAsync(req);
//        return StatusCode(201, ApiResponse<RegisterResponse>.Created(result, "Registration successful. OTP sent."));
//    }

//    [HttpPost("verify-otp")]
//    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest req)
//    {
//        var result = await _auth.VerifyOtpAsync(req);
//        return Ok(ApiResponse<AuthResponse>.Ok(result, "Verified successfully."));
//    }

//    [HttpPost("resend-otp")]
//    public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest req)
//    {
//        await _auth.ResendOtpAsync(req);
//        return Ok(ApiResponse.Ok("OTP resent."));
//    }

//    [HttpPost("login")]
//    public async Task<IActionResult> Login([FromBody] LoginRequest req)
//    {
//        try
//        {
//            var result = await _auth.LoginAsync(req);
//            return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful."));
//        }
//        catch (InvalidOperationException ex) when (ex.Message.StartsWith("UNVERIFIED:"))
//        {
//            // Return a structured 200 response so frontend can navigate to OTP screen
//            var parts  = ex.Message.Split(':');
//            var userId = parts.Length > 1 ? parts[1] : "";
//            var devOtp = parts.Length > 2 ? parts[2] : null;
//            return Ok(ApiResponse<object>.Ok(new
//            {
//                requiresVerification = true,
//                userId,
//                devOtp,
//            }, "Account not verified. Please verify OTP."));
//        }
//    }

//    [HttpPost("refresh")]
//    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest req)
//    {
//        var result = await _auth.RefreshTokenAsync(req.RefreshToken);
//        return Ok(ApiResponse<AuthResponse>.Ok(result));
//    }

//    [HttpPost("logout")]
//    [Authorize]
//    public async Task<IActionResult> Logout()
//    {
//        await _auth.LogoutAsync(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));
//        return Ok(ApiResponse.Ok("Logged out."));
//    }

//    [HttpPost("forgot-password")]
//    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
//    {
//        await _auth.ForgotPasswordAsync(req);
//        return Ok(ApiResponse.Ok("If account exists, OTP has been sent."));
//    }

//    [HttpPost("reset-password")]
//    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
//    {
//        await _auth.ResetPasswordAsync(req);
//        return Ok(ApiResponse.Ok("Password reset successfully."));
//    }

//    [HttpPost("change-password")]
//    [Authorize]
//    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
//    {
//        await _auth.ChangePasswordAsync(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!), req);
//        return Ok(ApiResponse.Ok("Password changed successfully."));
//    }

//    [HttpGet("me")]
//    [Authorize]
//    public async Task<IActionResult> Me([FromServices] IUserService users)
//    {
//        var profile = await users.GetMeAsync(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));
//        return profile == null
//            ? NotFound(ApiResponse<object>.Fail("User not found.", 404))
//            : Ok(ApiResponse<object>.Ok(profile));
//    }
//}
