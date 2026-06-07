using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Mingley.Application.DTOs.Auth;
using Mingley.Application.Interfaces;
using Mingley.Domain.Entities;
using Mingley.Infrastructure.Persistence;

namespace Mingley.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly MingleyDbContext _db;
    private readonly ITokenService _tokens;
    private readonly IConfiguration _cfg;
    private readonly INotificationService _notifs;
    private readonly IMemoryCache _cache;

    public AuthService(MingleyDbContext db, ITokenService tokens, IConfiguration cfg, INotificationService notifs, IMemoryCache cache)
    { _db = db; _tokens = tokens; _cfg = cfg; _notifs = notifs; _cache = cache; }

    //public async Task<RegisterResponse> RegisterAsync(RegisterRequest req)
    //{
    //    if (string.IsNullOrWhiteSpace(req.Email) && string.IsNullOrWhiteSpace(req.Phone))
    //        throw new InvalidOperationException("Email or phone is required.");

    //    if (!string.IsNullOrWhiteSpace(req.ConfirmPassword) && req.Password != req.ConfirmPassword)
    //        throw new InvalidOperationException("Passwords do not match.");

    //    if (req.Email != null && await _db.Users.IgnoreQueryFilters()
    //        .AnyAsync(u => u.Email == req.Email.ToLower().Trim()))
    //        throw new InvalidOperationException("Email already registered.");

    //    //if (req.Phone != null && await _db.Users.IgnoreQueryFilters()
    //    //    .AnyAsync(u => u.Phone == req.Phone.Trim()))
    //    //    throw new InvalidOperationException("Phone already registered.");
    //    // NEW — only check phone if it's actually provided
    //    var cleanPhone = string.IsNullOrWhiteSpace(req.Phone) ? null : req.Phone.Trim();

    //    if (cleanPhone != null && await _db.Users.IgnoreQueryFilters()
    //        .AnyAsync(u => u.Phone == cleanPhone))
    //        throw new InvalidOperationException("Phone already registered.");

    //    var otp = GenerateOtp();
    //    var user = new User
    //    {
    //        Email = req.Email?.ToLower().Trim(),
    //        Phone = req.Phone?.Trim(),
    //        PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
    //        FullName = req.FullName,
    //        Gender = req.Gender,
    //        DateOfBirth = req.DateOfBirth?.ToUniversalTime(),
    //        Avatar = req.Avatar,
    //        OtpCode = otp,
    //        OtpExpiry = DateTime.UtcNow.AddMinutes(10),
    //        OtpPurpose = "registration",
    //        CoinBalance = MingleyDbContext.WelcomeBonus,
    //    };
    //    _db.Users.Add(user);
    //    _db.UserPreferences.Add(new UserPreference { UserId = user.Id });
    //    await _db.SaveChangesAsync();

    //    // NEW: Save interests selected at registration
    //    if (req.Interests.Any())
    //    {
    //        var interestNames = req.Interests.Select(i => i.ToLower().Trim()).ToList();
    //        var matchedInterests = await _db.Interests
    //            .Where(i => !i.IsDeleted && interestNames.Contains(i.Name.ToLower()))
    //            .ToListAsync();
    //        foreach (var interest in matchedInterests)
    //            _db.UserInterests.Add(new UserInterest { UserId = user.Id, InterestId = interest.Id });
    //        if (matchedInterests.Any()) await _db.SaveChangesAsync();
    //    }

    //    _db.CoinTransactions.Add(new CoinTransaction
    //    {
    //        UserId = user.Id,
    //        Coins = MingleyDbContext.WelcomeBonus,
    //        Direction = "credit",
    //        Description = "Welcome bonus",
    //        TransactionType = "welcome",
    //    });
    //    await _db.SaveChangesAsync();

    //    Console.WriteLine($"\n📱 OTP [{user.Email ?? user.Phone}]: {otp}\n");
    //    return new RegisterResponse { UserId = user.Id.ToString(), DevOtp = IsDev() ? otp : null };
    //}
    public async Task<RegisterResponse> RegisterAsync(RegisterRequest req)
    {
        if (!string.IsNullOrWhiteSpace(req.ConfirmPassword) && req.Password != req.ConfirmPassword)
            throw new InvalidOperationException("Passwords do not match.");

        // Clean and validate email — must contain @ to be real
        var cleanEmail = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email.ToLower().Trim();
        if (cleanEmail != null && !cleanEmail.Contains('@'))
            cleanEmail = null; // not a real email, ignore it

        // Clean and validate phone — must be 7+ chars and start with digit or +
        var cleanPhone = string.IsNullOrWhiteSpace(req.Phone) ? null : req.Phone.Trim();
        if (cleanPhone != null && (cleanPhone.Length < 7 || (!char.IsDigit(cleanPhone[0]) && cleanPhone[0] != '+')))
            cleanPhone = null; // not a real phone number, ignore it

        // Must have at least one valid contact method
        if (cleanEmail == null && cleanPhone == null)
            throw new InvalidOperationException("A valid email address or phone number is required.");

        // Check duplicates only for real validated values
        if (cleanEmail != null && await _db.Users.IgnoreQueryFilters()
            .AnyAsync(u => u.Email == cleanEmail))
            throw new InvalidOperationException("Email already registered.");

        if (cleanPhone != null && await _db.Users.IgnoreQueryFilters()
            .AnyAsync(u => u.Phone == cleanPhone))
            throw new InvalidOperationException("Phone number already registered.");

        var otp = GenerateOtp();
        var user = new User
        {
            Email = cleanEmail,   // use validated value
            Phone = cleanPhone,   // use validated value
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password, workFactor: 10),
            FullName = req.FullName,
            Gender = req.Gender,
            DateOfBirth = req.DateOfBirth?.ToUniversalTime(),
            Avatar = req.Avatar,
            OtpCode = otp,
            OtpExpiry = DateTime.UtcNow.AddMinutes(10),
            OtpPurpose = "registration",
            CoinBalance = MingleyDbContext.WelcomeBonus,
        };
        _db.Users.Add(user);
        _db.UserPreferences.Add(new UserPreference { UserId = user.Id });
        await _db.SaveChangesAsync();

        // Save interests selected at registration
        if (req.Interests.Any())
        {
            var interestNames = req.Interests.Select(i => i.ToLower().Trim()).ToList();
            var matchedInterests = await _db.Interests
                .Where(i => !i.IsDeleted && interestNames.Contains(i.Name.ToLower()))
                .ToListAsync();
            foreach (var interest in matchedInterests)
                _db.UserInterests.Add(new UserInterest { UserId = user.Id, InterestId = interest.Id });
            if (matchedInterests.Any()) await _db.SaveChangesAsync();
        }

        _db.CoinTransactions.Add(new CoinTransaction
        {
            UserId = user.Id,
            Coins = MingleyDbContext.WelcomeBonus,
            Direction = "credit",
            Description = "Welcome bonus",
            TransactionType = "welcome",
        });
        await _db.SaveChangesAsync();

        Console.WriteLine($"\n📱 OTP [{user.Email ?? user.Phone}]: {otp}\n");
        return new RegisterResponse { UserId = user.Id.ToString(), DevOtp = IsDev() ? otp : null };
    }

    public async Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest req)
    {
        if (!Guid.TryParse(req.UserId, out var uid))
            throw new InvalidOperationException("Invalid user ID.");

        var user = await _db.Users.Include(u => u.Location)
            .FirstOrDefaultAsync(u => u.Id == uid)
            ?? throw new InvalidOperationException("User not found.");

        // Brute-force guard: max 5 OTP attempts per user per 15 minutes
        var attemptKey = $"otp_attempts_{uid}";
        var attempts = _cache.Get<int>(attemptKey);
        if (attempts >= 5)
            throw new InvalidOperationException("Too many incorrect attempts. Please wait 15 minutes and request a new OTP.");

        if (user.OtpCode != req.Otp)
        {
            _cache.Set(attemptKey, attempts + 1, TimeSpan.FromMinutes(15));
            throw new InvalidOperationException("Invalid OTP.");
        }
        _cache.Remove(attemptKey);
        if (user.OtpExpiry < DateTime.UtcNow) throw new InvalidOperationException("OTP expired. Please request a new one.");
        if (user.OtpPurpose != req.Purpose) throw new InvalidOperationException("OTP purpose mismatch.");

        user.IsVerified = true;
        user.OtpCode = null;
        user.OtpExpiry = null;
        user.OtpPurpose = null;
        user.LastActiveAt = DateTime.UtcNow;

        if (req.Purpose == "registration")
        {
            user.CoinBalance += MingleyDbContext.VerificationBonus;
            _db.CoinTransactions.Add(new CoinTransaction
            {
                UserId = user.Id,
                Coins = MingleyDbContext.VerificationBonus,
                Direction = "credit",
                Description = "Verification bonus",
                TransactionType = "verification",
            });
        }
        await _db.SaveChangesAsync();

        await _notifs.CreateAsync(user.Id, "Welcome to Mingley! 🎉",
            $"You received {MingleyDbContext.WelcomeBonus + MingleyDbContext.VerificationBonus} free coins!", "system");

        return BuildAuth(user);
    }

    public async Task ResendOtpAsync(ResendOtpRequest req)
    {
        if (!Guid.TryParse(req.UserId, out var uid))
            throw new InvalidOperationException("Invalid user ID.");

        var user = await _db.Users.FindAsync(uid) ?? throw new InvalidOperationException("User not found.");
        user.OtpCode = GenerateOtp();
        user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
        user.OtpPurpose = req.Purpose;
        await _db.SaveChangesAsync();
        Console.WriteLine($"\n📱 Resent OTP [{user.Email ?? user.Phone}]: {user.OtpCode}\n");
    }

    //public async Task<AuthResponse> LoginAsync(LoginRequest req)
    //{
    //    var id = req.Identifier.ToLower().Trim();
    //    var user = await _db.Users
    //        .Include(u => u.Location)
    //        .Include(u => u.Subscription).ThenInclude(s => s!.Plan)
    //        .FirstOrDefaultAsync(u => !u.IsDeleted && u.IsActive && (u.Email == id || u.Phone == id))
    //        ?? throw new InvalidOperationException("Invalid credentials.");

    //    if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash!))
    //        throw new InvalidOperationException("Invalid credentials.");

    //    if (user.IsSuspended)
    //        throw new InvalidOperationException($"Account suspended. Reason: {user.SuspendReason ?? "Contact support."}");

    //    if (!user.IsVerified)
    //    {
    //        user.OtpCode = GenerateOtp();
    //        user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
    //        user.OtpPurpose = "registration";
    //        await _db.SaveChangesAsync();
    //        Console.WriteLine($"\n📱 Login unverified OTP [{user.Email}]: {user.OtpCode}\n");
    //        throw new InvalidOperationException($"UNVERIFIED:{user.Id}:{(IsDev() ? user.OtpCode : "")}");
    //    }

    //    if (!string.IsNullOrWhiteSpace(req.FcmToken))
    //        user.FcmToken = req.FcmToken;

    //    user.LastActiveAt = DateTime.UtcNow;
    //    user.IsOnline = true;
    //    await _db.SaveChangesAsync();
    //    return BuildAuth(user);
    //}
    public async Task<AuthResponse> LoginAsync(LoginRequest req)
    {
        var id = req.Identifier.ToLower().Trim();
        var user = await _db.Users
            .Include(u => u.Location)
            .Include(u => u.Subscription).ThenInclude(s => s!.Plan)
            .FirstOrDefaultAsync(u => !u.IsDeleted && u.IsActive && (u.Email == id || u.Phone == id))
            ?? throw new InvalidOperationException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash!))
            throw new InvalidOperationException("Invalid credentials.");

        if (user.IsSuspended)
            throw new InvalidOperationException($"Account suspended. Reason: {user.SuspendReason ?? "Contact support."}");

        if (!user.IsVerified)
        {
            user.OtpCode = GenerateOtp();
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
            user.OtpPurpose = "registration";
            await _db.SaveChangesAsync();
            Console.WriteLine($"\n📱 Login unverified OTP [{user.Email}]: {user.OtpCode}\n");
            throw new InvalidOperationException($"UNVERIFIED:{user.Id}:{(IsDev() ? user.OtpCode : "")}");
        }

        if (!string.IsNullOrWhiteSpace(req.FcmToken))
            user.FcmToken = req.FcmToken;

        // FIX: Always reset online + timestamp so response has fresh state
        user.IsOnline = true;
        user.LastActiveAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // FIX: Re-fetch fresh from DB — guarantees no stale data in login response
        var freshUser = await _db.Users
            .Include(u => u.Location)
            .Include(u => u.Subscription).ThenInclude(s => s!.Plan)
            .FirstOrDefaultAsync(u => u.Id == user.Id && !u.IsDeleted)
            ?? user;

        return BuildAuth(freshUser);
    }


    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var uid = _tokens.ValidateRefreshToken(refreshToken)
            ?? throw new InvalidOperationException("Invalid or expired refresh token.");

        var user = await _db.Users.Include(u => u.Location)
            .FirstOrDefaultAsync(u => u.Id == uid && !u.IsDeleted && u.IsActive)
            ?? throw new InvalidOperationException("User not found.");

        _tokens.RevokeRefreshToken(refreshToken);
        return BuildAuth(user);
    }

    public async Task LogoutAsync(Guid userId)
    {
        var u = await _db.Users.FindAsync(userId);
        if (u == null) return;
        u.IsOnline = false; u.LastActiveAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // UPDATED: Returns userId + devOtp for auto-redirect to OTP screen
    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest req)
    {
        var id = req.Identifier.ToLower().Trim();
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Email == id || x.Phone == id);
        if (u == null)
        {
            // Don't reveal user doesn't exist — return empty userId
            return new ForgotPasswordResponse { UserId = "", Message = "If account exists, OTP has been sent." };
        }
        u.OtpCode = GenerateOtp();
        u.OtpExpiry = DateTime.UtcNow.AddMinutes(15);
        u.OtpPurpose = "forgot_password";
        await _db.SaveChangesAsync();
        Console.WriteLine($"\n📱 Forgot OTP [{u.Email}]: {u.OtpCode}\n");

        return new ForgotPasswordResponse
        {
            UserId = u.Id.ToString(),
            DevOtp = IsDev() ? u.OtpCode : null,
            Message = "OTP sent to your registered email/phone.",
        };
    }

    //public async Task ResetPasswordAsync(ResetPasswordRequest req)
    //{
    //    if (!Guid.TryParse(req.UserId, out var uid))
    //        throw new InvalidOperationException("Invalid user ID.");

    //    var u = await _db.Users.FindAsync(uid) ?? throw new InvalidOperationException("User not found.");
    //    if (u.OtpCode != req.Otp || u.OtpPurpose != "forgot_password")
    //        throw new InvalidOperationException("Invalid OTP.");
    //    if (u.OtpExpiry < DateTime.UtcNow)
    //        throw new InvalidOperationException("OTP expired.");

    //    u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
    //    u.OtpCode = null; u.OtpExpiry = null; u.OtpPurpose = null;
    //    await _db.SaveChangesAsync();
    //}
    public async Task ResetPasswordAsync(ResetPasswordRequest req)
    {
        var id = req.Identifier.ToLower().Trim();
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Email == id || x.Phone == id)
            ?? throw new InvalidOperationException("No account found with that email or phone.");

        if (u.OtpCode != req.Otp || u.OtpPurpose != "forgot_password")
            throw new InvalidOperationException("Invalid OTP.");
        if (u.OtpExpiry < DateTime.UtcNow)
            throw new InvalidOperationException("OTP expired. Request a new one.");

        u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        u.OtpCode = null; u.OtpExpiry = null; u.OtpPurpose = null;
        await _db.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest req)
    {
        var u = await _db.Users.FindAsync(userId) ?? throw new InvalidOperationException("User not found.");
        if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, u.PasswordHash!))
            throw new InvalidOperationException("Current password is incorrect.");
        u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        u.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    //private AuthResponse BuildAuth(User u)
    //{
    //    var at = _tokens.GenerateAccessToken(u);
    //    var rt = _tokens.GenerateRefreshToken();
    //    _tokens.StoreRefreshToken(u.Id, rt);
    //    return new AuthResponse
    //    {
    //        UserId = u.Id.ToString(),
    //        AccessToken = at,
    //        RefreshToken = rt,
    //        User = new UserDto
    //        {
    //            Id = u.Id.ToString(),
    //            FullName = u.FullName,
    //            Email = u.Email,
    //            Phone = u.Phone,
    //            Gender = u.Gender,
    //            Avatar = u.Avatar,
    //            IsPremium = u.IsPremium,
    //            IsVerified = u.IsVerified,
    //            IsOnline = u.IsOnline,
    //            CoinBalance = u.CoinBalance,
    //            Role = u.Role,
    //            TwoFactorEnabled = u.TwoFactorEnabled,
    //            ProfileComplete = u.ProfileComplete,
    //            LastActiveAt = u.LastActiveAt,
    //        },
    //    };
    //}
    private AuthResponse BuildAuth(User u)
    {
        var at = _tokens.GenerateAccessToken(u);
        var rt = _tokens.GenerateRefreshToken();
        _tokens.StoreRefreshToken(u.Id, rt);
        return new AuthResponse
        {
            UserId = u.Id.ToString(),
            AccessToken = at,
            RefreshToken = rt,
            User = new UserDto
            {
                Id = u.Id.ToString(),
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                Gender = u.Gender,
                Avatar = u.Avatar,
                CoverPhoto = u.CoverPhoto,
                Bio = u.Bio,
                Profession = u.Profession,
                DateOfBirth = u.DateOfBirth,
                Age = u.DateOfBirth.HasValue ? (int?)((DateTime.UtcNow - u.DateOfBirth.Value).Days / 365) : null,
                IsPremium = u.IsPremium,
                IsVerified = u.IsVerified,
                IsOnline = u.IsOnline,
                CoinBalance = u.CoinBalance,
                Role = u.Role,
                TwoFactorEnabled = u.TwoFactorEnabled,
                ProfileComplete = u.ProfileComplete,
                IsTrending = u.IsTrending,
                LastActiveAt = u.LastActiveAt,
            },
        };
    }

    private bool IsDev() => _cfg["App:Environment"] != "Production";
    private static string GenerateOtp() => Random.Shared.Next(100000, 999999).ToString();
}