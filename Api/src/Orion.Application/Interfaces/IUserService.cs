using Orion.Application.DTOs.Common;
using Orion.Application.DTOs.Users;

namespace Orion.Application.Interfaces;

/// <summary>
/// Application use cases for adventurer registration, retrieval, profile updates, and soft deletion.
/// </summary>
public interface IUserService
{
    Task<UserResponse> RegisterAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default);

    Task<UserResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResponse<UserResponse>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<UserResponse> UpdateProfileAsync(
        Guid id,
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<UserResponse> UpdateRoleAsync(
        Guid id,
        UpdateUserRoleRequest request,
        CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
