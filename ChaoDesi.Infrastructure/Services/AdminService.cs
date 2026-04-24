using ChaoDesi.Application.Features.Admin.Requests;
using ChaoDesi.Application.Features.Admin.Responses;
using ChaoDesi.Application.Interfaces;
using ChaoDesi.Domain.Entities;
using ChaoDesi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChaoDesi.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordService _passwordService;

    public AdminService(AppDbContext dbContext, IPasswordService passwordService)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
    }

    public async Task<AdminDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var users = await UserQuery()
            .Select(x => new
            {
                x.IsActive,
                x.CreatedAt,
                UserTypeCode = x.UserTypeId == null
                    ? null
                    : _dbContext.UserTypes
                        .Where(t => t.Id == x.UserTypeId)
                        .Select(t => t.Code)
                        .FirstOrDefault(),
                IsPremium = x.UserProfile != null && x.UserProfile.IsPremiumServiceProvider
            })
            .ToListAsync(cancellationToken);

        var startDate = DateTime.UtcNow.Date.AddDays(-29);
        var chart = Enumerable.Range(0, 30)
            .Select(offset =>
            {
                var date = startDate.AddDays(offset);
                return new AdminChartPointResponse
                {
                    Day = date.Day.ToString(),
                    Value = users.Count(x => x.CreatedAt.Date == date)
                };
            })
            .ToList();

        var premiumUsers = users.Count(x => x.IsPremium);
        var freeUsers = users.Count(x => !x.IsPremium);
        var serviceProviderUsers = users.Count(x => IsServiceProviderCode(x.UserTypeCode));
        var generalUsers = users.Count - serviceProviderUsers;

        return new AdminDashboardResponse
        {
            AllUsers = users.Count,
            ActiveUsers = users.Count(x => x.IsActive),
            InactiveUsers = users.Count(x => !x.IsActive),
            GeneralUsers = generalUsers,
            ServiceProviderUsers = serviceProviderUsers,
            FreeUsers = freeUsers,
            PremiumUsers = premiumUsers,
            StandardUsers = 0,
            PremiumPlusUsers = 0,
            PaidUsers = premiumUsers,
            NonPaidUsers = freeUsers,
            NewUsersLast30Days = users.Count(x => x.CreatedAt >= startDate),
            RegisteredUsersChart = chart
        };
    }

    public async Task<AdminUserListResponse> GetUsersAsync(AdminUserQueryRequest request, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 200);
        var query = ApplyFilter(UserQuery(), request.Filter);
        var search = NormalizeNullable(request.Search);

        if (search != null)
        {
            query = query.Where(x =>
                x.FullName.Contains(search) ||
                (x.Email != null && x.Email.Contains(search)) ||
                (x.MobileNumber != null && x.MobileNumber.Contains(search)) ||
                x.CustomerCode.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new AdminUserListResponse
        {
            Items = users.Select(MapUser).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AdminUserResponse> GetUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await UserQuery()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        return user == null ? throw new KeyNotFoundException("User not found.") : MapUser(user);
    }

    public async Task<AdminUserResponse> CreateAdminAccountAsync(CreateAdminAccountRequest request, CancellationToken cancellationToken = default)
    {
        var accountType = NormalizeUserTypeCode(request.AccountType);

        if (!IsAdminAccountType(accountType))
        {
            throw new InvalidOperationException("Account type must be ADMIN or SUPER_ADMIN.");
        }

        var createRequest = new UpsertAdminUserRequest
        {
            FullName = request.FullName,
            Email = request.Email,
            MobileNumber = request.MobileNumber,
            Password = request.Password,
            UserTypeCode = accountType,
            IsActive = request.IsActive,
            IsEmailVerified = request.IsEmailVerified,
            IsMobileVerified = request.IsMobileVerified
        };

        return await CreateUserAsync(createRequest, cancellationToken);
    }

    public async Task<AdminUserResponse> CreateUserAsync(UpsertAdminUserRequest request, CancellationToken cancellationToken = default)
    {
        ValidateUserRequest(request, isCreate: true);
        await EnsureUniqueLoginAsync(null, request, cancellationToken);

        var userType = await GetOrCreateUserTypeAsync(request.UserTypeCode, cancellationToken);
        var user = new User
        {
            UserTypeId = userType.Id,
            CustomerCode = "TEMP",
            FullName = request.FullName.Trim(),
            Email = NormalizeNullable(request.Email),
            MobileNumber = NormalizeNullable(request.MobileNumber),
            PasswordHash = _passwordService.HashPassword(request.Password!),
            IsActive = request.IsActive,
            IsEmailVerified = request.IsEmailVerified,
            IsMobileVerified = request.IsMobileVerified,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        user.CustomerCode = $"CHAO{user.Id:D3}";
        UpsertProfile(user, request);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetUserAsync(user.Id, cancellationToken);
    }

    public async Task<AdminUserResponse> UpdateUserAsync(int userId, UpsertAdminUserRequest request, CancellationToken cancellationToken = default)
    {
        ValidateUserRequest(request, isCreate: false);
        await EnsureUniqueLoginAsync(userId, request, cancellationToken);

        var user = await UserQuery()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var userType = await GetOrCreateUserTypeAsync(request.UserTypeCode, cancellationToken);

        user.UserTypeId = userType.Id;
        user.FullName = request.FullName.Trim();
        user.Email = NormalizeNullable(request.Email);
        user.MobileNumber = NormalizeNullable(request.MobileNumber);
        user.IsActive = request.IsActive;
        user.IsEmailVerified = request.IsEmailVerified;
        user.IsMobileVerified = request.IsMobileVerified;
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = _passwordService.HashPassword(request.Password);
        }

        UpsertProfile(user, request);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetUserAsync(user.Id, cancellationToken);
    }

    public async Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        user.IsDeleted = true;
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<User> UserQuery()
    {
        return _dbContext.Users
            .Include(x => x.UserProfile)
            .Where(x => !x.IsDeleted);
    }

    private IQueryable<User> ApplyFilter(IQueryable<User> query, string? filter)
    {
        var normalizedFilter = NormalizeNullable(filter)?.ToLowerInvariant();

        return normalizedFilter switch
        {
            "general" => query.Where(x => !_dbContext.UserTypes
                .Where(t => t.Id == x.UserTypeId)
                .Select(t => t.Code)
                .Any(code => code == "SERVICE_PROVIDER" || code == "PROVIDER" || code == "EXPERT")),
            "service-provider" => query.Where(x => _dbContext.UserTypes
                .Where(t => t.Id == x.UserTypeId)
                .Select(t => t.Code)
                .Any(code => code == "SERVICE_PROVIDER" || code == "PROVIDER" || code == "EXPERT")),
            "free" or "non-paid" => query.Where(x => x.UserProfile == null || !x.UserProfile.IsPremiumServiceProvider),
            "premium" or "paid" => query.Where(x => x.UserProfile != null && x.UserProfile.IsPremiumServiceProvider),
            "standard" or "premium-plus" => query.Where(_ => false),
            "new-requests" => query.Where(x => x.CreatedAt >= DateTime.UtcNow.AddDays(-30)),
            "inactive" => query.Where(x => !x.IsActive),
            _ => query
        };
    }

    private async Task EnsureUniqueLoginAsync(int? userId, UpsertAdminUserRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeNullable(request.Email);
        var mobileNumber = NormalizeNullable(request.MobileNumber);

        if (email != null)
        {
            var exists = await _dbContext.Users.AnyAsync(
                x => !x.IsDeleted && x.Id != userId && x.Email == email,
                cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException("Email already exists.");
            }
        }

        if (mobileNumber != null)
        {
            var exists = await _dbContext.Users.AnyAsync(
                x => !x.IsDeleted && x.Id != userId && x.MobileNumber == mobileNumber,
                cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException("Mobile number already exists.");
            }
        }
    }

    private async Task<UserType> GetOrCreateUserTypeAsync(string? code, CancellationToken cancellationToken)
    {
        var normalizedCode = NormalizeUserTypeCode(code);
        var userType = await _dbContext.UserTypes
            .FirstOrDefaultAsync(x => x.Code == normalizedCode, cancellationToken);

        if (userType != null)
        {
            return userType;
        }

        userType = new UserType
        {
            Code = normalizedCode,
            Name = ToDisplayName(normalizedCode),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.UserTypes.Add(userType);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return userType;
    }

    private static void ValidateUserRequest(UpsertAdminUserRequest request, bool isCreate)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new InvalidOperationException("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.MobileNumber))
        {
            throw new InvalidOperationException("Email or mobile number is required.");
        }

        if (isCreate && string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("Password is required.");
        }
    }

    private static void UpsertProfile(User user, UpsertAdminUserRequest request)
    {
        user.UserProfile ??= new UserProfile
        {
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        user.UserProfile.Country = NormalizeNullable(request.Country);
        user.UserProfile.State = NormalizeNullable(request.State);
        user.UserProfile.City = NormalizeNullable(request.City);
        user.UserProfile.AddressLine1 = NormalizeNullable(request.AddressLine1);
        user.UserProfile.AddressLine2 = NormalizeNullable(request.AddressLine2);
        user.UserProfile.ZipCode = NormalizeNullable(request.ZipCode);
        user.UserProfile.FacebookUrl = NormalizeNullable(request.FacebookUrl);
        user.UserProfile.TwitterUrl = NormalizeNullable(request.TwitterUrl);
        user.UserProfile.YoutubeUrl = NormalizeNullable(request.YoutubeUrl);
        user.UserProfile.WebsiteUrl = NormalizeNullable(request.WebsiteUrl);
        user.UserProfile.IsPremiumServiceProvider = request.IsPremiumServiceProvider;
        user.UserProfile.ProfileExpiryDate = request.ProfileExpiryDate;
        user.UserProfile.UpdatedAt = DateTime.UtcNow;
    }

    private AdminUserResponse MapUser(User user)
    {
        var userType = user.UserTypeId.HasValue
            ? _dbContext.UserTypes.FirstOrDefault(x => x.Id == user.UserTypeId.Value)
            : null;

        var userTypeCode = userType?.Code ?? "CUSTOMER";
        var isPremium = user.UserProfile?.IsPremiumServiceProvider ?? false;

        return new AdminUserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            MobileNumber = user.MobileNumber,
            CustomerCode = user.CustomerCode,
            UserTypeCode = userTypeCode,
            UserTypeName = userType?.Name ?? ToDisplayName(userTypeCode),
            PlanType = isPremium ? "Premium" : "Free",
            Amount = isPremium ? "$19" : "$0",
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            ProfileExpiryDate = user.UserProfile?.ProfileExpiryDate,
            IsActive = user.IsActive,
            IsEmailVerified = user.IsEmailVerified,
            IsMobileVerified = user.IsMobileVerified,
            IsPremiumServiceProvider = isPremium,
            Country = user.UserProfile?.Country,
            State = user.UserProfile?.State,
            City = user.UserProfile?.City,
            AddressLine1 = user.UserProfile?.AddressLine1,
            AddressLine2 = user.UserProfile?.AddressLine2,
            ZipCode = user.UserProfile?.ZipCode,
            FacebookUrl = user.UserProfile?.FacebookUrl,
            TwitterUrl = user.UserProfile?.TwitterUrl,
            YoutubeUrl = user.UserProfile?.YoutubeUrl,
            WebsiteUrl = user.UserProfile?.WebsiteUrl,
            ProfileImageUrl = user.UserProfile?.ProfileImageUrl
        };
    }

    private static string NormalizeUserTypeCode(string? value)
    {
        var normalizedValue = NormalizeNullable(value)?.ToUpperInvariant().Replace(" ", "_").Replace("-", "_");
        return string.IsNullOrWhiteSpace(normalizedValue) ? "CUSTOMER" : normalizedValue;
    }

    private static bool IsServiceProviderCode(string? code)
    {
        return code is "SERVICE_PROVIDER" or "PROVIDER" or "EXPERT";
    }

    private static bool IsAdminAccountType(string code)
    {
        return code is "ADMIN" or "SUPER_ADMIN";
    }

    private static string ToDisplayName(string code)
    {
        return string.Join(
            " ",
            code.Split('_', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant()));
    }

    private static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
