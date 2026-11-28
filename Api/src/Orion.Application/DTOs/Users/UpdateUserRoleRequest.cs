namespace Orion.Application.DTOs.Users;

/// <summary>
/// Input payload for updating an adventurer's authorization role ("User" or "Admin").
/// </summary>
public sealed record UpdateUserRoleRequest(string Role);
