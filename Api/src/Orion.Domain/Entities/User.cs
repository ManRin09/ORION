using Orion.Domain.Enums;
using Orion.Domain.Exceptions;

namespace Orion.Domain.Entities;

/// <summary>
/// Core adventurer aggregate representing identity, authentication, gamification state, and audit metadata.
/// </summary>
public sealed class User
{
    private User()
    {
    }

    private User(
        Guid id,
        string firstName,
        string? middleName,
        string email,
        string username,
        string? avatarUrl,
        string? passwordHash,
        AuthProvider authProvider,
        string? gitHubId,
        DateTime acceptedTermsAtUtc,
        DateTime createdAtUtc)
    {
        Id = id;
        FirstName = firstName;
        MiddleName = middleName;
        Email = email;
        Username = username;
        AvatarUrl = avatarUrl;
        PasswordHash = passwordHash;
        AuthProvider = authProvider;
        GitHubId = gitHubId;
        Role = UserRole.User;
        PlanType = PlanType.Free;
        AcceptedTermsAt = acceptedTermsAtUtc;

        TotalXp = 0;
        CurrentLevel = 1;
        CurrentStreak = 0;
        LongestStreak = 0;
        Gems = 0;

        IsActive = true;
        CreatedAt = createdAtUtc;
        UpdatedAt = createdAtUtc;
    }

    // Identity & Profile
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string? MiddleName { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string? AvatarUrl { get; private set; }

    // Security & Authentication
    public string? PasswordHash { get; private set; }
    public AuthProvider AuthProvider { get; private set; }
    public string? GitHubId { get; private set; }
    public UserRole Role { get; private set; }
    public PlanType PlanType { get; private set; }
    public DateTime AcceptedTermsAt { get; private set; }

    // Gamification State
    public int TotalXp { get; private set; }
    public int CurrentLevel { get; private set; }
    public int CurrentStreak { get; private set; }
    public int LongestStreak { get; private set; }
    public int Gems { get; private set; }

    // Audit & Soft Delete
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Creates a manually registered local adventurer with initial Level 1 RPG stats.
    /// </summary>
    public static User CreateLocal(
        string firstName,
        string? middleName,
        string email,
        string username,
        string passwordHash,
        DateTime acceptedTermsAtUtc,
        DateTime? utcNow = null)
    {
        var validPasswordHash = RequireNonEmpty(passwordHash, nameof(passwordHash));
        return CreateInternal(
            firstName: firstName,
            middleName: middleName,
            email: email,
            username: username,
            avatarUrl: null,
            passwordHash: validPasswordHash,
            authProvider: AuthProvider.Local,
            gitHubId: null,
            acceptedTermsAtUtc: acceptedTermsAtUtc,
            utcNow: utcNow);
    }

    /// <summary>
    /// Creates an adventurer authenticated through GitHub OAuth without requiring a local password hash.
    /// </summary>
    public static User CreateGitHub(
        string firstName,
        string? middleName,
        string email,
        string username,
        string gitHubId,
        string? avatarUrl,
        DateTime acceptedTermsAtUtc,
        DateTime? utcNow = null)
    {
        var validGitHubId = RequireNonEmpty(gitHubId, nameof(gitHubId));
        return CreateInternal(
            firstName: firstName,
            middleName: middleName,
            email: email,
            username: username,
            avatarUrl: NormalizeOptional(avatarUrl),
            passwordHash: null,
            authProvider: AuthProvider.GitHub,
            gitHubId: validGitHubId,
            acceptedTermsAtUtc: acceptedTermsAtUtc,
            utcNow: utcNow);
    }

    /// <summary>
    /// Updates editable profile fields and refreshes the UTC modification timestamp.
    /// </summary>
    public void UpdateProfile(
        string firstName,
        string? middleName,
        string? username,
        string? avatarUrl,
        DateTime? utcNow = null)
    {
        EnsureActive();

        FirstName = RequireNonEmpty(firstName, nameof(firstName));
        MiddleName = NormalizeOptional(middleName);
        if (!string.IsNullOrWhiteSpace(username))
        {
            Username = NormalizeUsername(username);
        }

        AvatarUrl = NormalizeOptional(avatarUrl);
        UpdatedAt = EnsureUtc(utcNow ?? DateTime.UtcNow);
    }

    /// <summary>
    /// Replaces the stored password hash for an active adventurer and refreshes the UTC modification timestamp.
    /// </summary>
    public void ChangePassword(string newPasswordHash, DateTime? utcNow = null)
    {
        EnsureActive();

        PasswordHash = RequireNonEmpty(newPasswordHash, nameof(newPasswordHash));
        UpdatedAt = EnsureUtc(utcNow ?? DateTime.UtcNow);
    }

    /// <summary>
    /// Links an external GitHub identifier to an existing active account.
    /// </summary>
    public void LinkGitHub(string gitHubId, string? avatarUrl = null, DateTime? utcNow = null)
    {
        EnsureActive();

        GitHubId = RequireNonEmpty(gitHubId, nameof(gitHubId));
        if (string.IsNullOrWhiteSpace(AvatarUrl) && !string.IsNullOrWhiteSpace(avatarUrl))
        {
            AvatarUrl = avatarUrl.Trim();
        }

        UpdatedAt = EnsureUtc(utcNow ?? DateTime.UtcNow);
    }

    /// <summary>
    /// Soft-deletes the adventurer account while preserving audit timestamps.
    /// </summary>
    public void Deactivate(DateTime? utcNow = null)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        UpdatedAt = EnsureUtc(utcNow ?? DateTime.UtcNow);
    }

    public static string NormalizeEmail(string email) =>
        RequireNonEmpty(email, nameof(email)).ToLowerInvariant();

    public static string NormalizeUsername(string username) =>
        RequireNonEmpty(username, nameof(username)).ToLowerInvariant();

    private static User CreateInternal(
        string firstName,
        string? middleName,
        string email,
        string username,
        string? avatarUrl,
        string? passwordHash,
        AuthProvider authProvider,
        string? gitHubId,
        DateTime acceptedTermsAtUtc,
        DateTime? utcNow)
    {
        if (acceptedTermsAtUtc == default)
        {
            throw new DomainException("Acceptance timestamp for terms of service is required.");
        }

        var createdAt = EnsureUtc(utcNow ?? DateTime.UtcNow);

        return new User(
            id: Guid.NewGuid(),
            firstName: RequireNonEmpty(firstName, nameof(firstName)),
            middleName: NormalizeOptional(middleName),
            email: NormalizeEmail(email),
            username: NormalizeUsername(username),
            avatarUrl: avatarUrl,
            passwordHash: passwordHash,
            authProvider: authProvider,
            gitHubId: gitHubId,
            acceptedTermsAtUtc: EnsureUtc(acceptedTermsAtUtc),
            createdAtUtc: createdAt);
    }

    private void EnsureActive()
    {
        if (!IsActive)
        {
            throw new DomainException("Cannot modify an inactive adventurer account.");
        }
    }

    private static string RequireNonEmpty(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{paramName} cannot be empty.");
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DateTime EnsureUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
}
