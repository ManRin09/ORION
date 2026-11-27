namespace Orion.Application.DTOs.Auth;

/// <summary>
/// Input payload for updating an active adventurer's Secret Spell (password).
/// </summary>
public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword);
