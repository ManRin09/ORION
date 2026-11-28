namespace Orion.Infrastructure.Security;

/// <summary>
/// Configuration options controlling JWT issuance and validation.
/// Supports Render production keys (<c>Jwt__SecretKey</c>, <c>Jwt__ExpirationMinutes</c>)
/// as well as legacy fallback keys (<c>Jwt__Key</c>, <c>Jwt__AccessTokenExpirationMinutes</c>).
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";
    public const string DefaultIssuer = "Orion.Api";
    public const string DefaultAudience = "Orion.Frontend";
    public const int DefaultExpirationMinutes = 120;
    public const int DefaultAccessTokenExpirationMinutes = DefaultExpirationMinutes;
    public const int DefaultRememberMeExpirationDays = 7;

    public string Issuer { get; set; } = DefaultIssuer;
    public string Audience { get; set; } = DefaultAudience;
    public string? SecretKey { get; set; }
    public string? Key { get; set; }
    public int ExpirationMinutes { get; set; } = DefaultExpirationMinutes;

    public int AccessTokenExpirationMinutes
    {
        get => ExpirationMinutes;
        set => ExpirationMinutes = value;
    }

    public int RememberMeExpirationDays { get; set; } = DefaultRememberMeExpirationDays;

    public string? ResolveSigningKey() =>
        !string.IsNullOrWhiteSpace(SecretKey) ? SecretKey : Key;
}
