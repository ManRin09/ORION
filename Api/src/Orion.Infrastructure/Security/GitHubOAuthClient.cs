using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Orion.Application.DTOs.Auth;
using Orion.Application.Interfaces;
using Orion.Domain.Exceptions;

namespace Orion.Infrastructure.Security;

/// <summary>
/// Outbound HTTP client implementing GitHub OAuth code exchange and user/email profile retrieval.
/// </summary>
public sealed class GitHubOAuthClient(
    IHttpClientFactory httpClientFactory,
    IOptions<GitHubOAuthSettings> gitHubOptions) : IGitHubOAuthClient
{
    public const string HttpClientName = "GitHubOAuth";
    public const string UserAgentValue = "Orion-App";

    private const string AuthorizeEndpoint = "https://github.com/login/oauth/authorize";
    private const string AccessTokenEndpoint = "https://github.com/login/oauth/access_token";
    private const string UserEndpoint = "https://api.github.com/user";
    private const string UserEmailsEndpoint = "https://api.github.com/user/emails";
    private const string OAuthScope = "read:user user:email";

    public string BuildAuthorizationUrl(string state)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(state);

        var settings = gitHubOptions.Value;
        RequireConfigured(settings.ClientId, "GitHubOAuth:ClientId");
        RequireConfigured(settings.CallbackUrl, "GitHubOAuth:CallbackUrl");

        return $"{AuthorizeEndpoint}" +
               $"?client_id={Uri.EscapeDataString(settings.ClientId)}" +
               $"&redirect_uri={Uri.EscapeDataString(settings.CallbackUrl)}" +
               $"&scope={Uri.EscapeDataString(OAuthScope)}" +
               $"&state={Uri.EscapeDataString(state)}";
    }

    public string GetFrontendRedirectUrl()
    {
        var frontendRedirectUrl = gitHubOptions.Value.FrontendRedirectUrl;
        RequireConfigured(frontendRedirectUrl, "GitHubOAuth:FrontendRedirectUrl");
        return frontendRedirectUrl;
    }

    public async Task<GitHubUserProfile> ExchangeCodeForUserProfileAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var settings = gitHubOptions.Value;
        RequireConfigured(settings.ClientId, "GitHubOAuth:ClientId");
        RequireConfigured(settings.ClientSecret, "GitHubOAuth:ClientSecret");
        RequireConfigured(settings.CallbackUrl, "GitHubOAuth:CallbackUrl");

        var client = httpClientFactory.CreateClient(HttpClientName);

        var accessToken = await ExchangeCodeForAccessTokenAsync(client, settings, code, cancellationToken);
        var gitHubUser = await FetchGitHubUserAsync(client, accessToken, cancellationToken);

        var email = gitHubUser.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            email = await FetchPrimaryVerifiedEmailAsync(client, accessToken, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException(
                "A verified primary email address on GitHub is required to sign in to Orion.");
        }

        if (gitHubUser.Id <= 0 || string.IsNullOrWhiteSpace(gitHubUser.Login))
        {
            throw new DomainException("GitHub returned an incomplete adventurer profile.");
        }

        return new GitHubUserProfile(
            GitHubId: gitHubUser.Id.ToString(CultureInfo.InvariantCulture),
            Login: gitHubUser.Login.Trim(),
            Name: string.IsNullOrWhiteSpace(gitHubUser.Name) ? null : gitHubUser.Name.Trim(),
            Email: email,
            AvatarUrl: string.IsNullOrWhiteSpace(gitHubUser.AvatarUrl) ? null : gitHubUser.AvatarUrl.Trim());
    }

    private static async Task<string> ExchangeCodeForAccessTokenAsync(
        HttpClient client,
        GitHubOAuthSettings settings,
        string code,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, AccessTokenEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = settings.ClientId,
                ["client_secret"] = settings.ClientSecret,
                ["code"] = code,
                ["redirect_uri"] = settings.CallbackUrl
            })
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        EnsureUserAgent(request);

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new DomainException(
                $"GitHub token exchange failed with status code {(int)response.StatusCode}.");
        }

        var tokenPayload = await response.Content.ReadFromJsonAsync<GitHubAccessTokenResponse>(
            cancellationToken: cancellationToken);

        if (tokenPayload is null || !string.IsNullOrWhiteSpace(tokenPayload.Error) ||
            string.IsNullOrWhiteSpace(tokenPayload.AccessToken))
        {
            var reason = tokenPayload?.ErrorDescription ?? tokenPayload?.Error ?? "empty access token";
            throw new DomainException($"GitHub OAuth token exchange was rejected: {reason}.");
        }

        return tokenPayload.AccessToken;
    }

    private static async Task<GitHubUserDto> FetchGitHubUserAsync(
        HttpClient client,
        string accessToken,
        CancellationToken cancellationToken)
    {
        using var request = CreateGitHubApiRequest(UserEndpoint, accessToken);
        using var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new DomainException(
                $"Failed to retrieve GitHub user profile (HTTP {(int)response.StatusCode}).");
        }

        var user = await response.Content.ReadFromJsonAsync<GitHubUserDto>(
            cancellationToken: cancellationToken);

        return user ?? throw new DomainException("GitHub returned an empty user profile.");
    }

    private static async Task<string?> FetchPrimaryVerifiedEmailAsync(
        HttpClient client,
        string accessToken,
        CancellationToken cancellationToken)
    {
        using var request = CreateGitHubApiRequest(UserEmailsEndpoint, accessToken);
        using var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new DomainException(
                $"Failed to retrieve GitHub user emails (HTTP {(int)response.StatusCode}).");
        }

        var emails = await response.Content.ReadFromJsonAsync<List<GitHubEmailDto>>(
            cancellationToken: cancellationToken);

        return emails?
            .FirstOrDefault(e => e.Primary && e.Verified && !string.IsNullOrWhiteSpace(e.Email))
            ?.Email
            ?.Trim();
    }

    private static HttpRequestMessage CreateGitHubApiRequest(string url, string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        EnsureUserAgent(request);
        return request;
    }

    private static void EnsureUserAgent(HttpRequestMessage request)
    {
        if (!request.Headers.UserAgent.Any())
        {
            request.Headers.UserAgent.ParseAdd(UserAgentValue);
        }
    }

    private static void RequireConfigured(string? value, string keyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Configuration value '{keyName}' is not configured.");
        }
    }

    private sealed class GitHubAccessTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; init; }

        [JsonPropertyName("error")]
        public string? Error { get; init; }

        [JsonPropertyName("error_description")]
        public string? ErrorDescription { get; init; }
    }

    private sealed class GitHubUserDto
    {
        [JsonPropertyName("id")]
        public long Id { get; init; }

        [JsonPropertyName("login")]
        public string? Login { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("email")]
        public string? Email { get; init; }

        [JsonPropertyName("avatar_url")]
        public string? AvatarUrl { get; init; }
    }

    private sealed class GitHubEmailDto
    {
        [JsonPropertyName("email")]
        public string? Email { get; init; }

        [JsonPropertyName("primary")]
        public bool Primary { get; init; }

        [JsonPropertyName("verified")]
        public bool Verified { get; init; }
    }
}
