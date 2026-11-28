using Orion.Application.DTOs.Auth;

namespace Orion.Application.Interfaces;

/// <summary>
/// Application use cases orchestrating GitHub OAuth login, account linking, registration, and JWT issuance.
/// </summary>
public interface IGitHubAuthService
{
    string BuildLoginRedirectUrl(string state);

    string GetFrontendRedirectUrl();

    Task<AuthResponse> AuthenticateAsync(
        string code,
        CancellationToken cancellationToken = default);
}
