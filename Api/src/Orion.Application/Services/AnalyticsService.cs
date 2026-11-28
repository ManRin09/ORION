using FluentValidation;
using Orion.Application.DTOs.Analytics;
using Orion.Application.Interfaces;
using Orion.Domain.Entities;
using Orion.Domain.Interfaces;

namespace Orion.Application.Services;

/// <summary>
/// Orchestrates visitor telemetry tracking and aggregates 30-day/24-hour and per-module metrics for Admins.
/// </summary>
public sealed class AnalyticsService(
    IAnalyticsRepository analyticsRepository,
    ILearningModuleRepository learningModuleRepository,
    IValidator<TrackEventRequest> trackEventValidator) : IAnalyticsService
{
    public async Task TrackEventAsync(
        TrackEventRequest request,
        Guid? userId = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await trackEventValidator.ValidateAndThrowAsync(request, cancellationToken);

        var analyticsEvent = AnalyticsEvent.Create(
            visitorId: request.VisitorId,
            userId: userId,
            eventType: request.EventType,
            moduleSlug: request.ModuleSlug,
            path: request.Path,
            userAgent: userAgent,
            utcNow: DateTime.UtcNow);

        await analyticsRepository.AddAsync(analyticsEvent, cancellationToken);
        await analyticsRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<AdminAnalyticsSummaryDto> GetAdminSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var last30DaysUtc = utcNow.AddDays(-30);
        var last24HoursUtc = utcNow.AddHours(-24);

        var uniqueVisitors30d = await analyticsRepository.CountUniqueVisitorsSinceAsync(
            last30DaysUtc,
            cancellationToken);
        var activeVisitors24h = await analyticsRepository.CountUniqueVisitorsSinceAsync(
            last24HoursUtc,
            cancellationToken);
        var totalUsers = await analyticsRepository.CountTotalRegisteredUsersAsync(cancellationToken);

        var modules = await learningModuleRepository.GetActiveOrderedAsync(cancellationToken);
        var engagement = await analyticsRepository.GetModuleEngagementAsync(cancellationToken);
        var engagementBySlug = engagement.ToDictionary(
            e => e.ModuleSlug,
            StringComparer.OrdinalIgnoreCase);

        var moduleMetrics = modules
            .OrderBy(m => m.OrderIndex)
            .Select(module =>
            {
                var hasMetrics = engagementBySlug.TryGetValue(module.Slug, out var counts);
                var starts = hasMetrics ? counts!.UniqueStarts : 0;
                var completions = hasMetrics ? counts!.UniqueCompletions : 0;
                var rate = starts > 0
                    ? Math.Round(Math.Min(100.0, (completions / (double)starts) * 100.0), 2)
                    : 0.0;

                return new ModuleAnalyticsDto(
                    Slug: module.Slug,
                    Title: module.Title,
                    OrderIndex: module.OrderIndex,
                    UniqueStarts: starts,
                    UniqueCompletions: completions,
                    CompletionRatePercent: rate);
            })
            .ToList();

        return new AdminAnalyticsSummaryDto(
            UniqueVisitorsLast30Days: uniqueVisitors30d,
            ActiveVisitorsLast24Hours: activeVisitors24h,
            TotalRegisteredUsers: totalUsers,
            Modules: moduleMetrics);
    }
}
