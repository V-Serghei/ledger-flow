using Banking.IntegrationTests.Support;
using FluentAssertions;

namespace Banking.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class StatementApiTests(BankingApiFactory factory)
{
    [Fact]
    public async Task should_return_statement()
    {
        using var httpClient = factory.CreateClient();
        var api = new BankingApiClient(httpClient);
        var user = await api.RegisterAsync();
        var account = await api.OpenAccountAsync(user.AccessToken);

        _ = await api.DepositAsync(user.AccessToken, account.AccountId, 100m, $"deposit-{Guid.NewGuid():N}");

        var statement = await api.GetStatementAsync(user.AccessToken, account.AccountId);

        statement.AccountId.Should().Be(account.AccountId);
        statement.Entries.Should().ContainSingle(entry =>
            entry.OperationType == "Deposit"
            && entry.Direction == "Credit"
            && entry.Amount == 100m
            && entry.BalanceAfter == 100m);
    }
}
