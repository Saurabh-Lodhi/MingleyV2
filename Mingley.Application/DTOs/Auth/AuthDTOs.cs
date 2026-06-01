using System.ComponentModel.DataAnnotations;

namespace Mingley.Application.DTOs.Auth;

public class AuthResponse
{
    public string? UserId { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; } = 86400;
    public UserDto? User { get; set; }
    public bool? Requires2FA { get; set; }
}

public class RegisterResponse
{
    public string? UserId { get; set; }
    public string? DevOtp { get; set; }
}

public class UserDto
{
    public string? Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public string? Avatar { get; set; }
    public bool IsPremium { get; set; }
    public bool IsVerified { get; set; }
    public bool IsOnline { get; set; }
    public int CoinBalance { get; set; }
    public string? Role { get; set; }
    public bool ProfileComplete { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTime? LastActiveAt { get; set; }
    public string? Profession { get; set; }
}

public class RegisterRequest
{
    [EmailAddress, MaxLength(200)]
    public string? Email { get; set; }

    [Phone, MaxLength(15)]
    public string? Phone { get; set; }

    [Required, MinLength(8), MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [Compare(nameof(Password))]
    public string? ConfirmPassword { get; set; }

    [Required, MaxLength(100)]
    public string? FullName { get; set; }

    [Required, RegularExpression("^(male|female|other)$", ErrorMessage = "Gender must be male, female, or other")]
    public string? Gender { get; set; }

    [Required]
    public DateTime? DateOfBirth { get; set; }

    [MaxLength(500)]
    public string? Avatar { get; set; }

    public List<string> Interests { get; set; } = new();

    [MaxLength(100)]
    public string? Profession { get; set; }
}

public class LoginRequest
{
    [Required, MaxLength(200)]
    public string Identifier { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? TwoFactorCode { get; set; }

    [MaxLength(500)]
    public string? FcmToken { get; set; }
}

public class VerifyOtpRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public string Purpose { get; set; } = "registration";
}

public class ResendOtpRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Purpose { get; set; } = "registration";
}

public class ForgotPasswordRequest
{
    public string Identifier { get; set; } = string.Empty;
}

public class ForgotPasswordResponse
{
    public string? UserId { get; set; }
    public string? DevOtp { get; set; }
    public string Message { get; set; } = "OTP sent to your registered email/phone.";
}

public class ResetPasswordRequest
{
    public string Identifier { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}