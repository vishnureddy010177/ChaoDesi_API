using ChaoDesi.Application.Features.UserProfile.Requests;
using ChaoDesi.Application.Features.UserProfile.Responses;
using ChaoDesi.Application.Interfaces;
using ChaoDesi.Domain.Entities;
using ChaoDesi.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ChaoDesi.Infrastructure.Services;

public class UserProfileService : IUserProfileService
{
    private const string UploadFolder = "uploads/userprofiles";

    private readonly AppDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserProfileService(
        AppDbContext dbContext,
        IPasswordService passwordService,
        IWebHostEnvironment webHostEnvironment,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
        _webHostEnvironment = webHostEnvironment;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<UserProfileMeResponse> GetMyProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserWithProfileAsync(userId, cancellationToken);
        return MapResponse(user);
    }

    public async Task<UserProfileMeResponse> UpdateMyProfileAsync(
        int userId,
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new InvalidOperationException("Full name is required.");
        }

        var user = await GetUserWithProfileAsync(userId, cancellationToken);

        var email = NormalizeNullable(request.Email);
        var mobileNumber = NormalizeNullable(request.MobileNumber);

        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailExists = await _dbContext.Users.AnyAsync(
                x => x.Id != userId && !x.IsDeleted && x.Email == email,
                cancellationToken);

            if (emailExists)
            {
                throw new InvalidOperationException("Email already exists.");
            }
        }

        if (!string.IsNullOrWhiteSpace(mobileNumber))
        {
            var mobileExists = await _dbContext.Users.AnyAsync(
                x => x.Id != userId && !x.IsDeleted && x.MobileNumber == mobileNumber,
                cancellationToken);

            if (mobileExists)
            {
                throw new InvalidOperationException("Mobile number already exists.");
            }
        }

        user.FullName = request.FullName.Trim();
        user.Email = email;
        user.MobileNumber = mobileNumber;
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = _passwordService.HashPassword(request.Password);
        }

        var profile = user.UserProfile;
        if (profile == null)
        {
            profile = new UserProfile
            {
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.UserProfiles.Add(profile);
            user.UserProfile = profile;
        }

        profile.Country = NormalizeNullable(request.Country);
        profile.State = NormalizeNullable(request.State);
        profile.City = NormalizeNullable(request.City);
        profile.AddressLine1 = NormalizeNullable(request.AddressLine1);
        profile.AddressLine2 = NormalizeNullable(request.AddressLine2);
        profile.ZipCode = NormalizeNullable(request.ZipCode);
        profile.FacebookUrl = NormalizeNullable(request.FacebookUrl);
        profile.TwitterUrl = NormalizeNullable(request.TwitterUrl);
        profile.YoutubeUrl = NormalizeNullable(request.YoutubeUrl);
        profile.WebsiteUrl = NormalizeNullable(request.WebsiteUrl);
        profile.IsPremiumServiceProvider = request.IsPremiumServiceProvider;
        profile.UpdatedAt = DateTime.UtcNow;

        if (request.ProfileImageFile is { Length: > 0 })
        {
            profile.ProfileImageUrl = await SaveFileAsync(request.ProfileImageFile, cancellationToken);
        }

        if (request.CoverImageFile is { Length: > 0 })
        {
            profile.CoverImageUrl = await SaveFileAsync(request.CoverImageFile, cancellationToken);
        }

        if (request.PhotoIdProofFile is { Length: > 0 })
        {
            profile.PhotoIdProofUrl = await SaveFileAsync(request.PhotoIdProofFile, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapResponse(user);
    }

    private async Task<User> GetUserWithProfileAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(x => x.UserProfile)
            .FirstOrDefaultAsync(
                x => x.Id == userId && !x.IsDeleted && x.IsActive,
                cancellationToken);

        return user ?? throw new KeyNotFoundException("Authenticated user was not found.");
    }

    private async Task<string> SaveFileAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var webRootPath = _webHostEnvironment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
        }

        var uploadDirectory = Path.Combine(
            webRootPath,
            UploadFolder.Replace('/', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(uploadDirectory);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadDirectory, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/{UploadFolder}/{fileName}";
    }

    private UserProfileMeResponse MapResponse(User user)
    {
        var profile = user.UserProfile;

        return new UserProfileMeResponse
        {
            Data = new UserProfileMeDataDto
            {
                User = new UserSummaryDto
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    MobileNumber = user.MobileNumber,
                    CreatedAt = user.CreatedAt,
                    IsEmailVerified = user.IsEmailVerified,
                    IsMobileVerified = user.IsMobileVerified
                },
                UserProfile = new UserProfileDetailsDto
                {
                    Country = profile?.Country,
                    State = profile?.State,
                    City = profile?.City,
                    AddressLine1 = profile?.AddressLine1,
                    AddressLine2 = profile?.AddressLine2,
                    ZipCode = profile?.ZipCode,
                    FacebookUrl = profile?.FacebookUrl,
                    TwitterUrl = profile?.TwitterUrl,
                    YoutubeUrl = profile?.YoutubeUrl,
                    WebsiteUrl = profile?.WebsiteUrl,
                    ProfileImageUrl = BuildPublicUrl(profile?.ProfileImageUrl),
                    CoverImageUrl = BuildPublicUrl(profile?.CoverImageUrl),
                    PhotoIdProofUrl = BuildPublicUrl(profile?.PhotoIdProofUrl),
                    IsPremiumServiceProvider = profile?.IsPremiumServiceProvider ?? false,
                    ProfileExpiryDate = profile?.ProfileExpiryDate
                }
            }
        };
    }

    private string? BuildPublicUrl(string? storedUrl)
    {
        var normalizedUrl = NormalizeNullable(storedUrl);
        if (normalizedUrl == null)
        {
            return null;
        }

        if (Uri.TryCreate(normalizedUrl, UriKind.Absolute, out _))
        {
            return normalizedUrl;
        }

        var request = _httpContextAccessor.HttpContext?.Request;
        if (request == null)
        {
            return normalizedUrl;
        }

        return $"{request.Scheme}://{request.Host}{normalizedUrl}";
    }

    private static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
