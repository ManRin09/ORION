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
/// Real registered adventurer profile combined with their actual learning module progress from database telemetry.
/// </summary>
public sealed record RegisteredUserProgressDto(
    Guid Id,
    string FirstName,
    string? MiddleName,
    string Email,
    string Username,
    string? AvatarUrl,
    string AuthProvider,
    string Role,
    string PlanType,
    int CurrentLevel,
    int TotalXp,
    int CurrentStreak,
    int LongestStreak,
    int Gems,
    IReadOnlyList<string> StartedModules,
    IReadOnlyList<string> CompletedModules,
    int StartedModulesCount,
    int CompletedModulesCount,
    int TotalModulesCount,
    double OverallProgressPercent,
    DateTime CreatedAt,
    DateTime? LastActivityAtUtc);

/// <summary>
/// Aggregated platform analytics summary and registered user progression visible exclusively to users with the Admin role.
/// </summary>
public sealed record AdminAnalyticsSummaryDto(
    int UniqueVisitorsLast30Days,
    int ActiveVisitorsLast24Hours,
    int TotalRegisteredUsers,
    IReadOnlyList<ModuleAnalyticsDto> Modules,
    IReadOnlyList<RegisteredUserProgressDto> UsersProgress);
