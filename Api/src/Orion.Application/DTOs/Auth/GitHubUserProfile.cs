namespace Orion.Application.DTOs.Auth;

/// <summary>
/// Normalized profile data retrieved from GitHub OAuth APIs (<c>/user</c> and <c>/user/emails</c>).
/// </summary>
public sealed record GitHubUserProfile(
    string GitHubId,
    string Login,
    string? Name,
    string Email,
    string? AvatarUrl);
