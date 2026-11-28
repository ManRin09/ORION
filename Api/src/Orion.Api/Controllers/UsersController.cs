using Microsoft.AspNetCore.Mvc;
using Orion.Application.DTOs.Common;
using Orion.Application.DTOs.Users;
using Orion.Application.Interfaces;

namespace Orion.Api.Controllers;

/// <summary>
/// RESTful endpoints for adventurer registration, retrieval, profile updates, and soft deletion.
/// </summary>
[ApiController]
[Route("api/users")]
[Tags("Users")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Registers a new local adventurer ("Spawn into Orion") with Level 1 initial RPG state.
    /// </summary>
    [HttpPost("register")]
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
    /// Retrieves an active adventurer by unique identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [EndpointSummary("Get adventurer by ID")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(id, cancellationToken);
        return Ok(user);
    }

    /// <summary>
    /// Lists active adventurers using basic pagination.
    /// </summary>
    [HttpGet]
    [EndpointSummary("List active adventurers (paginated)")]
    [ProducesResponseType<PagedResponse<UserResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<UserResponse>>> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var users = await userService.GetPagedAsync(pageNumber, pageSize, cancellationToken);
        return Ok(users);
    }

    /// <summary>
    /// Updates editable profile details for an active adventurer.
    /// </summary>
    [HttpPut("{id:guid}/profile")]
    [EndpointSummary("Update adventurer profile")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> UpdateProfile(
        Guid id,
        [FromBody] UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var updatedUser = await userService.UpdateProfileAsync(id, request, cancellationToken);
        return Ok(updatedUser);
    }

    /// <summary>
    /// Updates the authorization role ("User" or "Admin") for an active adventurer.
    /// </summary>
    [HttpPut("{id:guid}/role")]
    [EndpointSummary("Update adventurer role")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
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
    /// Soft-deletes (deactivates) an adventurer account while preserving historical records.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [EndpointSummary("Soft-delete an adventurer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SoftDelete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await userService.SoftDeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
