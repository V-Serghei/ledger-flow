using System.Net;
using Banking.IntegrationTests.Support;
using FluentAssertions;

namespace Banking.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class AccountsApiTests(BankingApiFactory factory)
{
    [Fact]
    public async Task should_open_account()
    {
        using var httpClient = factory.CreateClient();
        var api = new BankingApiClient(httpClient);
        var user = await api.RegisterAsync();

        var account = await api.OpenAccountAsync(user.AccessToken);

        account.AccountId.Should().NotBeEmpty();
        account.OwnerUserId.Should().Be(user.UserId);
        account.Status.Should().Be("Active");
        account.Balance.Should().Be(0);
    }

    [Fact]
    public async Task should_not_allow_customer_to_access_another_customer_account()
    {
        using var httpClient = factory.CreateClient();
        var api = new BankingApiClient(httpClient);
        var owner = await api.RegisterAsync();
        var intruder = await api.RegisterAsync();
        var account = await api.OpenAccountAsync(owner.AccessToken);

        using var request = BankingApiClient.CreateRequest(
            HttpMethod.Get,
            $"/api/v1/accounts/{account.AccountId}",
            intruder.AccessToken);
        var response = await httpClient.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
