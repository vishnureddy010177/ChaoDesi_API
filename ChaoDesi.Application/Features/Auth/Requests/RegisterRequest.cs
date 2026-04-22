namespace ChaoDesi.Application.Features.Auth.Requests;

public class RegisterRequest
{
    public int? UserTypeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? MobileNumber { get; set; }
    public string Password { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
}