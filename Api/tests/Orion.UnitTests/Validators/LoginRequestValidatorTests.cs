using FluentValidation.TestHelper;
using Orion.Application.DTOs.Auth;
using Orion.Application.Validators;

namespace Orion.UnitTests.Validators;

public sealed class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Theory]
    [InlineData("arthur@orion.realm", "Orion!Spell2026")]
    [InlineData("arthur_dev", "123456")]
    public void Validate_WithValidIdentifierAndPassword_ShouldPass(string identifier, string password)
    {
        var result = _validator.TestValidate(new LoginRequest(identifier, password));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ab")]
    [InlineData("  ab  ")]
    public void Validate_WithInvalidIdentifier_ShouldFailOnIdentifier(string invalidIdentifier)
    {
        var result = _validator.TestValidate(new LoginRequest(invalidIdentifier, "Orion!Spell2026"));

        result.ShouldHaveValidationErrorFor(x => x.Identifier);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    public void Validate_WithTooShortPassword_ShouldFailOnPassword(string invalidPassword)
    {
        var result = _validator.TestValidate(new LoginRequest("arthur_dev", invalidPassword));

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
