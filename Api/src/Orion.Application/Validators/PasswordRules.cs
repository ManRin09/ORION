using System.Text.RegularExpressions;
using FluentValidation;

namespace Orion.Application.Validators;

/// <summary>
/// Shared password complexity rules enforcing Orion's 4 "Cipher Integrity" runes.
/// </summary>
public static partial class PasswordRules
{
    public const int MinimumLength = 8;
    public const int MaximumLength = 128;

    [GeneratedRegex("[A-Z]")]
    private static partial Regex UppercaseRegex();

    [GeneratedRegex("[a-z]")]
    private static partial Regex LowercaseRegex();

    [GeneratedRegex("""[!@#$%^&*(),.?":{}|<>_\-+=\[\]\\/~`';]""")]
    private static partial Regex SpecialGlyphRegex();

    public static IRuleBuilderOptions<T, string> ApplyCipherRunes<T>(
        this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty()
            .WithMessage("Secret Spell (password) is required.")
            .MinimumLength(MinimumLength)
            .WithMessage($"Secret Spell must contain at least {MinimumLength} runes (characters).")
            .MaximumLength(MaximumLength)
            .WithMessage($"Secret Spell cannot exceed {MaximumLength} characters.")
            .Matches(UppercaseRegex())
            .WithMessage("Secret Spell must include an Elder Capital Rune (A-Z).")
            .Matches(LowercaseRegex())
            .WithMessage("Secret Spell must include a Swift Minor Rune (a-z).")
            .Matches(SpecialGlyphRegex())
            .WithMessage("Secret Spell must include an Ancient Glyph (!@#$%^&* or similar).");
}
