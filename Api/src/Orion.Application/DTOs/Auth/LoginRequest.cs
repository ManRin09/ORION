namespace Orion.Application.DTOs.Auth;

/// <summary>
/// Input payload for authenticating an adventurer by username or email ("Slot 01: Load Game").
/// </summary>
public sealed record LoginRequest(
    string Identifier,
    string Password,
    bool RememberTerminal = false);
