using NSubstitute;
using Orion.Application.DTOs.Auth;
using Orion.Application.Interfaces;
using Orion.Application.Services;
using Orion.Domain.Entities;
using Orion.Domain.Enums;
using Orion.Domain.Exceptions;
using Orion.Domain.Interfaces;

namespace Orion.UnitTests.Services;

public sealed class GitHubAuthServiceTests
{
    private readonly IGitHubOAuthClient _gitHubOAuthClient = Substitute.For<IGitHubOAuthClient>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly GitHubAuthService _sut;

    public GitHubAuthServiceTests()
    {
        _sut = new GitHubAuthService(_gitHubOAuthClient, _userRepository, _tokenService);
    }

    [Fact]
    public async Task AuthenticateAsync_WithNewGitHubUser_SpawnsLevel1AdventurerAndResolvesUsernameCollision()
    {
        var profile = new GitHubUserProfile(
            GitHubId: "123456",
            Login: "Ada-Lovelace",
            Name: "Ada Augusta Byron",
            Email: "Ada@Orion.Realm",
            AvatarUrl: "https://avatars.githubusercontent.com/u/123456");

        User? createdUser = null;
        var expectedExpiry = DateTime.UtcNow.AddDays(7);

        _gitHubOAuthClient.ExchangeCodeForUserProfileAsync("oauth-code", Arg.Any<CancellationToken>())
            .Returns(profile);
        _userRepository.GetByGitHubIdAsync("123456", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.GetByEmailAsync("ada@orion.realm", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.ExistsByEmailAsync("ada@orion.realm", Arg.Any<CancellationToken>())
            .Returns(false);
        _userRepository.ExistsByUsernameAsync("ada_lovelace", Arg.Any<CancellationToken>())
            .Returns(true);
        _userRepository.ExistsByUsernameAsync(
                Arg.Is<string>(u => u.StartsWith("ada_lovelace_", StringComparison.Ordinal)),
                Arg.Any<CancellationToken>())
            .Returns(false);
        _userRepository.When(r => r.AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()))
            .Do(call => createdUser = call.Arg<User>());
        _tokenService.GenerateAccessToken(Arg.Any<User>(), true)
            .Returns(("orion-jwt-token", expectedExpiry));

        var response = await _sut.AuthenticateAsync("oauth-code");

        Assert.NotNull(createdUser);
        Assert.Equal(AuthProvider.GitHub, createdUser.AuthProvider);
        Assert.Equal("123456", createdUser.GitHubId);
        Assert.Equal("Ada", createdUser.FirstName);
        Assert.Equal("Augusta Byron", createdUser.MiddleName);
        Assert.Equal("ada@orion.realm", createdUser.Email);
        Assert.StartsWith("ada_lovelace_", createdUser.Username, StringComparison.Ordinal);
        Assert.Equal(1, createdUser.CurrentLevel);
        Assert.Equal(0, createdUser.TotalXp);
        Assert.True(createdUser.AcceptedTermsAt > DateTime.MinValue);

        Assert.Equal("orion-jwt-token", response.AccessToken);
        Assert.Equal("Bearer", response.TokenType);
        Assert.Equal("GitHub", response.User.AuthProvider);
        Assert.Equal(1, response.User.CurrentLevel);
        Assert.Equal(0, response.User.TotalXp);

        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AuthenticateAsync_WhenExistingUserMatchesByGitHubId_ReturnsTokenWithoutCreatingUser()
    {
        var existingUser = User.CreateGitHub(
            firstName: "Grace",
            middleName: "Hopper",
            email: "grace@orion.realm",
            username: "grace_hopper",
            gitHubId: "777",
            avatarUrl: "https://avatars.githubusercontent.com/u/777",
            acceptedTermsAtUtc: DateTime.UtcNow.AddDays(-10));

        var profile = new GitHubUserProfile("777", "grace", "Grace Hopper", "grace@orion.realm", null);

        _gitHubOAuthClient.ExchangeCodeForUserProfileAsync("code-777", Arg.Any<CancellationToken>())
            .Returns(profile);
        _userRepository.GetByGitHubIdAsync("777", Arg.Any<CancellationToken>())
            .Returns(existingUser);
        _tokenService.GenerateAccessToken(existingUser, true)
            .Returns(("jwt-777", DateTime.UtcNow.AddDays(7)));

        var response = await _sut.AuthenticateAsync("code-777");

        Assert.Equal("jwt-777", response.AccessToken);
        Assert.Equal(existingUser.Id, response.User.Id);
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _userRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AuthenticateAsync_WhenExistingLocalUserMatchesByEmail_LinksGitHubAccount()
    {
        var existingLocalUser = User.CreateLocal(
            firstName: "Alan",
            middleName: "Turing",
            email: "alan@orion.realm",
            username: "alan_turing",
            passwordHash: "hashed-secret",
            acceptedTermsAtUtc: DateTime.UtcNow.AddDays(-3));

        var profile = new GitHubUserProfile(
            GitHubId: "999",
            Login: "aturing",
            Name: "Alan Turing",
            Email: "Alan@Orion.Realm",
            AvatarUrl: "https://avatars.githubusercontent.com/u/999");

        _gitHubOAuthClient.ExchangeCodeForUserProfileAsync("code-999", Arg.Any<CancellationToken>())
            .Returns(profile);
        _userRepository.GetByGitHubIdAsync("999", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.GetByEmailAsync("alan@orion.realm", Arg.Any<CancellationToken>())
            .Returns(existingLocalUser);
        _tokenService.GenerateAccessToken(existingLocalUser, true)
            .Returns(("jwt-999", DateTime.UtcNow.AddDays(7)));

        var response = await _sut.AuthenticateAsync("code-999");

        Assert.Equal("999", existingLocalUser.GitHubId);
        Assert.Equal("https://avatars.githubusercontent.com/u/999", existingLocalUser.AvatarUrl);
        Assert.Equal("jwt-999", response.AccessToken);
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AuthenticateAsync_WhenSoftDeletedAccountUsesSameEmail_ThrowsDomainException()
    {
        var profile = new GitHubUserProfile("555", "ghost", "Ghost", "ghost@orion.realm", null);

        _gitHubOAuthClient.ExchangeCodeForUserProfileAsync("code-555", Arg.Any<CancellationToken>())
            .Returns(profile);
        _userRepository.GetByGitHubIdAsync("555", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.GetByEmailAsync("ghost@orion.realm", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.ExistsByEmailAsync("ghost@orion.realm", Arg.Any<CancellationToken>())
            .Returns(true);

        await Assert.ThrowsAsync<DomainException>(() => _sut.AuthenticateAsync("code-555"));

        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }
}
