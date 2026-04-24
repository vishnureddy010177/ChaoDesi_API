namespace ChaoDesi.Application.Features.Admin.Responses;

public class AdminDashboardResponse
{
    public int AllUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int GeneralUsers { get; set; }
    public int ServiceProviderUsers { get; set; }
    public int FreeUsers { get; set; }
    public int PremiumUsers { get; set; }
    public int StandardUsers { get; set; }
    public int PremiumPlusUsers { get; set; }
    public int PaidUsers { get; set; }
    public int NonPaidUsers { get; set; }
    public int NewUsersLast30Days { get; set; }
    public List<AdminChartPointResponse> RegisteredUsersChart { get; set; } = new();
}

public class AdminChartPointResponse
{
    public string Day { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class AdminUserListResponse
{
    public List<AdminUserResponse> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class AdminUserResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? MobileNumber { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string UserTypeCode { get; set; } = string.Empty;
    public string UserTypeName { get; set; } = string.Empty;
    public string PlanType { get; set; } = string.Empty;
    public string Amount { get; set; } = "$0";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? ProfileExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsMobileVerified { get; set; }
    public bool IsPremiumServiceProvider { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? ZipCode { get; set; }
    public string? FacebookUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? ProfileImageUrl { get; set; }
}
