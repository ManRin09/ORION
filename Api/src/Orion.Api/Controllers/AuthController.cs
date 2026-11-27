using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orion.Application.DTOs.Auth;
using Orion.Application.DTOs.Users;
using Orion.Application.Interfaces;

namespace Orion.Api.Controllers;

/// <summary>
/// RESTful endpoints for adventurer login, session inspection, credential updates, and logout.
/// </summary>
[ApiController]
[Route("api/auth")]
[Tags("Auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
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

    private bool TryGetAuthenticatedUserId(out Guid userId)
    {
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(subject, out userId);
    }
}
