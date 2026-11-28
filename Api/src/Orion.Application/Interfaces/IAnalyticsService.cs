using Orion.Application.DTOs.Analytics;

namespace Orion.Application.Interfaces;

/// <summary>
/// Use cases for recording telemetry events and generating the Admin analytics summary.
/// </summary>
public interface IAnalyticsService
{
    Task TrackEventAsync(
        TrackEventRequest request,
        Guid? userId = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);

    Task<AdminAnalyticsSummaryDto> GetAdminSummaryAsync(
        CancellationToken cancellationToken = default);
}
