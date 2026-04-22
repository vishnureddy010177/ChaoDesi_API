namespace ChaoDesi.Application.Features.Auth.Requests;

public class VerifyOtpRequest
{
    public string LoginId { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
}