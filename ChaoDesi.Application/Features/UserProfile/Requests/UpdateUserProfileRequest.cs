using Microsoft.AspNetCore.Http;

namespace ChaoDesi.Application.Features.UserProfile.Requests;

public class UpdateUserProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? MobileNumber { get; set; }
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
    public string? Password { get; set; }
    public IFormFile? ProfileImageFile { get; set; }
    public IFormFile? CoverImageFile { get; set; }
    public IFormFile? PhotoIdProofFile { get; set; }
}
