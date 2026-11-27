using FluentValidation.TestHelper;
using Orion.Application.DTOs.Users;
using Orion.Application.Validators;

namespace Orion.UnitTests.Validators;

public sealed class UpdateUserProfileRequestValidatorTests
{
    private readonly UpdateUserProfileRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidProfileAndHttpsAvatar_ShouldPass()
    {
        var request = new UpdateUserProfileRequest(
            FirstName: "Arthur",
            MiddleName: "High King",
            Username: "king_arthur",
            AvatarUrl: "https://avatars.githubusercontent.com/u/1?v=4");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://orion.realm/avatar.png")]
    [InlineData("/relative/path.png")]
    public void Validate_WithInvalidAvatarUrl_ShouldFailOnAvatarUrl(string invalidUrl)
    {
        var request = new UpdateUserProfileRequest(
            FirstName: "Arthur",
            MiddleName: null,
            Username: null,
            AvatarUrl: invalidUrl);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.AvatarUrl);
    }
}
