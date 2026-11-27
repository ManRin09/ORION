using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Orion.Application.DTOs.Auth;
using Orion.Application.DTOs.Users;
using Orion.Application.Interfaces;
using Orion.Domain.Exceptions;

namespace Orion.Api.Controllers;

/// <summary>
/// RESTful endpoints for adventurer login, GitHub OAuth, session inspection, credential updates, and logout.
/// </summary>
[ApiController]
[Route("api/auth")]
[Tags("Auth")]
public sealed class AuthController(
    IAuthService authService,
    IGitHubAuthService gitHubAuthService,
    IHostEnvironment environment,
    ILogger<AuthController> logger) : ControllerBase
{
    private const string GitHubStateCookieName = "orion_github_oauth_state";
    private const string GitHubCookiePath = "/api/auth/github";
    private static readonly TimeSpan StateCookieLifetime = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Authenticates an active adventurer by username or email ("Slot 01: Load Game").
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EndpointSummary("Authenticate adventurer")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Initiates the GitHub OAuth authorization redirect with an HttpOnly SameSite=Lax anti-CSRF state cookie.
    /// </summary>
    [HttpGet("github/login")]
    [AllowAnonymous]
    [EndpointSummary("Start GitHub OAuth login flow")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult GitHubLogin()
    {
        var state = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(32));
        var authorizeUrl = gitHubAuthService.BuildLoginRedirectUrl(state);

        Response.Cookies.Append(GitHubStateCookieName, state, BuildStateCookieOptions(includeMaxAge: true));

        return Redirect(authorizeUrl);
    }

    /// <summary>
    /// Handles the GitHub OAuth callback, validates the anti-CSRF state cookie, links/spawns the adventurer,
    /// and redirects to the frontend callback with the issued Orion JWT.
    /// </summary>
    [HttpGet("github/callback")]
    [AllowAnonymous]
    [EndpointSummary("Complete GitHub OAuth callback")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> GitHubCallback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error,
        CancellationToken cancellationToken)
    {
        var frontendRedirectUrl = gitHubAuthService.GetFrontendRedirectUrl();
        Request.Cookies.TryGetValue(GitHubStateCookieName, out var expectedState);
        Response.Cookies.Delete(GitHubStateCookieName, BuildStateCookieOptions(includeMaxAge: false));

        if (!string.IsNullOrWhiteSpace(error))
        {
            logger.LogWarning("GitHub OAuth returned error '{Error}'.", error);
            return Redirect(QueryHelpers.AddQueryString(frontendRedirectUrl, "error", error));
        }

        if (!IsValidState(expectedState, state))
        {
            logger.LogWarning("GitHub OAuth callback rejected due to missing or mismatched anti-CSRF state.");
            return Redirect(QueryHelpers.AddQueryString(frontendRedirectUrl, "error", "invalid_state"));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return Redirect(QueryHelpers.AddQueryString(frontendRedirectUrl, "error", "missing_code"));
        }

        try
        {
            var authResponse = await gitHubAuthService.AuthenticateAsync(code, cancellationToken);
            var redirectWithToken = QueryHelpers.AddQueryString(
                frontendRedirectUrl,
                "token",
                authResponse.AccessToken);

            return Redirect(redirectWithToken);
        }
        catch (DomainException domainException)
        {
            logger.LogWarning(domainException, "GitHub OAuth domain error during callback.");
            return Redirect(QueryHelpers.AddQueryString(frontendRedirectUrl, "error", "github_auth_failed"));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected failure during GitHub OAuth callback.");
            return Redirect(QueryHelpers.AddQueryString(frontendRedirectUrl, "error", "oauth_server_error"));
        }
    }

    /// <summary>
    /// Retrieves the currently authenticated adventurer profile from the bearer token session.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [EndpointSummary("Get current authenticated adventurer")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetCurrentUser(CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized();
        }

        var user = await authService.GetCurrentUserAsync(userId, cancellationToken);
        return Ok(user);
    }

    /// <summary>
    /// Updates the Secret Spell (password) for the currently authenticated adventurer.
    /// </summary>
    [HttpPut("password")]
    [Authorize]
    [EndpointSummary("Change authenticated adventurer password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized();
        }

        await authService.ChangePasswordAsync(userId, request, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Ends the current adventurer login session (stateless logout confirmation).
    /// </summary>
    [HttpDelete("session")]
    [Authorize]
    [EndpointSummary("End current login session (logout)")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Logout() => NoContent();

    private CookieOptions BuildStateCookieOptions(bool includeMaxAge)
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = !environment.IsDevelopment(),
            IsEssential = true,
            Path = GitHubCookiePath
        };

        if (includeMaxAge)
        {
            options.MaxAge = StateCookieLifetime;
        }

        return options;
    }

    private static bool IsValidState(string? expectedState, string? receivedState)
    {
        if (string.IsNullOrWhiteSpace(expectedState) || string.IsNullOrWhiteSpace(receivedState))
        {
            return false;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(expectedState);
        var receivedBytes = Encoding.UTF8.GetBytes(receivedState);

        return expectedBytes.Length == receivedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(expectedBytes, receivedBytes);
    }

    private bool TryGetAuthenticatedUserId(out Guid userId)
    {
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(subject, out userId);
    }
}
