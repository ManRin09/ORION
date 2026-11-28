namespace Orion.Application.DTOs.Analytics;

/// <summary>
/// Input payload for tracking a visitor or authenticated adventurer telemetry event.
/// </summary>
public sealed record TrackEventRequest(
    Guid VisitorId,
    string EventType,
    string? ModuleSlug,
    string Path);
