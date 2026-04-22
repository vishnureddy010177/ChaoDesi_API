namespace ChaoDesi.Application.Features.UserProfile.Responses;

public class UserProfileMeResponse
{
    public UserProfileMeDataDto Data { get; set; } = new();
}

public class UserProfileMeDataDto
{
    public UserSummaryDto User { get; set; } = new();
    public UserProfileDetailsDto UserProfile { get; set; } = new();
}

public class UserSummaryDto
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? MobileNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsMobileVerified { get; set; }
}

public class UserProfileDetailsDto
{
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
    public string? CoverImageUrl { get; set; }
    public string? PhotoIdProofUrl { get; set; }
    public bool IsPremiumServiceProvider { get; set; }
    public DateTime? ProfileExpiryDate { get; set; }
}
