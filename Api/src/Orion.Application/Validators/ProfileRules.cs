using System.Text.RegularExpressions;
using FluentValidation;

namespace Orion.Application.Validators;

/// <summary>
/// Shared validation rules for adventurer identity fields across registration and profile updates.
/// </summary>
internal static partial class ProfileRules
{
    [GeneratedRegex("^[a-zA-Z0-9_]+$")]
    private static partial Regex UsernameFormatRegex();

    public static IRuleBuilderOptions<T, string> ApplyFirstNameRules<T>(
        this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty()
            .WithMessage("First name is required.")
            .MinimumLength(2)
            .WithMessage("First name must be at least 2 characters.")
            .MaximumLength(100)
            .WithMessage("First name cannot exceed 100 characters.");

    public static IRuleBuilderOptions<T, string?> ApplyMiddleNameRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder
            .MaximumLength(100)
            .WithMessage("Middle / Title cannot exceed 100 characters.");

    public static IRuleBuilderOptions<T, string?> ApplyOptionalUsernameRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters.")
            .MaximumLength(50)
            .WithMessage("Username cannot exceed 50 characters.")
            .Matches(UsernameFormatRegex())
            .WithMessage("Username may only contain letters, numbers, and underscores (_).");
}
