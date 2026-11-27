using FluentValidation;
using Orion.Application.DTOs.Users;

namespace Orion.Application.Validators;

/// <summary>
/// Validates manual adventurer registration requests ("Spawn into Orion").
/// </summary>
public sealed class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.FirstName).ApplyFirstNameRules();

        RuleFor(x => x.MiddleName)
            .ApplyMiddleNameRules()
            .When(x => !string.IsNullOrWhiteSpace(x.MiddleName));

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Scroll Address (email) is required.")
            .MaximumLength(255)
            .WithMessage("Email cannot exceed 255 characters.")
            .EmailAddress()
            .WithMessage("Scroll Address must be a valid email address.");

        RuleFor(x => x.Username)
            .ApplyOptionalUsernameRules()
            .When(x => !string.IsNullOrWhiteSpace(x.Username));

        RuleFor(x => x.Password).ApplyCipherRunes();

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Confirm Spell is required.")
            .Equal(x => x.Password)
            .WithMessage("Secret Spell and Confirm Spell must match.");

        RuleFor(x => x.AcceptTerms)
            .Equal(true)
            .WithMessage("You must accept the Orion Adventurer Code of Conduct & Terms of Space.");
    }
}
