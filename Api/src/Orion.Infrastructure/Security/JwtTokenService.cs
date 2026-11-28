using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Orion.Application.Interfaces;
using Orion.Domain.Entities;

namespace Orion.Infrastructure.Security;

/// <summary>
/// Issues signed HMAC-SHA256 JWT access tokens for authenticated Orion adventurers.
/// </summary>
public sealed class JwtTokenService(
    IOptions<JwtSettings> jwtOptions,
    JwtSigningKeyProvider signingKeyProvider) : ITokenService
{
    public (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(User user, bool rememberTerminal)
    {
        ArgumentNullException.ThrowIfNull(user);

        var settings = jwtOptions.Value;
        var utcNow = DateTime.UtcNow;
        var expiresAtUtc = rememberTerminal
            ? utcNow.AddDays(Math.Max(1, settings.RememberMeExpirationDays))
            : utcNow.AddMinutes(Math.Max(5, settings.ExpirationMinutes));

        var displayName = string.IsNullOrWhiteSpace(user.MiddleName)
            ? user.FirstName
            : $"{user.FirstName} {user.MiddleName}";

        var roleName = user.Role.ToString();

        var claims = new Claim[]
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, displayName),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new("role", roleName),
            new(ClaimTypes.Role, roleName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credentials = new SigningCredentials(
            signingKeyProvider.SigningKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            notBefore: utcNow,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        var serializedToken = new JwtSecurityTokenHandler().WriteToken(token);
        return (serializedToken, expiresAtUtc);
    }
}
