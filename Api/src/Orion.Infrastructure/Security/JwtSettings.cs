namespace Orion.Infrastructure.Security;

/// <summary>
/// Configuration options controlling JWT issuance and validation.
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";
    public const string DefaultIssuer = "Orion.Api";
    public const string DefaultAudience = "Orion.Frontend";
    public const int DefaultAccessTokenExpirationMinutes = 120;
    public const int DefaultRememberMeExpirationDays = 7;

    public string Issuer { get; set; } = DefaultIssuer;
    public string Audience { get; set; } = DefaultAudience;
    public string? Key { get; set; }
    public int AccessTokenExpirationMinutes { get; set; } = DefaultAccessTokenExpirationMinutes;
    public int RememberMeExpirationDays { get; set; } = DefaultRememberMeExpirationDays;
}
