using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orion.Application.DTOs.Common;
using Orion.Application.DTOs.Users;
using Orion.Application.Interfaces;
using Orion.Domain.Enums;

namespace Orion.Api.Controllers;

/// <summary>
/// RESTful endpoints for adventurer registration, retrieval, profile updates, role management, and soft deletion.
/// Enforces authentication and object-level authorization (owner or Admin) to prevent IDOR/BOLA vulnerabilities.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize]
[Tags("Users")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Registers a new local adventurer ("Spawn into Orion") with Level 1 initial RPG state.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [EndpointSummary("Register a new adventurer")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var createdUser = await userService.RegisterAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
    }

    /// <summary>
    /// Retrieves an active adventurer by unique identifier (restricted to the account owner or an Admin).
    /// </summary>
    [HttpGet("{id:guid}")]
    [EndpointSummary("Get adventurer by ID")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var callerId))
        {
            return Unauthorized();
        }

        if (!IsOwnerOrAdmin(callerId, id))
        {
            return Forbid();
        }

        var user = await userService.GetByIdAsync(id, cancellationToken);
        return Ok(user);
    }

    /// <summary>
    /// Lists active adventurers using pagination (restricted to Admin role).
    /// </summary>
    [HttpGet]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [EndpointSummary("List active adventurers (Admin only)")]
    [ProducesResponseType<PagedResponse<UserResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResponse<UserResponse>>> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var users = await userService.GetPagedAsync(pageNumber, pageSize, cancellationToken);
        return Ok(users);
    }

    /// <summary>
    /// Updates editable profile details for an active adventurer (restricted to the account owner or an Admin).
    /// </summary>
    [HttpPut("{id:guid}/profile")]
    [EndpointSummary("Update adventurer profile")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> UpdateProfile(
        Guid id,
        [FromBody] UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var callerId))
        {
            return Unauthorized();
        }

        if (!IsOwnerOrAdmin(callerId, id))
        {
            return Forbid();
        }

        var updatedUser = await userService.UpdateProfileAsync(id, request, cancellationToken);
        return Ok(updatedUser);
    }

    /// <summary>
    /// Updates the authorization role ("User" or "Admin") for an active adventurer (Admin only).
    /// </summary>
    [HttpPut("{id:guid}/role")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [EndpointSummary("Update adventurer role (Admin only)")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> UpdateRole(
        Guid id,
        [FromBody] UpdateUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        var updatedUser = await userService.UpdateRoleAsync(id, request, cancellationToken);
        return Ok(updatedUser);
    }

    /// <summary>
    /// Soft-deletes (deactivates) an adventurer account while preserving historical records (restricted to owner or Admin).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [EndpointSummary("Soft-delete an adventurer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SoftDelete(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var callerId))
        {
            return Unauthorized();
        }

        if (!IsOwnerOrAdmin(callerId, id))
        {
            return Forbid();
        }

        await userService.SoftDeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private bool TryGetAuthenticatedUserId(out Guid userId)
    {
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(subject, out userId);
    }

    private bool IsOwnerOrAdmin(Guid callerId, Guid targetUserId) =>
        callerId == targetUserId ||
        User.IsInRole(nameof(UserRole.Admin)) ||
        User.HasClaim("role", nameof(UserRole.Admin));
}
