using Orion.Domain.Entities;

namespace Orion.Domain.Interfaces;

/// <summary>
/// Aggregated visitor counts per module for start and completion telemetry events.
/// </summary>
public sealed record ModuleEngagementAggregate(
    string ModuleSlug,
    int UniqueStarts,
    int UniqueCompletions);

/// <summary>
/// Persistence contract for recording telemetry events and querying aggregated admin metrics.
/// </summary>
public interface IAnalyticsRepository
{
    Task AddAsync(AnalyticsEvent analyticsEvent, CancellationToken cancellationToken = default);

    Task<int> CountUniqueVisitorsSinceAsync(DateTime sinceUtc, CancellationToken cancellationToken = default);

    Task<int> CountTotalRegisteredUsersAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ModuleEngagementAggregate>> GetModuleEngagementAsync(
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
