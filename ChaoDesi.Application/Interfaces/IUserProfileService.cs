using ChaoDesi.Application.Features.UserProfile.Requests;
using ChaoDesi.Application.Features.UserProfile.Responses;

namespace ChaoDesi.Application.Interfaces;

public interface IUserProfileService
{
    Task<UserProfileMeResponse> GetMyProfileAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserProfileMeResponse> UpdateMyProfileAsync(
        int userId,
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken = default);
}
