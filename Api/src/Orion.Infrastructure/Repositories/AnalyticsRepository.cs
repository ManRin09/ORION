using Microsoft.EntityFrameworkCore;
using Orion.Domain.Constants;
using Orion.Domain.Entities;
using Orion.Domain.Interfaces;
using Orion.Infrastructure.Persistence;

namespace Orion.Infrastructure.Repositories;

/// <summary>
/// EF Core repository implementation for persisting <see cref="AnalyticsEvent"/> telemetry and computing admin metrics.
/// </summary>
public sealed class AnalyticsRepository(AppDbContext dbContext) : IAnalyticsRepository
{
    public async Task AddAsync(AnalyticsEvent analyticsEvent, CancellationToken cancellationToken = default)
    {
        await dbContext.AnalyticsEvents.AddAsync(analyticsEvent, cancellationToken);
    }

    public Task<int> CountUniqueVisitorsSinceAsync(
        DateTime sinceUtc,
        CancellationToken cancellationToken = default) =>
        dbContext.AnalyticsEvents
            .AsNoTracking()
            .Where(e => e.CreatedAt >= sinceUtc)
            .Select(e => e.VisitorId)
            .Distinct()
            .CountAsync(cancellationToken);

    public Task<int> CountTotalRegisteredUsersAsync(CancellationToken cancellationToken = default) =>
        dbContext.Users
            .AsNoTracking()
            .CountAsync(cancellationToken);

    public async Task<IReadOnlyList<ModuleEngagementAggregate>> GetModuleEngagementAsync(
        CancellationToken cancellationToken = default)
    {
        var groupedPairs = await dbContext.AnalyticsEvents
            .AsNoTracking()
            .Where(e =>
                e.ModuleSlug != null &&
                (e.EventType == AnalyticsEventTypes.ModuleStart ||
                 e.EventType == AnalyticsEventTypes.ModuleComplete))
            .Select(e => new { ModuleSlug = e.ModuleSlug!, e.EventType, e.VisitorId })
            .Distinct()
            .GroupBy(e => new { e.ModuleSlug, e.EventType })
            .Select(g => new
            {
                g.Key.ModuleSlug,
                g.Key.EventType,
                UniqueVisitors = g.Count()
            })
            .ToListAsync(cancellationToken);

        return groupedPairs
            .GroupBy(x => x.ModuleSlug, StringComparer.OrdinalIgnoreCase)
            .Select(g => new ModuleEngagementAggregate(
                ModuleSlug: g.Key.ToLowerInvariant(),
                UniqueStarts: g
                    .Where(x => x.EventType == AnalyticsEventTypes.ModuleStart)
                    .Sum(x => x.UniqueVisitors),
                UniqueCompletions: g
                    .Where(x => x.EventType == AnalyticsEventTypes.ModuleComplete)
                    .Sum(x => x.UniqueVisitors)))
            .ToList();
    }

    public async Task<IReadOnlyList<User>> GetAllActiveUsersOrderedAsync(
        CancellationToken cancellationToken = default) =>
        await dbContext.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .ThenBy(u => u.Username)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<UserModuleActivityAggregate>> GetUserModuleActivityAsync(
        CancellationToken cancellationToken = default)
    {
        var userEvents = await dbContext.AnalyticsEvents
            .AsNoTracking()
            .Where(e => e.UserId != null)
            .Select(e => new
            {
                UserId = e.UserId!.Value,
                e.EventType,
                e.ModuleSlug,
                e.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return userEvents
            .GroupBy(e => e.UserId)
            .Select(group =>
            {
                var started = group
                    .Where(e =>
                        !string.IsNullOrWhiteSpace(e.ModuleSlug) &&
                        (e.EventType == AnalyticsEventTypes.ModuleStart ||
                         e.EventType == AnalyticsEventTypes.ModuleComplete))
                    .Select(e => e.ModuleSlug!.ToLowerInvariant())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(slug => slug, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var completed = group
                    .Where(e =>
                        !string.IsNullOrWhiteSpace(e.ModuleSlug) &&
                        e.EventType == AnalyticsEventTypes.ModuleComplete)
                    .Select(e => e.ModuleSlug!.ToLowerInvariant())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(slug => slug, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var lastActivity = group.Max(e => (DateTime?)e.CreatedAt);

                return new UserModuleActivityAggregate(
                    UserId: group.Key,
                    StartedModuleSlugs: started,
                    CompletedModuleSlugs: completed,
                    LastActivityAtUtc: lastActivity);
            })
            .ToList();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
