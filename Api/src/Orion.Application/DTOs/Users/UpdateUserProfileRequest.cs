namespace Orion.Application.DTOs.Users;

/// <summary>
/// Input payload for updating editable adventurer profile details.
/// </summary>
public sealed record UpdateUserProfileRequest(
    string FirstName,
    string? MiddleName,
    string? Username,
    string? AvatarUrl);
