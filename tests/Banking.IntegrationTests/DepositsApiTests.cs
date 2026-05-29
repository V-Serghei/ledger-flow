using System.Net;
using Banking.Application.Deposits;
using Banking.IntegrationTests.Support;
using FluentAssertions;

namespace Banking.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class DepositsApiTests(BankingApiFactory factory)
{
    [Fact]
    public async Task should_deposit_money()
    {
        using var httpClient = factory.CreateClient();
        var api = new BankingApiClient(httpClient);
        var user = await api.RegisterAsync();
        var account = await api.OpenAccountAsync(user.AccessToken);

        var deposit = await api.DepositAsync(
            user.AccessToken,
            account.AccountId,
            100m,
            $"deposit-{Guid.NewGuid():N}");

        deposit.AccountId.Should().Be(account.AccountId);
        deposit.Amount.Should().Be(100m);
        deposit.BalanceAfter.Should().Be(100m);
    }

    [Fact]
    public async Task should_reject_transfer_without_idempotency_key()
    {
        using var httpClient = factory.CreateClient();
        var api = new BankingApiClient(httpClient);
        var user = await api.RegisterAsync();
        var source = await api.OpenAccountAsync(user.AccessToken);
        var destination = await api.OpenAccountAsync(user.AccessToken);

        using var request = BankingApiClient.CreateRequest(HttpMethod.Post, "/api/v1/transfers", user.AccessToken, body: new
        {
            sourceAccountId = source.AccountId,
            destinationAccountId = destination.AccountId,
            amount = 10m,
            description = "missing idempotency key"
        });
        var response = await httpClient.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task should_return_same_response_for_same_idempotency_key()
    {
        using var httpClient = factory.CreateClient();
        var api = new BankingApiClient(httpClient);
        var user = await api.RegisterAsync();
        var account = await api.OpenAccountAsync(user.AccessToken);
        var key = $"deposit-{Guid.NewGuid():N}";

        var first = await api.DepositAsync(user.AccessToken, account.AccountId, 100m, key);
        var second = await api.DepositAsync(user.AccessToken, account.AccountId, 100m, key);

        second.Should().BeEquivalentTo(first);
    }

    [Fact]
    public async Task should_reject_same_idempotency_key_with_different_payload()
    {
        using var httpClient = factory.CreateClient();
        var api = new BankingApiClient(httpClient);
        var user = await api.RegisterAsync();
        var account = await api.OpenAccountAsync(user.AccessToken);
        var key = $"deposit-{Guid.NewGuid():N}";
        _ = await api.DepositAsync(user.AccessToken, account.AccountId, 100m, key);

        using var request = BankingApiClient.CreateRequest(HttpMethod.Post, "/api/v1/deposits", user.AccessToken, key, new
        {
            accountId = account.AccountId,
            amount = 120m,
            description = "changed amount"
        });
        var response = await httpClient.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
