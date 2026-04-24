namespace ChaoDesi.Application.Features.Auth.Responses;

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public int? UserId { get; set; }
    public string? CustomerCode { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? UserType { get; set; }
    public string? RedirectUrl { get; set; }
}
