using Orion.Domain.Entities;

namespace Orion.Application.Interfaces;

/// <summary>
/// Abstraction for issuing signed access tokens for authenticated adventurers.
/// </summary>
public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(User user, bool rememberTerminal);
}
