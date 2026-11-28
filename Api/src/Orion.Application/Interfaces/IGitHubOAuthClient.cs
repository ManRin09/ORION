using Orion.Application.DTOs.Auth;

namespace Orion.Application.Interfaces;

/// <summary>
/// Outbound OAuth client abstraction for communicating with GitHub authorization and user APIs.
/// </summary>
public interface IGitHubOAuthClient
{
    string BuildAuthorizationUrl(string state);

    string GetFrontendRedirectUrl();

    Task<GitHubUserProfile> ExchangeCodeForUserProfileAsync(
        string code,
        CancellationToken cancellationToken = default);
}
