using FluentValidation;
using NSubstitute;
using Orion.Application.DTOs.Auth;
using Orion.Application.Interfaces;
using Orion.Application.Services;
using Orion.Application.Validators;
using Orion.Domain.Entities;
using Orion.Domain.Exceptions;
using Orion.Domain.Interfaces;

namespace Orion.UnitTests.Services;

public sealed class AuthServiceTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(
            _userRepository,
            _passwordHasher,
            _tokenService,
            new LoginRequestValidator(),
            new ChangePasswordRequestValidator());
    }

    private static User CreateLocalUser(string passwordHash = "$2a$12$hashed_secret_value") =>
        User.CreateLocal(
            firstName: "Arthur",
            middleName: "Pendragon",
            email: "Arthur.Pendragon@Orion.Realm",
            username: "arthur_pendragon",
            passwordHash: passwordHash,
            acceptedTermsAtUtc: DateTime.UtcNow.AddDays(-1));

    [Theory]
    [InlineData("Arthur.Pendragon@Orion.Realm", true)]
    [InlineData("arthur_pendragon", false)]
    public async Task LoginAsync_WithValidCredentials_ReturnsBearerTokenAndUserProfile(
        string identifier,
        bool rememberTerminal)
    {
        var user = CreateLocalUser();
        var expectedExpiresAt = DateTime.UtcNow.AddHours(2);
        var request = new LoginRequest(identifier, "Orion!Spell2026", rememberTerminal);

        _userRepository.GetByIdentifierAsync(identifier, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify(request.Password, user.PasswordHash!).Returns(true);
        _tokenService.GenerateAccessToken(user, rememberTerminal).Returns(("jwt-token-value", expectedExpiresAt));

        var response = await _sut.LoginAsync(request);

        Assert.Equal("jwt-token-value", response.AccessToken);
        Assert.Equal("Bearer", response.TokenType);
        Assert.Equal(expectedExpiresAt, response.ExpiresAtUtc);
        Assert.Equal(rememberTerminal, response.RememberTerminal);
        Assert.Equal(user.Id, response.User.Id);
        Assert.Equal("arthur_pendragon", response.User.Username);
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ThrowsInvalidCredentialsException()
    {
        var request = new LoginRequest("missing@orion.realm", "Orion!Spell2026");
        _userRepository.GetByIdentifierAsync(request.Identifier, Arg.Any<CancellationToken>()).Returns((User?)null);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.LoginAsync(request));

        _passwordHasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
        _tokenService.DidNotReceive().GenerateAccessToken(Arg.Any<User>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task LoginAsync_WhenUserHasNoLocalPasswordHash_ThrowsInvalidCredentialsException()
    {
        var gitHubUser = User.CreateGitHub(
            firstName: "Arthur",
            middleName: null,
            email: "arthur@github.realm",
            username: "arthur_gh",
            gitHubId: "123456",
            avatarUrl: null,
            acceptedTermsAtUtc: DateTime.UtcNow);
        var request = new LoginRequest("arthur_gh", "Orion!Spell2026");

        _userRepository.GetByIdentifierAsync("arthur_gh", Arg.Any<CancellationToken>()).Returns(gitHubUser);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.LoginAsync(request));

        _passwordHasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
        _tokenService.DidNotReceive().GenerateAccessToken(Arg.Any<User>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordDoesNotMatch_ThrowsInvalidCredentialsException()
    {
        var user = CreateLocalUser();
        var request = new LoginRequest("arthur_pendragon", "Wrong!Spell2026");

        _userRepository.GetByIdentifierAsync(request.Identifier, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify(request.Password, user.PasswordHash!).Returns(false);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.LoginAsync(request));

        _tokenService.DidNotReceive().GenerateAccessToken(Arg.Any<User>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPayload_ThrowsValidationExceptionWithoutQueryingRepository()
    {
        var request = new LoginRequest("ab", "123");

        await Assert.ThrowsAsync<ValidationException>(() => _sut.LoginAsync(request));

        await _userRepository.DidNotReceive().GetByIdentifierAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenUserExists_ReturnsUserResponse()
    {
        var user = CreateLocalUser();
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var response = await _sut.GetCurrentUserAsync(user.Id);

        Assert.Equal(user.Id, response.Id);
        Assert.Equal("arthur.pendragon@orion.realm", response.Email);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenUserNotFound_ThrowsUserNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _userRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((User?)null);

        await Assert.ThrowsAsync<UserNotFoundException>(() => _sut.GetCurrentUserAsync(missingId));
    }

    [Fact]
    public async Task ChangePasswordAsync_WithValidRequest_UpdatesHashAndSavesChanges()
    {
        var user = CreateLocalUser("old-hash");
        var request = new ChangePasswordRequest(
            CurrentPassword: "Orion!Spell2026",
            NewPassword: "Orion!NewSpell2026",
            ConfirmNewPassword: "Orion!NewSpell2026");

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify(request.CurrentPassword, "old-hash").Returns(true);
        _passwordHasher.Hash(request.NewPassword).Returns("new-hash");

        await _sut.ChangePasswordAsync(user.Id, request);

        Assert.Equal("new-hash", user.PasswordHash);
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenCurrentPasswordIsIncorrect_ThrowsInvalidCredentialsException()
    {
        var user = CreateLocalUser("old-hash");
        var request = new ChangePasswordRequest(
            CurrentPassword: "Wrong!Spell2026",
            NewPassword: "Orion!NewSpell2026",
            ConfirmNewPassword: "Orion!NewSpell2026");

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify(request.CurrentPassword, "old-hash").Returns(false);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.ChangePasswordAsync(user.Id, request));

        _passwordHasher.DidNotReceive().Hash(Arg.Any<string>());
        await _userRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenAccountHasNoLocalPassword_ThrowsDomainException()
    {
        var gitHubUser = User.CreateGitHub(
            firstName: "Arthur",
            middleName: null,
            email: "arthur@github.realm",
            username: "arthur_gh",
            gitHubId: "123456",
            avatarUrl: null,
            acceptedTermsAtUtc: DateTime.UtcNow);
        var request = new ChangePasswordRequest(
            CurrentPassword: "Orion!Spell2026",
            NewPassword: "Orion!NewSpell2026",
            ConfirmNewPassword: "Orion!NewSpell2026");

        _userRepository.GetByIdAsync(gitHubUser.Id, Arg.Any<CancellationToken>()).Returns(gitHubUser);

        await Assert.ThrowsAsync<DomainException>(() => _sut.ChangePasswordAsync(gitHubUser.Id, request));
        await _userRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenUserIsDeactivated_ThrowsDomainException()
    {
        var user = CreateLocalUser("old-hash");
        user.Deactivate();

        Assert.Throws<DomainException>(() => user.ChangePassword("new-hash"));
    }
}
