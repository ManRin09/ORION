using FluentValidation.TestHelper;
using Orion.Application.DTOs.Users;
using Orion.Application.Validators;

namespace Orion.UnitTests.Validators;

public sealed class RegisterUserRequestValidatorTests
{
    private readonly RegisterUserRequestValidator _validator = new();

    private static RegisterUserRequest CreateValidRequest(
        string firstName = "Arthur",
        string? middleName = "Pendragon",
        string email = "arthur@orion.realm",
        string? username = "arthur_dev",
        string password = "Orion!Spell2026",
        string? confirmPassword = null,
        bool acceptTerms = true) =>
        new(
            FirstName: firstName,
            MiddleName: middleName,
            Email: email,
            Username: username,
            Password: password,
            ConfirmPassword: confirmPassword ?? password,
            AcceptTerms: acceptTerms);

    [Fact]
    public void Validate_WithAllValidFieldsAndAllFourRunes_ShouldPass()
    {
        var result = _validator.TestValidate(CreateValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "Secret Spell (password) is required.")]
    [InlineData("Or!1a", "Minimum 8 runes")]
    [InlineData("orion!spell2026", "Elder Capital Rune")]
    [InlineData("ORION!SPELL2026", "Swift Minor Rune")]
    [InlineData("OrionSpell2026", "Ancient Glyph")]
    public void Validate_WithMissingCipherRune_ShouldFailOnPassword(
        string weakPassword,
        string _)
    {
        var request = CreateValidRequest(password: weakPassword, confirmPassword: weakPassword);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WhenConfirmPasswordDoesNotMatch_ShouldFailOnConfirmPassword()
    {
        var request = CreateValidRequest(
            password: "Orion!Spell2026",
            confirmPassword: "Different!Spell2026");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Fact]
    public void Validate_WhenTermsAreNotAccepted_ShouldFailOnAcceptTerms()
    {
        var request = CreateValidRequest(acceptTerms: false);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.AcceptTerms);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing-domain@")]
    [InlineData("")]
    public void Validate_WithInvalidEmail_ShouldFailOnEmail(string invalidEmail)
    {
        var request = CreateValidRequest(email: invalidEmail);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("bad-username!")]
    [InlineData("has spaces")]
    public void Validate_WithInvalidOptionalUsername_ShouldFailOnUsername(string invalidUsername)
    {
        var request = CreateValidRequest(username: invalidUsername);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Validate_WhenOptionalFieldsAreNull_ShouldPass()
    {
        var request = CreateValidRequest(middleName: null, username: null);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
