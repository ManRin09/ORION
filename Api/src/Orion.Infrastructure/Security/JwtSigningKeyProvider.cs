using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Orion.Infrastructure.Security;

/// <summary>
/// Provides a validated HMAC-SHA256 signing key from configuration or an ephemeral runtime fallback.
/// </summary>
public sealed class JwtSigningKeyProvider
{
    private const int MinimumKeyBytes = 32;

    public SymmetricSecurityKey SigningKey { get; }

    public JwtSigningKeyProvider(
        IOptions<JwtSettings> jwtOptions,
        ILogger<JwtSigningKeyProvider> logger)
    {
        var configuredKey = jwtOptions.Value.ResolveSigningKey();
        if (!string.IsNullOrWhiteSpace(configuredKey))
        {
            var keyBytes = Encoding.UTF8.GetBytes(configuredKey);
            if (keyBytes.Length < MinimumKeyBytes)
            {
                throw new InvalidOperationException(
                    $"Jwt:SecretKey (or Jwt:Key) must be at least {MinimumKeyBytes} bytes (256 bits) for HMAC-SHA256 signing.");
            }

            SigningKey = new SymmetricSecurityKey(keyBytes);
            return;
        }

        logger.LogWarning(
            "Jwt:SecretKey is not configured; using an ephemeral 256-bit signing key for this process lifetime. " +
            "Set Jwt__SecretKey in environment variables to persist issued tokens across restarts.");

        SigningKey = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(MinimumKeyBytes));
    }
}
