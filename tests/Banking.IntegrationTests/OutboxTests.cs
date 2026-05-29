using Banking.Infrastructure.Outbox;
using Banking.IntegrationTests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Banking.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class OutboxTests(BankingApiFactory factory)
{
    [Fact]
    public async Task should_write_outbox_message_for_deposit()
    {
        using var httpClient = factory.CreateClient();
        var api = new BankingApiClient(httpClient);
        var user = await api.RegisterAsync();
        var account = await api.OpenAccountAsync(user.AccessToken);

        _ = await api.DepositAsync(user.AccessToken, account.AccountId, 100m, $"deposit-{Guid.NewGuid():N}");

        var messages = await factory.ExecuteDbContextAsync(context =>
            context.OutboxMessages
                .AsNoTracking()
                .Where(message => message.Type.Contains("MoneyDepositedIntegrationEvent"))
                .ToListAsync());

        messages.Should().Contain(message => message.Status == OutboxStatus.Pending);
    }

    [Fact]
    public async Task should_write_outbox_message_for_transfer()
    {
        using var httpClient = factory.CreateClient();
        var api = new BankingApiClient(httpClient);
        var user = await api.RegisterAsync();
        var source = await api.OpenAccountAsync(user.AccessToken);
        var destination = await api.OpenAccountAsync(user.AccessToken);
        _ = await api.DepositAsync(user.AccessToken, source.AccountId, 100m, $"deposit-{Guid.NewGuid():N}");

        _ = await api.TransferAsync(
            user.AccessToken,
            source.AccountId,
            destination.AccountId,
            25m,
            $"transfer-{Guid.NewGuid():N}");

        var messages = await factory.ExecuteDbContextAsync(context =>
            context.OutboxMessages
                .AsNoTracking()
                .Where(message => message.Type.Contains("TransferCompletedIntegrationEvent"))
                .ToListAsync());

        messages.Should().Contain(message => message.Status == OutboxStatus.Pending);
    }
}
