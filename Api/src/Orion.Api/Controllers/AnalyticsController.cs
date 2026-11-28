using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orion.Application.DTOs.Analytics;
using Orion.Application.Interfaces;
using Orion.Domain.Enums;

namespace Orion.Api.Controllers;

/// <summary>
/// RESTful endpoints for visitor/adventurer telemetry tracking and Admin-only analytics summaries.
/// </summary>
[ApiController]
[Route("api/analytics")]
[Tags("Analytics")]
public sealed class AnalyticsController(IAnalyticsService analyticsService) : ControllerBase
{
    /// <summary>
    /// Tracks a page view or learning module progression event (page_view, module_start, module_complete).
    /// Automatically attaches the authenticated adventurer's UserId if a valid Bearer JWT is provided.
    /// </summary>
    [HttpPost("track")]
    [AllowAnonymous]
    [EndpointSummary("Track page view or module event")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TrackEvent(
        [FromBody] TrackEventRequest request,
        CancellationToken cancellationToken)
    {
        var userId = TryGetAuthenticatedUserId();
        var userAgent = Request.Headers.UserAgent.ToString();

        await analyticsService.TrackEventAsync(
            request: request,
            userId: userId,
            userAgent: string.IsNullOrWhiteSpace(userAgent) ? null : userAgent,
            cancellationToken: cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Retrieves aggregated platform visitor and per-module progression metrics (restricted to Admin role).
    /// </summary>
    [HttpGet("admin/summary")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [EndpointSummary("Get admin analytics summary (Admin only)")]
    [ProducesResponseType<AdminAnalyticsSummaryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminAnalyticsSummaryDto>> GetAdminSummary(
        CancellationToken cancellationToken)
    {
        var summary = await analyticsService.GetAdminSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    private Guid? TryGetAuthenticatedUserId()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(subject, out var userId) ? userId : null;
    }
}
