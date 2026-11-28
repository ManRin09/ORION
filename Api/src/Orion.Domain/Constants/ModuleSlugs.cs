namespace Orion.Domain.Constants;

/// <summary>
/// Canonical slugs for Orion's 4 core learning path modules.
/// </summary>
public static class ModuleSlugs
{
    public const string Python = "python";
    public const string HtmlCss = "html-css";
    public const string Js = "js";
    public const string Postgres = "postgres";

    public static readonly IReadOnlyList<string> All =
    [
        Python,
        HtmlCss,
        Js,
        Postgres
    ];

    private static readonly HashSet<string> ValidSet = new(All, StringComparer.OrdinalIgnoreCase);

    public static bool IsValid(string? slug) =>
        !string.IsNullOrWhiteSpace(slug) && ValidSet.Contains(slug.Trim());

    public static string Normalize(string slug) =>
        slug.Trim().ToLowerInvariant();
}
