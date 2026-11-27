using FluentValidation;
using Orion.Application.DTOs.Auth;

namespace Orion.Application.Validators;

/// <summary>
/// Validates adventurer login requests using username or email identifier.
/// </summary>
public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public const int MinimumIdentifierLength = 3;
    public const int MaximumIdentifierLength = 255;
    public const int MinimumLoginPasswordLength = 6;

    public LoginRequestValidator()
    {
        RuleFor(x => x.Identifier)
            .NotEmpty()
            .WithMessage("Adventurer Name or Email is required.")
            .Must(identifier => identifier is not null && identifier.Trim().Length >= MinimumIdentifierLength)
            .WithMessage($"Adventurer Name or Email must be at least {MinimumIdentifierLength} characters.")
            .Must(identifier => identifier is null || identifier.Trim().Length <= MaximumIdentifierLength)
            .WithMessage($"Adventurer Name or Email cannot exceed {MaximumIdentifierLength} characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Secret Spell (password) is required.")
            .MinimumLength(MinimumLoginPasswordLength)
            .WithMessage($"Secret Spell must be at least {MinimumLoginPasswordLength} characters long.")
            .MaximumLength(PasswordRules.MaximumLength)
            .WithMessage($"Secret Spell cannot exceed {PasswordRules.MaximumLength} characters.");
    }
}
