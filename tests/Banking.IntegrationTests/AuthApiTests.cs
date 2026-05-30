using System.Net;
using System.Net.Http.Json;
using Banking.IntegrationTests.Support;
using FluentAssertions;

namespace Banking.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class AuthApiTests(BankingApiFactory factory)
{
    [Fact]
    public async Task should_register_user()
    {
        using var httpClient = factory.CreateClient();
        var api = new BankingApiClient(httpClient);

        var response = await api.RegisterAsync();

        response.UserId.Should().NotBeEmpty();
        response.Email.Should().EndWith("@test.com");
        response.Role.Should().Be("Customer");
        response.AccessToken.Should().NotBeNullOrWhiteSpace();
        response.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task should_login_user()
    {
        using var httpClient = factory.CreateClient();
        var api = new BankingApiClient(httpClient);
        var registered = await api.RegisterAsync();

        var response = await api.LoginAsync(registered.Email);

        response.UserId.Should().Be(registered.UserId);
        response.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task should_refresh_token()
    {
        using var httpClient = factory.CreateClient();
        var api = new BankingApiClient(httpClient);
        var registered = await api.RegisterAsync();

        var response = await api.RefreshAsync(registered.RefreshToken);

        response.UserId.Should().Be(registered.UserId);
        response.AccessToken.Should().NotBe(registered.AccessToken);
        response.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task should_reject_invalid_registration_payload()
    {
        using var httpClient = factory.CreateClient();

        var response = await httpClient.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "not-an-email",
            password = "short"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
