using System.Net;
using Banking.Application.Accounts;
using Banking.Application.Statements;
using Banking.IntegrationTests.Support;
using FluentAssertions;

namespace Banking.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class ConcurrentTransfersTests(BankingApiFactory factory)
{
    [Fact]
    public async Task should_not_allow_negative_balance_when_two_transfers_run_concurrently()
    {
        using var httpClient = factory.CreateClient();
        var api = new BankingApiClient(httpClient);
        var user = await api.RegisterAsync();
        var source = await api.OpenAccountAsync(user.AccessToken);
        var destinations = new[]
        {
            await api.OpenAccountAsync(user.AccessToken),
            await api.OpenAccountAsync(user.AccessToken)
        };
        _ = await api.DepositAsync(user.AccessToken, source.AccountId, 100m, $"deposit-{Guid.NewGuid():N}");

        var responses = await Task.WhenAll(destinations.Select((destination, index) =>
            SendTransferAsync(
                httpClient,
                user.AccessToken,
                source.AccountId,
                destination.AccountId,
                80m,
                $"transfer-two-{index}-{Guid.NewGuid():N}")));

        responses.Count(response => response.StatusCode == HttpStatusCode.Created).Should().Be(1);
        responses.Count(response => response.StatusCode == HttpStatusCode.Conflict).Should().Be(1);

        var account = await GetAccountAsync(httpClient, user.AccessToken, source.AccountId);
        account.Balance.Should().Be(20m);
        account.Balance.Should().BeGreaterThanOrEqualTo(0m);
    }

    [Fact]
    public async Task should_allow_only_valid_number_of_concurrent_transfers()
    {
        using var httpClient = factory.CreateClient();
        var api = new BankingApiClient(httpClient);
        var user = await api.RegisterAsync();
        var source = await api.OpenAccountAsync(user.AccessToken);
        var destinations = new List<AccountDetailsResponse>();

        for (var i = 0; i < 10; i++)
        {
            destinations.Add(await api.OpenAccountAsync(user.AccessToken));
        }

        _ = await api.DepositAsync(user.AccessToken, source.AccountId, 100m, $"deposit-{Guid.NewGuid():N}");

        var responses = await Task.WhenAll(destinations.Select((destination, index) =>
            SendTransferAsync(
                httpClient,
                user.AccessToken,
                source.AccountId,
                destination.AccountId,
                30m,
                $"transfer-ten-{index}-{Guid.NewGuid():N}")));

        responses.Count(response => response.StatusCode == HttpStatusCode.Created).Should().Be(3);
        responses.Count(response => response.StatusCode == HttpStatusCode.Conflict).Should().Be(7);

        var account = await GetAccountAsync(httpClient, user.AccessToken, source.AccountId);
        account.Balance.Should().Be(10m);
        account.Balance.Should().BeGreaterThanOrEqualTo(0m);
    }

    [Fact]
    public async Task should_keep_statement_consistent_after_concurrent_operations()
    {
        using var httpClient = factory.CreateClient();
        var api = new BankingApiClient(httpClient);
        var user = await api.RegisterAsync();
        var source = await api.OpenAccountAsync(user.AccessToken);
        var destinations = new List<AccountDetailsResponse>();

        for (var i = 0; i < 10; i++)
        {
            destinations.Add(await api.OpenAccountAsync(user.AccessToken));
        }

        _ = await api.DepositAsync(user.AccessToken, source.AccountId, 100m, $"deposit-{Guid.NewGuid():N}");

        _ = await Task.WhenAll(destinations.Select((destination, index) =>
            SendTransferAsync(
                httpClient,
                user.AccessToken,
                source.AccountId,
                destination.AccountId,
                30m,
                $"transfer-statement-{index}-{Guid.NewGuid():N}")));

        var statement = await api.GetStatementAsync(user.AccessToken, source.AccountId);

        statement.Entries.Count(entry => entry.OperationType == "Transfer" && entry.Direction == "Debit")
            .Should()
            .Be(3);
        statement.Entries.Should().OnlyContain(entry => entry.BalanceAfter >= 0m);
        statement.Entries.Min(entry => entry.BalanceAfter).Should().Be(10m);
    }

    private static async Task<HttpResponseMessage> SendTransferAsync(
        HttpClient httpClient,
        string token,
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        string idempotencyKey)
    {
        using var request = BankingApiClient.CreateRequest(HttpMethod.Post, "/api/v1/transfers", token, idempotencyKey, new
        {
            sourceAccountId,
            destinationAccountId,
            amount,
            description = "concurrent transfer"
        });

        return await httpClient.SendAsync(request);
    }

    private static async Task<AccountDetailsResponse> GetAccountAsync(
        HttpClient httpClient,
        string token,
        Guid accountId)
    {
        using var request = BankingApiClient.CreateRequest(HttpMethod.Get, $"/api/v1/accounts/{accountId}", token);
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await BankingApiClient.ReadAsync<AccountDetailsResponse>(response);
    }
}
