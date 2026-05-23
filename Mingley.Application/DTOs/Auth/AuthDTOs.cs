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
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Password { get; set; } = string.Empty;
    public string? ConfirmPassword { get; set; }
    public string? FullName { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Avatar { get; set; }
    // NEW: interests selected during registration
    public List<string> Interests { get; set; } = new();
    public string? Profession { get; set; }
}

public class LoginRequest
{
    public string Identifier { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? TwoFactorCode { get; set; }
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

// NEW: Returns userId + devOtp so frontend auto-navigates to OTP screen
public class ForgotPasswordResponse
{
    public string? UserId { get; set; }
    public string? DevOtp { get; set; }   // only populated in dev/staging
    public string Message { get; set; } = "OTP sent to your registered email/phone.";
}

public class ResetPasswordRequest
{
    public string Identifier { get; set; } = string.Empty; // email or phone — NO userId needed
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


//namespace Mingley.Application.DTOs.Auth;

//public class AuthResponse
//{
//    public string? UserId { get; set; }
//    public string? AccessToken { get; set; }
//    public string? RefreshToken { get; set; }
//    public string TokenType { get; set; } = "Bearer";
//    public int ExpiresIn { get; set; } = 86400;
//    public UserDto? User { get; set; }
//    public bool? Requires2FA { get; set; }
//}

//public class RegisterResponse
//{
//    public string? UserId { get; set; }
//    public string? DevOtp { get; set; }
//}

//public class UserDto
//{
//    public string? Id { get; set; }
//    public string? FullName { get; set; }
//    public string? Email { get; set; }
//    public string? Phone { get; set; }
//    public string? Gender { get; set; }
//    public string? Avatar { get; set; }
//    public bool IsPremium { get; set; }
//    public bool IsVerified { get; set; }
//    public bool IsOnline { get; set; }
//    public int CoinBalance { get; set; }
//    public string? Role { get; set; }
//    public bool ProfileComplete { get; set; }
//    public bool TwoFactorEnabled { get; set; }
//    public DateTime? LastActiveAt { get; set; }
//}

//public class RegisterRequest
//{
//    public string? Email { get; set; }
//    public string? Phone { get; set; }
//    public string Password { get; set; } = string.Empty;
//    public string? ConfirmPassword { get; set; }
//    public string? FullName { get; set; }
//    public string? Gender { get; set; }
//    public DateTime? DateOfBirth { get; set; }
//    public string? Avatar { get; set; }   // NEW: optional profile picture URL
//}

//public class LoginRequest
//{
//    public string Identifier { get; set; } = string.Empty;
//    public string Password { get; set; } = string.Empty;
//    public string? TwoFactorCode { get; set; }
//    public string? FcmToken { get; set; }
//}

//public class VerifyOtpRequest
//{
//    public string UserId { get; set; } = string.Empty;
//    public string Otp { get; set; } = string.Empty;
//    public string Purpose { get; set; } = "registration";
//}

//public class ResendOtpRequest
//{
//    public string UserId { get; set; } = string.Empty;
//    public string Purpose { get; set; } = "registration";
//}

//public class ForgotPasswordRequest
//{
//    public string Identifier { get; set; } = string.Empty;
//}

//public class ResetPasswordRequest
//{
//    public string UserId { get; set; } = string.Empty;
//    public string Otp { get; set; } = string.Empty;
//    public string NewPassword { get; set; } = string.Empty;
//}

//public class ChangePasswordRequest
//{
//    public string CurrentPassword { get; set; } = string.Empty;
//    public string NewPassword { get; set; } = string.Empty;
//}

//public class RefreshTokenRequest
//{
//    public string RefreshToken { get; set; } = string.Empty;
//}

////namespace Mingley.Application.DTOs.Auth;

////public class AuthResponse
////{
////    public string? UserId { get; set; }
////    public string? AccessToken { get; set; }
////    public string? RefreshToken { get; set; }
////    public string TokenType { get; set; } = "Bearer";
////    public int ExpiresIn { get; set; } = 86400; // 24h
////    public UserDto? User { get; set; }
////    public bool? Requires2FA { get; set; }
////}

////public class RegisterResponse
////{
////    public string? UserId { get; set; }
////    public string? DevOtp { get; set; } // Dev only
////}

////public class UserDto
////{
////    public string? Id { get; set; }
////    public string? FullName { get; set; }
////    public string? Email { get; set; }
////    public string? Phone { get; set; }
////    public string? Gender { get; set; }
////    public string? Avatar { get; set; }
////    public bool IsPremium { get; set; }
////    public bool IsVerified { get; set; }
////    public bool IsOnline { get; set; }
////    public int CoinBalance { get; set; }
////    public string? Role { get; set; }
////    public bool ProfileComplete { get; set; }
////    public bool TwoFactorEnabled { get; set; }
////    public DateTime? LastActiveAt { get; set; }
////}

////public class RegisterRequest
////{
////    public string? Email { get; set; }
////    public string? Phone { get; set; }
////    public string Password { get; set; } = string.Empty;
////    public string? ConfirmPassword { get; set; }
////    public string? FullName { get; set; }
////    public string? Gender { get; set; }
////    public DateTime? DateOfBirth { get; set; }
////}

////public class LoginRequest
////{
////    public string Identifier { get; set; } = string.Empty; // email or phone
////    public string Password { get; set; } = string.Empty;
////    public string? TwoFactorCode { get; set; }
////    public string? FcmToken { get; set; }
////}

////public class VerifyOtpRequest
////{
////    public string UserId { get; set; } = string.Empty;
////    public string Otp { get; set; } = string.Empty;
////    public string Purpose { get; set; } = "registration";
////}

////public class ResendOtpRequest
////{
////    public string UserId { get; set; } = string.Empty;
////    public string Purpose { get; set; } = "registration";
////}

////public class ForgotPasswordRequest
////{
////    public string Identifier { get; set; } = string.Empty;
////}

////public class ResetPasswordRequest
////{
////    public string UserId { get; set; } = string.Empty;
////    public string Otp { get; set; } = string.Empty;
////    public string NewPassword { get; set; } = string.Empty;
////}

////public class ChangePasswordRequest
////{
////    public string CurrentPassword { get; set; } = string.Empty;
////    public string NewPassword { get; set; } = string.Empty;
////}

////public class RefreshTokenRequest
////{
////    public string RefreshToken { get; set; } = string.Empty;
////}

//namespace Mingley.Application.DTOs.Auth;

//public class AuthResponse
//{
//    public string? UserId { get; set; }
//    public string? AccessToken { get; set; }
//    public string? RefreshToken { get; set; }
//    public string TokenType { get; set; } = "Bearer";
//    public int ExpiresIn { get; set; } = 86400;
//    public UserDto? User { get; set; }
//    public bool? Requires2FA { get; set; }
//}

//public class RegisterResponse
//{
//    public string? UserId { get; set; }
//    public string? DevOtp { get; set; }
//}

//public class UserDto
//{
//    public string? Id { get; set; }
//    public string? FullName { get; set; }
//    public string? Email { get; set; }
//    public string? Phone { get; set; }
//    public string? Gender { get; set; }
//    public string? Avatar { get; set; }
//    public bool IsPremium { get; set; }
//    public bool IsVerified { get; set; }
//    public bool IsOnline { get; set; }
//    public int CoinBalance { get; set; }
//    public string? Role { get; set; }
//    public bool ProfileComplete { get; set; }
//    public bool TwoFactorEnabled { get; set; }
//    public DateTime? LastActiveAt { get; set; }
//}

//public class RegisterRequest
//{
//    public string? Email { get; set; }
//    public string? Phone { get; set; }
//    public string Password { get; set; } = string.Empty;
//    public string? ConfirmPassword { get; set; }
//    public string? FullName { get; set; }
//    public string? Gender { get; set; }
//    public DateTime? DateOfBirth { get; set; }
//    public string? Avatar { get; set; }
//    // NEW: interests selected during registration
//    public List<string> Interests { get; set; } = new();
//}

//public class LoginRequest
//{
//    public string Identifier { get; set; } = string.Empty;
//    public string Password { get; set; } = string.Empty;
//    public string? TwoFactorCode { get; set; }
//    public string? FcmToken { get; set; }
//}

//public class VerifyOtpRequest
//{
//    public string UserId { get; set; } = string.Empty;
//    public string Otp { get; set; } = string.Empty;
//    public string Purpose { get; set; } = "registration";
//}

//public class ResendOtpRequest
//{
//    public string UserId { get; set; } = string.Empty;
//    public string Purpose { get; set; } = "registration";
//}

//public class ForgotPasswordRequest
//{
//    public string Identifier { get; set; } = string.Empty;
//}

//// NEW: Returns userId + devOtp so frontend auto-navigates to OTP screen
//public class ForgotPasswordResponse
//{
//    public string? UserId { get; set; }
//    public string? DevOtp { get; set; }   // only populated in dev/staging
//    public string Message { get; set; } = "OTP sent to your registered email/phone.";
//}

//public class ResetPasswordRequest
//{
//    public string Identifier { get; set; } = string.Empty; // email or phone — NO userId needed
//    public string Otp { get; set; } = string.Empty;
//    public string NewPassword { get; set; } = string.Empty;
//}

//public class ChangePasswordRequest
//{
//    public string CurrentPassword { get; set; } = string.Empty;
//    public string NewPassword { get; set; } = string.Empty;
//}

//public class RefreshTokenRequest
//{
//    public string RefreshToken { get; set; } = string.Empty;
//}


////namespace Mingley.Application.DTOs.Auth;

////public class AuthResponse
////{
////    public string? UserId { get; set; }
////    public string? AccessToken { get; set; }
////    public string? RefreshToken { get; set; }
////    public string TokenType { get; set; } = "Bearer";
////    public int ExpiresIn { get; set; } = 86400;
////    public UserDto? User { get; set; }
////    public bool? Requires2FA { get; set; }
////}

////public class RegisterResponse
////{
////    public string? UserId { get; set; }
////    public string? DevOtp { get; set; }
////}

////public class UserDto
////{
////    public string? Id { get; set; }
////    public string? FullName { get; set; }
////    public string? Email { get; set; }
////    public string? Phone { get; set; }
////    public string? Gender { get; set; }
////    public string? Avatar { get; set; }
////    public bool IsPremium { get; set; }
////    public bool IsVerified { get; set; }
////    public bool IsOnline { get; set; }
////    public int CoinBalance { get; set; }
////    public string? Role { get; set; }
////    public bool ProfileComplete { get; set; }
////    public bool TwoFactorEnabled { get; set; }
////    public DateTime? LastActiveAt { get; set; }
////}

////public class RegisterRequest
////{
////    public string? Email { get; set; }
////    public string? Phone { get; set; }
////    public string Password { get; set; } = string.Empty;
////    public string? ConfirmPassword { get; set; }
////    public string? FullName { get; set; }
////    public string? Gender { get; set; }
////    public DateTime? DateOfBirth { get; set; }
////    public string? Avatar { get; set; }   // NEW: optional profile picture URL
////}

////public class LoginRequest
////{
////    public string Identifier { get; set; } = string.Empty;
////    public string Password { get; set; } = string.Empty;
////    public string? TwoFactorCode { get; set; }
////    public string? FcmToken { get; set; }
////}

////public class VerifyOtpRequest
////{
////    public string UserId { get; set; } = string.Empty;
////    public string Otp { get; set; } = string.Empty;
////    public string Purpose { get; set; } = "registration";
////}

////public class ResendOtpRequest
////{
////    public string UserId { get; set; } = string.Empty;
////    public string Purpose { get; set; } = "registration";
////}

////public class ForgotPasswordRequest
////{
////    public string Identifier { get; set; } = string.Empty;
////}

////public class ResetPasswordRequest
////{
////    public string UserId { get; set; } = string.Empty;
////    public string Otp { get; set; } = string.Empty;
////    public string NewPassword { get; set; } = string.Empty;
////}

////public class ChangePasswordRequest
////{
////    public string CurrentPassword { get; set; } = string.Empty;
////    public string NewPassword { get; set; } = string.Empty;
////}

////public class RefreshTokenRequest
////{
////    public string RefreshToken { get; set; } = string.Empty;
////}

//////namespace Mingley.Application.DTOs.Auth;

//////public class AuthResponse
//////{
//////    public string? UserId { get; set; }
//////    public string? AccessToken { get; set; }
//////    public string? RefreshToken { get; set; }
//////    public string TokenType { get; set; } = "Bearer";
//////    public int ExpiresIn { get; set; } = 86400; // 24h
//////    public UserDto? User { get; set; }
//////    public bool? Requires2FA { get; set; }
//////}

//////public class RegisterResponse
//////{
//////    public string? UserId { get; set; }
//////    public string? DevOtp { get; set; } // Dev only
//////}

//////public class UserDto
//////{
//////    public string? Id { get; set; }
//////    public string? FullName { get; set; }
//////    public string? Email { get; set; }
//////    public string? Phone { get; set; }
//////    public string? Gender { get; set; }
//////    public string? Avatar { get; set; }
//////    public bool IsPremium { get; set; }
//////    public bool IsVerified { get; set; }
//////    public bool IsOnline { get; set; }
//////    public int CoinBalance { get; set; }
//////    public string? Role { get; set; }
//////    public bool ProfileComplete { get; set; }
//////    public bool TwoFactorEnabled { get; set; }
//////    public DateTime? LastActiveAt { get; set; }
//////}

//////public class RegisterRequest
//////{
//////    public string? Email { get; set; }
//////    public string? Phone { get; set; }
//////    public string Password { get; set; } = string.Empty;
//////    public string? ConfirmPassword { get; set; }
//////    public string? FullName { get; set; }
//////    public string? Gender { get; set; }
//////    public DateTime? DateOfBirth { get; set; }
//////}

//////public class LoginRequest
//////{
//////    public string Identifier { get; set; } = string.Empty; // email or phone
//////    public string Password { get; set; } = string.Empty;
//////    public string? TwoFactorCode { get; set; }
//////    public string? FcmToken { get; set; }
//////}

//////public class VerifyOtpRequest
//////{
//////    public string UserId { get; set; } = string.Empty;
//////    public string Otp { get; set; } = string.Empty;
//////    public string Purpose { get; set; } = "registration";
//////}

//////public class ResendOtpRequest
//////{
//////    public string UserId { get; set; } = string.Empty;
//////    public string Purpose { get; set; } = "registration";
//////}

//////public class ForgotPasswordRequest
//////{
//////    public string Identifier { get; set; } = string.Empty;
//////}

//////public class ResetPasswordRequest
//////{
//////    public string UserId { get; set; } = string.Empty;
//////    public string Otp { get; set; } = string.Empty;
//////    public string NewPassword { get; set; } = string.Empty;
//////}

//////public class ChangePasswordRequest
//////{
//////    public string CurrentPassword { get; set; } = string.Empty;
//////    public string NewPassword { get; set; } = string.Empty;
//////}

//////public class RefreshTokenRequest
//////{
//////    public string RefreshToken { get; set; } = string.Empty;
//////}
