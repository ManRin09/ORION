using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using NSubstitute;
using Orion.Domain.Exceptions;
using Orion.Infrastructure.Security;

namespace Orion.UnitTests.Security;

public sealed class GitHubOAuthClientTests
{
    private static IOptions<GitHubOAuthSettings> CreateSettings() =>
        Options.Create(new GitHubOAuthSettings
        {
            ClientId = "Ov23liwydqmi2AcQpDwK",
            ClientSecret = "test-client-secret",
            CallbackUrl = "https://orion-backend-yar4.onrender.com/api/auth/github/callback",
            FrontendRedirectUrl = "https://orion-1frh.onrender.com/auth/callback"
        });

    private static GitHubOAuthClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var handler = new StubHttpMessageHandler(responder);
        var httpClient = new HttpClient(handler);
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(GitHubOAuthClient.HttpClientName).Returns(httpClient);

        return new GitHubOAuthClient(factory, CreateSettings());
    }

    [Fact]
    public void BuildAuthorizationUrl_IncludesClientIdRedirectUriScopesAndState()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK));

        var url = client.BuildAuthorizationUrl("csrf-state-123");

        Assert.StartsWith("https://github.com/login/oauth/authorize?", url, StringComparison.Ordinal);
        Assert.Contains("client_id=Ov23liwydqmi2AcQpDwK", url, StringComparison.Ordinal);
        Assert.Contains(
            "redirect_uri=https%3A%2F%2Forion-backend-yar4.onrender.com%2Fapi%2Fauth%2Fgithub%2Fcallback",
            url,
            StringComparison.Ordinal);
        Assert.Contains("scope=read%3Auser%20user%3Aemail", url, StringComparison.Ordinal);
        Assert.Contains("state=csrf-state-123", url, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExchangeCodeForUserProfileAsync_WhenEmailIsPrivate_QueriesUserEmailsAndSelectsPrimaryVerified()
    {
        var requestedUrls = new List<string>();
        var userAgentHeaders = new List<string>();

        var client = CreateClient(request =>
        {
            requestedUrls.Add(request.RequestUri!.ToString());
            userAgentHeaders.Add(request.Headers.UserAgent.ToString());

            return request.RequestUri.ToString() switch
            {
                "https://github.com/login/oauth/access_token" => JsonResponse(
                    """{"access_token":"gho_test_token","token_type":"bearer"}"""),

                "https://api.github.com/user" => JsonResponse(
                    """{"id":4242,"login":"octo_coder","name":"Octo Coder","email":null,"avatar_url":"https://avatars.githubusercontent.com/u/4242"}"""),

                "https://api.github.com/user/emails" => JsonResponse(
                    """
                    [
                      {"email":"unverified@orion.realm","primary":true,"verified":false},
                      {"email":"secondary@orion.realm","primary":false,"verified":true},
                      {"email":"primary.verified@orion.realm","primary":true,"verified":true}
                    ]
                    """),

                _ => new HttpResponseMessage(HttpStatusCode.NotFound)
            };
        });

        var profile = await client.ExchangeCodeForUserProfileAsync("valid-code");

        Assert.Equal("4242", profile.GitHubId);
        Assert.Equal("octo_coder", profile.Login);
        Assert.Equal("Octo Coder", profile.Name);
        Assert.Equal("primary.verified@orion.realm", profile.Email);
        Assert.Equal("https://avatars.githubusercontent.com/u/4242", profile.AvatarUrl);

        Assert.Contains("https://api.github.com/user/emails", requestedUrls);
        Assert.All(userAgentHeaders, ua => Assert.Contains("Orion-App", ua, StringComparison.Ordinal));
    }

    [Fact]
    public async Task ExchangeCodeForUserProfileAsync_WhenPublicEmailExists_DoesNotCallUserEmailsEndpoint()
    {
        var requestedUrls = new List<string>();

        var client = CreateClient(request =>
        {
            requestedUrls.Add(request.RequestUri!.ToString());

            return request.RequestUri.ToString() switch
            {
                "https://github.com/login/oauth/access_token" => JsonResponse(
                    """{"access_token":"gho_public_token"}"""),

                "https://api.github.com/user" => JsonResponse(
                    """{"id":101,"login":"public_dev","name":"Public Dev","email":"public@orion.realm","avatar_url":null}"""),

                _ => new HttpResponseMessage(HttpStatusCode.NotFound)
            };
        });

        var profile = await client.ExchangeCodeForUserProfileAsync("code-public");

        Assert.Equal("public@orion.realm", profile.Email);
        Assert.DoesNotContain("https://api.github.com/user/emails", requestedUrls);
    }

    [Fact]
    public async Task ExchangeCodeForUserProfileAsync_WhenNoPrimaryVerifiedEmailExists_ThrowsDomainException()
    {
        var client = CreateClient(request => request.RequestUri!.ToString() switch
        {
            "https://github.com/login/oauth/access_token" => JsonResponse(
                """{"access_token":"gho_token"}"""),

            "https://api.github.com/user" => JsonResponse(
                """{"id":900,"login":"private_dev","name":null,"email":null}"""),

            "https://api.github.com/user/emails" => JsonResponse(
                """[{"email":"secondary@orion.realm","primary":false,"verified":true}]"""),

            _ => new HttpResponseMessage(HttpStatusCode.NotFound)
        });

        await Assert.ThrowsAsync<DomainException>(() => client.ExchangeCodeForUserProfileAsync("code-no-email"));
    }

    [Fact]
    public async Task ExchangeCodeForUserProfileAsync_WhenGitHubRejectsCode_ThrowsDomainException()
    {
        var client = CreateClient(_ => JsonResponse(
            """{"error":"bad_verification_code","error_description":"The code passed is incorrect or expired."}"""));

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => client.ExchangeCodeForUserProfileAsync("expired-code"));

        Assert.Contains("incorrect or expired", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static HttpResponseMessage JsonResponse(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(responder(request));
    }
}
