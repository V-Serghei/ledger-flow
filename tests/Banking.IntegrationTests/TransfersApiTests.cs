using Banking.IntegrationTests.Support;
using FluentAssertions;

namespace Banking.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class TransfersApiTests(BankingApiFactory factory)
{
    [Fact]
    public async Task should_transfer_money()
    {
        using var httpClient = factory.CreateClient();
        var api = new BankingApiClient(httpClient);
        var user = await api.RegisterAsync();
        var source = await api.OpenAccountAsync(user.AccessToken);
        var destination = await api.OpenAccountAsync(user.AccessToken);
        _ = await api.DepositAsync(user.AccessToken, source.AccountId, 100m, $"deposit-{Guid.NewGuid():N}");

        var transfer = await api.TransferAsync(
            user.AccessToken,
            source.AccountId,
            destination.AccountId,
            50m,
            $"transfer-{Guid.NewGuid():N}");

        transfer.TransferId.Should().NotBeEmpty();
        transfer.SourceAccountId.Should().Be(source.AccountId);
        transfer.DestinationAccountId.Should().Be(destination.AccountId);
        transfer.Amount.Should().Be(50m);
        transfer.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task should_get_transfer_by_id()
    {
        using var httpClient = factory.CreateClient();
        var api = new BankingApiClient(httpClient);
        var user = await api.RegisterAsync();
        var source = await api.OpenAccountAsync(user.AccessToken);
        var destination = await api.OpenAccountAsync(user.AccessToken);
        _ = await api.DepositAsync(user.AccessToken, source.AccountId, 100m, $"deposit-{Guid.NewGuid():N}");
        var transfer = await api.TransferAsync(
            user.AccessToken,
            source.AccountId,
            destination.AccountId,
            25m,
            $"transfer-{Guid.NewGuid():N}");

        using var request = BankingApiClient.CreateRequest(
            HttpMethod.Get,
            $"/api/v1/transfers/{transfer.TransferId}",
            user.AccessToken);
        var response = await httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var fetched = await BankingApiClient.ReadAsync<Banking.Application.Transfers.TransferResponse>(response);
        fetched.Should().BeEquivalentTo(transfer, options => options.Excluding(candidate => candidate.OccurredOn));
        fetched.OccurredOn.Should().BeCloseTo(transfer.OccurredOn, TimeSpan.FromMilliseconds(1));
    }
}
