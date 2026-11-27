using FluentValidation.TestHelper;
using Orion.Application.DTOs.Auth;
using Orion.Application.Validators;

namespace Orion.UnitTests.Validators;

public sealed class ChangePasswordRequestValidatorTests
{
    private readonly ChangePasswordRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidCurrentAndStrongNewPassword_ShouldPass()
    {
        var request = new ChangePasswordRequest(
            CurrentPassword: "Orion!OldSpell2026",
            NewPassword: "Orion!NewSpell2026",
            ConfirmNewPassword: "Orion!NewSpell2026");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenNewPasswordMatchesCurrentPassword_ShouldFail()
    {
        const string sameSpell = "Orion!Spell2026";
        var request = new ChangePasswordRequest(sameSpell, sameSpell, sameSpell);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void Validate_WhenNewPasswordViolatesCipherRunes_ShouldFail()
    {
        var request = new ChangePasswordRequest(
            CurrentPassword: "Orion!OldSpell2026",
            NewPassword: "weak",
            ConfirmNewPassword: "weak");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void Validate_WhenConfirmNewPasswordDoesNotMatch_ShouldFail()
    {
        var request = new ChangePasswordRequest(
            CurrentPassword: "Orion!OldSpell2026",
            NewPassword: "Orion!NewSpell2026",
            ConfirmNewPassword: "Orion!Mismatch2026");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ConfirmNewPassword);
    }
}
