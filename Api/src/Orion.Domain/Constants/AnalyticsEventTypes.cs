namespace Orion.Domain.Constants;

/// <summary>
/// Supported telemetry event types tracked by Orion's internal analytics engine.
/// </summary>
public static class AnalyticsEventTypes
{
    public const string PageView = "page_view";
    public const string ModuleStart = "module_start";
    public const string ModuleComplete = "module_complete";

    public static readonly IReadOnlyList<string> All =
    [
        PageView,
        ModuleStart,
        ModuleComplete
    ];

    private static readonly HashSet<string> ValidSet = new(All, StringComparer.OrdinalIgnoreCase);

    public static bool IsValid(string? eventType) =>
        !string.IsNullOrWhiteSpace(eventType) && ValidSet.Contains(eventType.Trim());

    public static bool RequiresModuleSlug(string? eventType) =>
        !string.IsNullOrWhiteSpace(eventType) &&
        (string.Equals(eventType.Trim(), ModuleStart, StringComparison.OrdinalIgnoreCase) ||
         string.Equals(eventType.Trim(), ModuleComplete, StringComparison.OrdinalIgnoreCase));

    public static string Normalize(string eventType) =>
        eventType.Trim().ToLowerInvariant();
}
