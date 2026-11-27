using Orion.Application.DTOs.Users;

namespace Orion.Application.DTOs.Auth;

/// <summary>
/// Authentication response containing a signed bearer token, expiration metadata, and adventurer profile.
/// </summary>
public sealed record AuthResponse(
    string AccessToken,
    string TokenType,
    DateTime ExpiresAtUtc,
    bool RememberTerminal,
    UserResponse User);
