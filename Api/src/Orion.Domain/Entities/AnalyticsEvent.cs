using Orion.Domain.Constants;
using Orion.Domain.Exceptions;

namespace Orion.Domain.Entities;

/// <summary>
/// Immutable telemetry event capturing page views and module progression by visitor/user.
/// </summary>
public sealed class AnalyticsEvent
{
    private AnalyticsEvent()
    {
    }

    private AnalyticsEvent(
        Guid visitorId,
        Guid? userId,
        string eventType,
        string? moduleSlug,
        string path,
        string? userAgent,
        DateTime createdAtUtc)
    {
        VisitorId = visitorId;
        UserId = userId;
        EventType = eventType;
        ModuleSlug = moduleSlug;
        Path = path;
        UserAgent = userAgent;
        CreatedAt = createdAtUtc;
    }

    public long Id { get; private set; }
    public Guid VisitorId { get; private set; }
    public Guid? UserId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string? ModuleSlug { get; private set; }
    public string Path { get; private set; } = string.Empty;
    public string? UserAgent { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static AnalyticsEvent Create(
        Guid visitorId,
        Guid? userId,
        string eventType,
        string? moduleSlug,
        string path,
        string? userAgent,
        DateTime? utcNow = null)
    {
        if (visitorId == Guid.Empty)
        {
            throw new DomainException("VisitorId cannot be empty.");
        }

        if (!AnalyticsEventTypes.IsValid(eventType))
        {
            throw new DomainException($"Unsupported analytics event type '{eventType}'.");
        }

        var normalizedEventType = AnalyticsEventTypes.Normalize(eventType);

        string? normalizedSlug = null;
        if (!string.IsNullOrWhiteSpace(moduleSlug))
        {
            if (!ModuleSlugs.IsValid(moduleSlug))
            {
                throw new DomainException($"Unsupported module slug '{moduleSlug}'.");
            }

            normalizedSlug = ModuleSlugs.Normalize(moduleSlug);
        }
        else if (AnalyticsEventTypes.RequiresModuleSlug(normalizedEventType))
        {
            throw new DomainException($"ModuleSlug is required for event type '{normalizedEventType}'.");
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new DomainException("Path cannot be empty.");
        }

        var normalizedPath = path.Trim();
        if (normalizedPath.Length > 255)
        {
            normalizedPath = normalizedPath[..255];
        }

        string? normalizedUserAgent = null;
        if (!string.IsNullOrWhiteSpace(userAgent))
        {
            var trimmedUa = userAgent.Trim();
            normalizedUserAgent = trimmedUa.Length > 512 ? trimmedUa[..512] : trimmedUa;
        }

        var createdAt = utcNow ?? DateTime.UtcNow;
        var createdAtUtc = createdAt.Kind == DateTimeKind.Utc ? createdAt : createdAt.ToUniversalTime();

        return new AnalyticsEvent(
            visitorId: visitorId,
            userId: userId,
            eventType: normalizedEventType,
            moduleSlug: normalizedSlug,
            path: normalizedPath,
            userAgent: normalizedUserAgent,
            createdAtUtc: createdAtUtc);
    }
}
