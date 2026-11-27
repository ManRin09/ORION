using Orion.Domain.Entities;

namespace Orion.Application.DTOs.Users;

/// <summary>
/// Public representation of an Orion adventurer account (excludes sensitive credentials).
/// </summary>
public sealed record UserResponse(
    Guid Id,
    string FirstName,
    string? MiddleName,
    string Email,
    string Username,
    string? AvatarUrl,
    string AuthProvider,
    string Role,
    string PlanType,
    int TotalXp,
    int CurrentLevel,
    int CurrentStreak,
    int LongestStreak,
    int Gems,
    bool IsActive,
    DateTime AcceptedTermsAt,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static UserResponse FromEntity(User user) =>
        new(
            Id: user.Id,
            FirstName: user.FirstName,
            MiddleName: user.MiddleName,
            Email: user.Email,
            Username: user.Username,
            AvatarUrl: user.AvatarUrl,
            AuthProvider: user.AuthProvider.ToString(),
            Role: user.Role.ToString(),
            PlanType: user.PlanType.ToString(),
            TotalXp: user.TotalXp,
            CurrentLevel: user.CurrentLevel,
            CurrentStreak: user.CurrentStreak,
            LongestStreak: user.LongestStreak,
            Gems: user.Gems,
            IsActive: user.IsActive,
            AcceptedTermsAt: user.AcceptedTermsAt,
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt);
}
