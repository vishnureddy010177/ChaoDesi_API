namespace ChaoDesi.Application.Features.Admin.Requests;

public class AdminUserQueryRequest
{
    public string? Filter { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 100;
}

public class UpsertAdminUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? MobileNumber { get; set; }
    public string? Password { get; set; }
    public string UserTypeCode { get; set; } = "CUSTOMER";
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; }
    public bool IsMobileVerified { get; set; }
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
    public bool IsPremiumServiceProvider { get; set; }
    public DateTime? ProfileExpiryDate { get; set; }
}

public class CreateAdminAccountRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? MobileNumber { get; set; }
    public string Password { get; set; } = string.Empty;
    public string AccountType { get; set; } = "ADMIN";
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; } = true;
    public bool IsMobileVerified { get; set; } = true;
}
