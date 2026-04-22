using ChaoDesi.Application.Features.Auth.Requests;
using ChaoDesi.Application.Features.Auth.Responses;

namespace ChaoDesi.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> SendOtpAsync(SendOtpRequest request);
    Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request);
}