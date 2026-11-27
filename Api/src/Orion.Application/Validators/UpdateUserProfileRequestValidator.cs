using FluentValidation;
using Orion.Application.DTOs.Users;

namespace Orion.Application.Validators;

/// <summary>
/// Validates adventurer profile update requests.
/// </summary>
public sealed class UpdateUserProfileRequestValidator : AbstractValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileRequestValidator()
    {
        RuleFor(x => x.FirstName).ApplyFirstNameRules();

        RuleFor(x => x.MiddleName)
            .ApplyMiddleNameRules()
            .When(x => !string.IsNullOrWhiteSpace(x.MiddleName));

        RuleFor(x => x.Username)
            .ApplyOptionalUsernameRules()
            .When(x => !string.IsNullOrWhiteSpace(x.Username));

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(500)
            .WithMessage("Avatar URL cannot exceed 500 characters.")
            .Must(BeValidHttpUrl)
            .WithMessage("Avatar URL must be a valid HTTP or HTTPS URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.AvatarUrl));
    }

    private static bool BeValidHttpUrl(string? url) =>
        Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
