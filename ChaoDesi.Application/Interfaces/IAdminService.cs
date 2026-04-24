using ChaoDesi.Application.Features.Admin.Requests;
using ChaoDesi.Application.Features.Admin.Responses;

namespace ChaoDesi.Application.Interfaces;

public interface IAdminService
{
    Task<AdminDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<AdminUserListResponse> GetUsersAsync(AdminUserQueryRequest request, CancellationToken cancellationToken = default);
    Task<AdminUserResponse> GetUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<AdminUserResponse> CreateAdminAccountAsync(CreateAdminAccountRequest request, CancellationToken cancellationToken = default);
    Task<AdminUserResponse> CreateUserAsync(UpsertAdminUserRequest request, CancellationToken cancellationToken = default);
    Task<AdminUserResponse> UpdateUserAsync(int userId, UpsertAdminUserRequest request, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default);
}
