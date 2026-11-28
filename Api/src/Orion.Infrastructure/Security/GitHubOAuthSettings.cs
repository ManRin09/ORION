namespace Orion.Infrastructure.Security;

/// <summary>
/// Typed configuration options for GitHub OAuth mapped from the <c>GitHubOAuth</c> section
/// (Render env vars: <c>GitHubOAuth__ClientId</c>, <c>GitHubOAuth__ClientSecret</c>,
/// <c>GitHubOAuth__CallbackUrl</c>, <c>GitHubOAuth__FrontendRedirectUrl</c>).
/// </summary>
public sealed class GitHubOAuthSettings
{
    public const string SectionName = "GitHubOAuth";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;
    public string FrontendRedirectUrl { get; set; } = string.Empty;
}
