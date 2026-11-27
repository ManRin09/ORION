using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Orion.Domain.Entities;
using Orion.Infrastructure.Security;

namespace Orion.UnitTests.Security;

public sealed class JwtTokenServiceTests
{
    private static User CreateTestUser() =>
        User.CreateLocal(
            firstName: "Arthur",
            middleName: "Pendragon",
            email: "arthur@orion.realm",
            username: "arthur_pendragon",
            passwordHash: "hashed",
            acceptedTermsAtUtc: DateTime.UtcNow);

    [Fact]
    public void GenerateAccessToken_WithConfiguredKey_EmbedsExpectedClaimsAndRespectsRememberTerminal()
    {
        var options = Options.Create(new JwtSettings
        {
            Issuer = "Orion.Api",
            Audience = "Orion.Frontend",
            Key = "orion-super-secret-signing-key-2026-min-32-bytes!",
            AccessTokenExpirationMinutes = 60,
            RememberMeExpirationDays = 14
        });
        var keyProvider = new JwtSigningKeyProvider(options, NullLogger<JwtSigningKeyProvider>.Instance);
        var service = new JwtTokenService(options, keyProvider);
        var user = CreateTestUser();

        var (shortToken, shortExpiry) = service.GenerateAccessToken(user, rememberTerminal: false);
        var (longToken, longExpiry) = service.GenerateAccessToken(user, rememberTerminal: true);

        Assert.True(longExpiry > shortExpiry.AddDays(10));

        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(shortToken);
        Assert.Equal("Orion.Api", parsed.Issuer);
        Assert.Contains("Orion.Frontend", parsed.Audiences);
        Assert.Equal(user.Id.ToString(), parsed.Subject);
        Assert.Contains(parsed.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        Assert.Contains(parsed.Claims, c => c.Type == JwtRegisteredClaimNames.Name && c.Value == "Arthur Pendragon");
        Assert.Contains(parsed.Claims, c => c.Type == JwtRegisteredClaimNames.UniqueName && c.Value == user.Username);
        Assert.Contains(parsed.Claims, c => c.Type == "role" && c.Value == "User");
        Assert.Contains(parsed.Claims, c => c.Type == ClaimTypes.Role && c.Value == "User");
    }

    [Fact]
    public void GenerateAccessToken_WithRenderSecretKeyAndExpirationMinutes_UsesConfiguredSettings()
    {
        var options = Options.Create(new JwtSettings
        {
            Issuer = "Orion.Api",
            Audience = "Orion.Frontend",
            SecretKey = "render-production-secret-key-at-least-32-bytes!",
            ExpirationMinutes = 45
        });
        var keyProvider = new JwtSigningKeyProvider(options, NullLogger<JwtSigningKeyProvider>.Instance);
        var service = new JwtTokenService(options, keyProvider);
        var user = CreateTestUser();

        var before = DateTime.UtcNow;
        var (token, expiresAtUtc) = service.GenerateAccessToken(user, rememberTerminal: false);

        Assert.InRange(expiresAtUtc, before.AddMinutes(44), DateTime.UtcNow.AddMinutes(46));

        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal(user.Id.ToString(), parsed.Subject);
        Assert.Contains(parsed.Claims, c => c.Type == JwtRegisteredClaimNames.Name && c.Value == "Arthur Pendragon");
        Assert.Contains(parsed.Claims, c => c.Type == "role" && c.Value == "User");
    }

    [Fact]
    public void SigningKeyProvider_WhenKeyIsMissing_GeneratesEphemeral256BitKeyWithoutThrowing()
    {
        var options = Options.Create(new JwtSettings
        {
            Issuer = "Orion.Api",
            Audience = "Orion.Frontend",
            Key = null
        });

        var keyProvider = new JwtSigningKeyProvider(options, NullLogger<JwtSigningKeyProvider>.Instance);

        Assert.NotNull(keyProvider.SigningKey);
        Assert.True(keyProvider.SigningKey.KeySize >= 256);
    }

    [Fact]
    public void SigningKeyProvider_WhenConfiguredKeyIsTooShort_ThrowsInvalidOperationException()
    {
        var options = Options.Create(new JwtSettings
        {
            Key = "too-short"
        });

        Assert.Throws<InvalidOperationException>(() =>
            new JwtSigningKeyProvider(options, NullLogger<JwtSigningKeyProvider>.Instance));
    }
}
