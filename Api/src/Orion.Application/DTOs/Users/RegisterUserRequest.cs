namespace Orion.Application.DTOs.Users;

/// <summary>
/// Input payload for manual adventurer registration ("Spawn into Orion").
/// </summary>
public sealed record RegisterUserRequest(
    string FirstName,
    string? MiddleName,
    string Email,
    string? Username,
    string Password,
    string ConfirmPassword,
    bool AcceptTerms);
