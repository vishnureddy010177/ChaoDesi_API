using ChaoDesi.Application.Features.Auth.Requests;
using ChaoDesi.Application.Features.Auth.Responses;
using ChaoDesi.Application.Interfaces;
using ChaoDesi.Domain.Entities;
using ChaoDesi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ChaoDesi.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;

    public AuthService(
    AppDbContext dbContext,
    IPasswordService passwordService,
    IJwtTokenService jwtTokenService,
    IEmailService emailService)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
    }


    public async Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var otp = await _dbContext.OtpVerifications
            .Where(x => x.LoginId == request.LoginId
                     && x.Purpose == request.Purpose
                     && !x.IsVerified)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        if (otp == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "OTP not found."
            };
        }

        if (otp.ExpiresAt < DateTime.UtcNow)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "OTP expired."
            };
        }

        if (otp.OtpCode != request.OtpCode)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid OTP."
            };
        }

        otp.IsVerified = true;
        otp.VerifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return new AuthResponse
        {
            Success = true,
            Message = "OTP verified successfully."
        };
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var loginId = !string.IsNullOrWhiteSpace(request.Email)
            ? request.Email!
            : request.MobileNumber!;

        var otp = await _dbContext.OtpVerifications
            .Where(x => x.LoginId == loginId
                     && x.Purpose == "Register"
                     && x.IsVerified)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        if (otp == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Please verify OTP first."
            };
        }

        var emailExists = !string.IsNullOrWhiteSpace(request.Email) &&
                          await _dbContext.Users.AnyAsync(x => x.Email == request.Email && !x.IsDeleted);

        if (emailExists)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Email already exists."
            };
        }

        var mobileExists = !string.IsNullOrWhiteSpace(request.MobileNumber) &&
                           await _dbContext.Users.AnyAsync(x => x.MobileNumber == request.MobileNumber && !x.IsDeleted);

        if (mobileExists)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Mobile number already exists."
            };
        }

        var customerUserTypeId = await _dbContext.UserTypes
            .Where(x => x.Code == "CUSTOMER" && x.IsActive)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        if (customerUserTypeId == 0)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Default customer user type not configured."
            };
        }

        var finalUserTypeId = request.UserTypeId ?? customerUserTypeId;

        var user = new User
        {
            UserTypeId = finalUserTypeId,
            CustomerCode = "TEMP",
            FullName = request.FullName,
            Email = request.Email,
            MobileNumber = request.MobileNumber,
            PasswordHash = _passwordService.HashPassword(request.Password),
            IsEmailVerified = !string.IsNullOrWhiteSpace(request.Email),
            IsMobileVerified = !string.IsNullOrWhiteSpace(request.MobileNumber),
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        user.CustomerCode = $"CHAO{user.Id:D3}";

        var profile = new UserProfile
        {
            UserId = user.Id,
            ZipCode = request.ZipCode,
            Country = request.Country,
            State = request.State,
            City = request.City,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.UserProfiles.Add(profile);
        await _dbContext.SaveChangesAsync();

        return new AuthResponse
        {
            Success = true,
            Message = "Registration successful.",
            UserId = user.Id,
            CustomerCode = user.CustomerCode
        };
    }
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var loginId = request.LoginId.Trim();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x =>
                !x.IsDeleted &&
                x.IsActive &&
                (x.Email == loginId || x.MobileNumber == loginId));

        if (user == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "User not found."
            };
        }

        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid password."
            };
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        var userType = await ResolveUserTypeCodeAsync(user.UserTypeId);
        var token = _jwtTokenService.GenerateToken(user, userType);
        var redirectUrl = IsAdminUserType(userType) ? "/admin/profile" : "/user-info";

        return new AuthResponse
        {
            Success = true,
            Message = "Login successful.",
            Token = token,
            UserId = user.Id,
            CustomerCode = user.CustomerCode,
            Email = user.Email,
            FullName = user.FullName,
            UserType = userType,
            RedirectUrl = redirectUrl
        };
    }
   

public async Task<AuthResponse> SendOtpAsync(SendOtpRequest request)
{
    if (request == null || string.IsNullOrWhiteSpace(request.LoginId))
    {
        return new AuthResponse
        {
            Success = false,
            Message = "Email or mobile number is required."
        };
    }

    var loginId = request.LoginId.Trim();
    var purpose = request.Purpose.Trim();
    var isRegisterOtp = purpose.Equals("Register", StringComparison.OrdinalIgnoreCase);
    var isForgotPasswordOtp = purpose.Equals("ForgotPassword", StringComparison.OrdinalIgnoreCase);

    if (!isRegisterOtp && !isForgotPasswordOtp)
    {
        return new AuthResponse
        {
            Success = false,
            Message = "Invalid OTP purpose."
        };
    }

    if (isRegisterOtp && !IsValidEmail(loginId))
    {
        return new AuthResponse
        {
            Success = false,
            Message = "Please enter a valid email address."
        };
    }

    var user = await _dbContext.Users.FirstOrDefaultAsync(a =>
        !a.IsDeleted &&
        (a.Email == loginId || a.MobileNumber == loginId));

    if (user != null && isRegisterOtp)
    {
        return new AuthResponse
        {
            Success = false,
            Message = "Email id already registered."
        };
    }

    if (user == null && isForgotPasswordOtp)
    {
        return new AuthResponse
        {
            Success = false,
            Message = "User not found."
        };
    }

    var recipientEmail = isForgotPasswordOtp ? user?.Email : loginId;

    if (string.IsNullOrWhiteSpace(recipientEmail) || !IsValidEmail(recipientEmail))
    {
        return new AuthResponse
        {
            Success = false,
            Message = "A valid email address is required to send OTP."
        };
    }

    var random = new Random();
    var otp = random.Next(1000, 9999).ToString();

    var entity = new OtpVerification
    {
        UserId = user?.Id,
        LoginId = loginId,
        OtpCode = otp,
        Purpose = purpose,
        IsVerified = false,
        ExpiresAt = DateTime.UtcNow.AddMinutes(10),
        CreatedAt = DateTime.UtcNow
    };

    _dbContext.OtpVerifications.Add(entity);
    await _dbContext.SaveChangesAsync();

    var subject = "Your OTP Code - ChaoDesi";
    var body = $@"
    <div style='font-family: Arial, sans-serif;'>
        <h2>ChaoDesi OTP Verification</h2>
        <p>Your OTP code is:</p>
        <h1 style='color:#2563eb;'>{otp}</h1>
        <p>This OTP will expire in 10 minutes.</p>
    </div>";

    await _emailService.SendEmailAsync(recipientEmail, subject, body);

    return new AuthResponse
    {
        Success = true,
        Message = "OTP sent successfully to your email."
    };
}

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        email = email.Trim();

        var pattern = @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$";
        return Regex.IsMatch(email, pattern);
    }
    //public async Task<AuthResponse> SendOtpAsync(SendOtpRequest request)
    //{

    //    var user = _dbContext.Users.Where(a=>a.Email == request.LoginId).FirstOrDefault();
    //    if (user != null && request.Purpose == "Register")
    //    {
    //        return new AuthResponse
    //        {
    //            Success = false,
    //            Message = "email id already registered."
    //        };
    //    }
    //    var random = new Random();
    //    var otp = random.Next(1000, 9999).ToString();

    //    var entity = new OtpVerification
    //    {
    //        LoginId = request.LoginId,
    //        OtpCode = otp,
    //        Purpose = request.Purpose,
    //        IsVerified = false,
    //        ExpiresAt = DateTime.UtcNow.AddMinutes(10),
    //        CreatedAt = DateTime.UtcNow
    //    };

    //    _dbContext.OtpVerifications.Add(entity);
    //    await _dbContext.SaveChangesAsync();

    //    var subject = "Your OTP Code - ChaoDesi";
    //    var body = $@"
    //    <div style='font-family: Arial, sans-serif;'>
    //        <h2>ChaoDesi OTP Verification</h2>
    //        <p>Your OTP code is:</p>
    //        <h1 style='color:#2563eb;'>{otp}</h1>
    //        <p>This OTP will expire in 10 minutes.</p>
    //    </div>";

    //    await _emailService.SendEmailAsync(request.LoginId, subject, body);

    //    return new AuthResponse
    //    {
    //        Success = true,
    //        Message = "OTP sent successfully to your email."
    //    };
    //}

    public async Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var loginId = request.LoginId.Trim();
        var otpCode = request.OtpCode.Trim();

        if (request.NewPassword != request.ConfirmPassword)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Passwords do not match."
            };
        }

        var otpRecord = await _dbContext.OtpVerifications
            .Where(x =>
                x.LoginId == loginId &&
                x.OtpCode == otpCode &&
                x.Purpose == "ForgotPassword" &&
                x.IsVerified &&
                x.ExpiresAt >= DateTime.UtcNow)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        if (otpRecord == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid or unverified OTP."
            };
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x =>
                !x.IsDeleted &&
                (x.Email == loginId || x.MobileNumber == loginId));

        if (user == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "User not found."
            };
        }

        user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        otpRecord.IsVerified = false;
        await _dbContext.SaveChangesAsync();

        return new AuthResponse
        {
            Success = true,
            Message = "Password reset successful."
        };
    }

    private async Task<string> ResolveUserTypeCodeAsync(int? userTypeId)
    {
        if (!userTypeId.HasValue)
        {
            return "CUSTOMER";
        }

        var code = await _dbContext.UserTypes
            .Where(x => x.Id == userTypeId.Value)
            .Select(x => x.Code)
            .FirstOrDefaultAsync();

        return string.IsNullOrWhiteSpace(code) ? "CUSTOMER" : code;
    }

    private static bool IsAdminUserType(string userType)
    {
        return userType.Equals("ADMIN", StringComparison.OrdinalIgnoreCase)
            || userType.Equals("SUPER_ADMIN", StringComparison.OrdinalIgnoreCase);
    }
}
