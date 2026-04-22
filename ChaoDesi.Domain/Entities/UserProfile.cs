namespace ChaoDesi.Domain.Entities;

public class UserProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? ZipCode { get; set; }
    public string? PhotoIdProofUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public bool IsPremiumServiceProvider { get; set; }
    public DateTime? ProfileExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User? User { get; set; }
}
