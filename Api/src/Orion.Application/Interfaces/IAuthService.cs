using Orion.Application.DTOs.Auth;
using Orion.Application.DTOs.Users;

namespace Orion.Application.Interfaces;

/// <summary>
/// Application use cases for adventurer authentication, session inspection, and credential updates.
/// </summary>
public interface IAuthService
{
    Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<UserResponse> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default);
}
