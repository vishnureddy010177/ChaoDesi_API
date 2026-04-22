namespace ChaoDesi.Domain.Entities;

public class OtpVerification
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string LoginId { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}