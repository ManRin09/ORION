using FluentValidation;
using Orion.Application.DTOs.Auth;

namespace Orion.Application.Validators;

/// <summary>
/// Validates requests to update an active adventurer's Secret Spell (password).
/// </summary>
public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("Current Secret Spell is required.")
            .MaximumLength(PasswordRules.MaximumLength)
            .WithMessage($"Current Secret Spell cannot exceed {PasswordRules.MaximumLength} characters.");

        RuleFor(x => x.NewPassword)
            .ApplyCipherRunes()
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("New Secret Spell must differ from your current Secret Spell.");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty()
            .WithMessage("Confirm New Spell is required.")
            .Equal(x => x.NewPassword)
            .WithMessage("New Secret Spell and Confirm New Spell must match.");
    }
}
