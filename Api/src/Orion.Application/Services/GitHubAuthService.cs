using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Orion.Application.DTOs.Auth;
using Orion.Application.DTOs.Users;
using Orion.Application.Interfaces;
using Orion.Domain.Entities;
using Orion.Domain.Exceptions;
using Orion.Domain.Interfaces;

namespace Orion.Application.Services;

/// <summary>
/// Orchestrates GitHub OAuth sign-in by locating existing adventurers, linking by verified email,
/// or spawning a new Level 1 adventurer and issuing an Orion JWT access token.
/// </summary>
public sealed partial class GitHubAuthService(
    IGitHubOAuthClient gitHubOAuthClient,
    IUserRepository userRepository,
    ITokenService tokenService) : IGitHubAuthService
{
    private const int MaxFirstOrMiddleNameLength = 100;
    private const int MaxGeneratedUsernameBaseLength = 38;
    private const int MaxUsernameCollisionAttempts = 8;

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumericRegex();

    public string BuildLoginRedirectUrl(string state)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
        return gitHubOAuthClient.BuildAuthorizationUrl(state);
    }

    public string GetFrontendRedirectUrl() => gitHubOAuthClient.GetFrontendRedirectUrl();

    public async Task<AuthResponse> AuthenticateAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("GitHub authorization code is required.");
        }

        var profile = await gitHubOAuthClient.ExchangeCodeForUserProfileAsync(code, cancellationToken);
        var normalizedEmail = User.NormalizeEmail(profile.Email);

        var user = await userRepository.GetByGitHubIdAsync(profile.GitHubId, cancellationToken);
        if (user is null)
        {
            user = await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
            if (user is not null)
            {
                user.LinkGitHub(profile.GitHubId, profile.AvatarUrl);
                await userRepository.SaveChangesAsync(cancellationToken);
            }
            else
            {
                if (await userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken))
                {
                    throw new DomainException(
                        "An inactive adventurer account is already registered with this email address.");
                }

                var (firstName, middleName) = SplitDisplayName(profile.Name, profile.Login);
                var username = await ResolveUniqueUsernameAsync(profile.Login, normalizedEmail, cancellationToken);
                var utcNow = DateTime.UtcNow;

                user = User.CreateGitHub(
                    firstName: firstName,
                    middleName: middleName,
                    email: normalizedEmail,
                    username: username,
                    gitHubId: profile.GitHubId,
                    avatarUrl: profile.AvatarUrl,
                    acceptedTermsAtUtc: utcNow,
                    utcNow: utcNow);

                await userRepository.AddAsync(user, cancellationToken);
                await userRepository.SaveChangesAsync(cancellationToken);
            }
        }

        var (accessToken, expiresAtUtc) = tokenService.GenerateAccessToken(user, rememberTerminal: true);

        return new AuthResponse(
            AccessToken: accessToken,
            TokenType: "Bearer",
            ExpiresAtUtc: expiresAtUtc,
            RememberTerminal: true,
            User: UserResponse.FromEntity(user));
    }

    private static (string FirstName, string? MiddleName) SplitDisplayName(string? name, string fallbackLogin)
    {
        var raw = string.IsNullOrWhiteSpace(name) ? fallbackLogin : name.Trim();
        var parts = raw.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var firstName = Truncate(parts.Length > 0 ? parts[0] : fallbackLogin.Trim(), MaxFirstOrMiddleNameLength);
        var middleName = parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1])
            ? Truncate(parts[1], MaxFirstOrMiddleNameLength)
            : null;

        return (firstName, middleName);
    }

    private async Task<string> ResolveUniqueUsernameAsync(
        string login,
        string normalizedEmail,
        CancellationToken cancellationToken)
    {
        var baseSlug = BuildUsernameBaseSlug(login, normalizedEmail);
        if (!await userRepository.ExistsByUsernameAsync(baseSlug, cancellationToken))
        {
            return baseSlug;
        }

        for (var attempt = 0; attempt < MaxUsernameCollisionAttempts; attempt++)
        {
            var suffix = RandomNumberGenerator.GetHexString(4, lowercase: true);
            var candidate = $"{baseSlug}_{suffix}";
            if (!await userRepository.ExistsByUsernameAsync(candidate, cancellationToken))
            {
                return candidate;
            }
        }

        return $"adventurer_{Guid.NewGuid().ToString("N")[..12]}";
    }

    private static string BuildUsernameBaseSlug(string login, string email)
    {
        var slug = SanitizeSlug(login);
        if (slug.Length < 3)
        {
            var emailLocalPart = email.Split('@', 2)[0];
            slug = SanitizeSlug(emailLocalPart);
        }

        if (slug.Length < 3)
        {
            slug = $"adventurer_{slug}".TrimEnd('_');
        }

        return slug.Length > MaxGeneratedUsernameBaseLength
            ? slug[..MaxGeneratedUsernameBaseLength].TrimEnd('_')
            : slug;
    }

    private static string SanitizeSlug(string input)
    {
        var decomposed = input.Normalize(NormalizationForm.FormD);
        var asciiBuilder = new StringBuilder(decomposed.Length);

        foreach (var ch in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                asciiBuilder.Append(char.ToLowerInvariant(ch));
            }
        }

        return NonAlphanumericRegex()
            .Replace(asciiBuilder.ToString(), "_")
            .Trim('_');
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
