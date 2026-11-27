using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using FluentValidation;
using Orion.Application.DTOs.Common;
using Orion.Application.DTOs.Users;
using Orion.Application.Interfaces;
using Orion.Domain.Entities;
using Orion.Domain.Exceptions;
using Orion.Domain.Interfaces;

namespace Orion.Application.Services;

/// <summary>
/// Orchestrates adventurer registration, profile management, and soft deletion use cases.
/// </summary>
public sealed partial class UserService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IValidator<RegisterUserRequest> registerValidator,
    IValidator<UpdateUserProfileRequest> updateProfileValidator) : IUserService
{
    private const int MaxGeneratedUsernameBaseLength = 38;
    private const int MaxUsernameCollisionAttempts = 8;

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumericRegex();

    public async Task<UserResponse> RegisterAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await registerValidator.ValidateAndThrowAsync(request, cancellationToken);

        var normalizedEmail = User.NormalizeEmail(request.Email);
        if (await userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken))
        {
            throw new EmailAlreadyExistsException(normalizedEmail);
        }

        var username = await ResolveUniqueUsernameAsync(request, cancellationToken);
        var passwordHash = passwordHasher.Hash(request.Password);
        var utcNow = DateTime.UtcNow;

        var user = User.CreateLocal(
            firstName: request.FirstName,
            middleName: request.MiddleName,
            email: normalizedEmail,
            username: username,
            passwordHash: passwordHash,
            acceptedTermsAtUtc: utcNow,
            utcNow: utcNow);

        await userRepository.AddAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

        return UserResponse.FromEntity(user);
    }

    public async Task<UserResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new UserNotFoundException(id);

        return UserResponse.FromEntity(user);
    }

    public async Task<PagedResponse<UserResponse>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var safePageNumber = Math.Max(1, pageNumber);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var (users, totalCount) = await userRepository.GetPagedAsync(
            safePageNumber,
            safePageSize,
            cancellationToken);

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)safePageSize);

        var items = users.Select(UserResponse.FromEntity).ToList();

        return new PagedResponse<UserResponse>(
            Items: items,
            PageNumber: safePageNumber,
            PageSize: safePageSize,
            TotalCount: totalCount,
            TotalPages: totalPages);
    }

    public async Task<UserResponse> UpdateProfileAsync(
        Guid id,
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await updateProfileValidator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new UserNotFoundException(id);

        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            var normalizedUsername = User.NormalizeUsername(request.Username);
            if (!string.Equals(normalizedUsername, user.Username, StringComparison.Ordinal) &&
                await userRepository.ExistsByUsernameAsync(normalizedUsername, cancellationToken))
            {
                throw new UsernameAlreadyExistsException(normalizedUsername);
            }
        }

        user.UpdateProfile(
            firstName: request.FirstName,
            middleName: request.MiddleName,
            username: request.Username,
            avatarUrl: request.AvatarUrl);

        await userRepository.SaveChangesAsync(cancellationToken);

        return UserResponse.FromEntity(user);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new UserNotFoundException(id);

        user.Deactivate();
        await userRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> ResolveUniqueUsernameAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            var requestedUsername = User.NormalizeUsername(request.Username);
            if (await userRepository.ExistsByUsernameAsync(requestedUsername, cancellationToken))
            {
                throw new UsernameAlreadyExistsException(requestedUsername);
            }

            return requestedUsername;
        }

        var baseSlug = BuildUsernameBaseSlug(request.FirstName, request.MiddleName, request.Email);
        if (!await userRepository.ExistsByUsernameAsync(baseSlug, cancellationToken))
        {
            return baseSlug;
        }

        for (var attempt = 0; attempt < MaxUsernameCollisionAttempts; attempt++)
        {
            var suffix = RandomNumberGenerator.GetHexString(4, lowercase: true);
            var candidate = $"{baseSlug}_{suffix}";
            if (!await userRepository.ExistsByUsernameAsync(candidate, cancellationToken))
            {
                return candidate;
            }
        }

        return $"adventurer_{Guid.NewGuid().ToString("N")[..12]}";
    }

    private static string BuildUsernameBaseSlug(string firstName, string? middleName, string email)
    {
        var rawIdentity = string.IsNullOrWhiteSpace(middleName)
            ? firstName
            : $"{firstName}_{middleName}";

        var slug = SanitizeSlug(rawIdentity);
        if (slug.Length < 3)
        {
            var emailLocalPart = email.Split('@', 2)[0];
            slug = SanitizeSlug(emailLocalPart);
        }

        if (slug.Length < 3)
        {
            slug = $"adventurer_{slug}".TrimEnd('_');
        }

        return slug.Length > MaxGeneratedUsernameBaseLength
            ? slug[..MaxGeneratedUsernameBaseLength].TrimEnd('_')
            : slug;
    }

    private static string SanitizeSlug(string input)
    {
        var decomposed = input.Normalize(NormalizationForm.FormD);
        var asciiBuilder = new StringBuilder(decomposed.Length);

        foreach (var ch in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                asciiBuilder.Append(char.ToLowerInvariant(ch));
            }
        }

        return NonAlphanumericRegex()
            .Replace(asciiBuilder.ToString(), "_")
            .Trim('_');
    }
}
