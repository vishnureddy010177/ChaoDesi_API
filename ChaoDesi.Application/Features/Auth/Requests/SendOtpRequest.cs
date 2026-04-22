namespace ChaoDesi.Application.Features.Auth.Requests;

public class SendOtpRequest
{
    public string LoginId { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
}