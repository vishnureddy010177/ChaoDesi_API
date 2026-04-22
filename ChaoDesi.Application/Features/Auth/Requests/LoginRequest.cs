namespace ChaoDesi.Application.Features.Auth.Requests;

public class LoginRequest
{
    public string LoginId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}