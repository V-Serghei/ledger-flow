using Banking.Application.Pix;
using Banking.IntegrationTests.Support;
using FluentAssertions;

namespace Banking.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class PixApiTests(BankingApiFactory factory)
{
    [Fact]
    public async Task should_register_pix_key()
    {
        using var httpClient = factory.CreateClient();
        var api = new BankingApiClient(httpClient);
        var user = await api.RegisterAsync();
        var account = await api.OpenAccountAsync(user.AccessToken);
        var key = BankingApiClient.UniqueEmail();

        var pixKey = await api.RegisterPixKeyAsync(user.AccessToken, account.AccountId, key);

        pixKey.Id.Should().NotBeEmpty();
        pixKey.AccountId.Should().Be(account.AccountId);
        pixKey.Key.Should().Be(key);
        pixKey.Type.Should().Be("Email");
    }

    [Fact]
    public async Task should_pay_pix_as_transfer()
    {
        using var httpClient = factory.CreateClient();
        var api = new BankingApiClient(httpClient);
        var payer = await api.RegisterAsync();
        var receiver = await api.RegisterAsync();
        var source = await api.OpenAccountAsync(payer.AccessToken);
        var destination = await api.OpenAccountAsync(receiver.AccessToken);
        var key = BankingApiClient.UniqueEmail();
        _ = await api.RegisterPixKeyAsync(receiver.AccessToken, destination.AccountId, key);
        _ = await api.DepositAsync(payer.AccessToken, source.AccountId, 100m, $"deposit-{Guid.NewGuid():N}");

        var payment = await api.PayPixAsync(
            payer.AccessToken,
            source.AccountId,
            key,
            40m,
            $"pix-{Guid.NewGuid():N}");

        payment.TransferId.Should().NotBeEmpty();
        payment.SourceAccountId.Should().Be(source.AccountId);
        payment.DestinationAccountId.Should().Be(destination.AccountId);
        payment.Amount.Should().Be(40m);
        payment.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task should_list_pix_keys()
    {
        using var httpClient = factory.CreateClient();
        var api = new BankingApiClient(httpClient);
        var user = await api.RegisterAsync();
        var account = await api.OpenAccountAsync(user.AccessToken);
        var key = BankingApiClient.UniqueEmail();
        _ = await api.RegisterPixKeyAsync(user.AccessToken, account.AccountId, key);

        using var request = BankingApiClient.CreateRequest(HttpMethod.Get, "/api/v1/pix/keys", user.AccessToken);
        var response = await httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var keys = await BankingApiClient.ReadAsync<IReadOnlyCollection<PixKeyResponse>>(response);
        keys.Should().ContainSingle(candidate => candidate.AccountId == account.AccountId && candidate.Key == key);
    }
}
