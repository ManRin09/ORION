using FluentValidation;
using Orion.Application.DTOs.Auth;
using Orion.Application.DTOs.Users;
using Orion.Application.Interfaces;
using Orion.Domain.Exceptions;
using Orion.Domain.Interfaces;

namespace Orion.Application.Services;

/// <summary>
/// Orchestrates adventurer authentication, active session retrieval, and password updates.
/// </summary>
public sealed class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IValidator<LoginRequest> loginValidator,
    IValidator<ChangePasswordRequest> changePasswordValidator) : IAuthService
{
    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await loginValidator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await userRepository.GetByIdentifierAsync(request.Identifier, cancellationToken);
        if (user is null ||
            string.IsNullOrWhiteSpace(user.PasswordHash) ||
            !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        var (accessToken, expiresAtUtc) = tokenService.GenerateAccessToken(user, request.RememberTerminal);

        return new AuthResponse(
            AccessToken: accessToken,
            TokenType: "Bearer",
            ExpiresAtUtc: expiresAtUtc,
            RememberTerminal: request.RememberTerminal,
            User: UserResponse.FromEntity(user));
    }

    public async Task<UserResponse> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new UserNotFoundException(userId);

        return UserResponse.FromEntity(user);
    }

    public async Task ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await changePasswordValidator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new UserNotFoundException(userId);

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            throw new DomainException(
                "This adventurer account uses an external login provider and does not have a local password.");
        }

        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new InvalidCredentialsException("Current Secret Spell is incorrect.");
        }

        var newPasswordHash = passwordHasher.Hash(request.NewPassword);
        user.ChangePassword(newPasswordHash);

        await userRepository.SaveChangesAsync(cancellationToken);
    }
}
