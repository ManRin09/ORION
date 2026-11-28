namespace Orion.Application.DTOs.Analytics;

/// <summary>
/// Per-module funnel metrics (unique starts, unique completions, and completion percentage) for the Admin dashboard.
/// </summary>
public sealed record ModuleAnalyticsDto(
    string Slug,
    string Title,
    int OrderIndex,
    int UniqueStarts,
    int UniqueCompletions,
    double CompletionRatePercent);

/// <summary>
/// Aggregated platform analytics summary visible exclusively to users with the Admin role.
/// </summary>
public sealed record AdminAnalyticsSummaryDto(
    int UniqueVisitorsLast30Days,
    int ActiveVisitorsLast24Hours,
    int TotalRegisteredUsers,
    IReadOnlyList<ModuleAnalyticsDto> Modules);
