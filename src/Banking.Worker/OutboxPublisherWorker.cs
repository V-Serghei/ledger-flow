using System.Text.Json;
using Banking.Infrastructure.Outbox;
using Banking.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Banking.Worker;

public sealed class OutboxPublisherWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxPublisherWorker> logger) : BackgroundService
{
    private const int BatchSize = 20;
    private const int MaxRetries = 5;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingMessagesAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Outbox publisher iteration failed. The worker will retry.");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task PublishPendingMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var messages = await dbContext.OutboxMessages
            .Where(message => message.Status == OutboxStatus.Pending)
            .OrderBy(message => message.OccurredOn)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            await PublishMessageAsync(dbContext, publishEndpoint, message, cancellationToken);
        }
    }

    private async Task PublishMessageAsync(
        BankingDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        OutboxMessageRecord message,
        CancellationToken cancellationToken)
    {
        message.Status = OutboxStatus.Processing;
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var messageType = Type.GetType(message.Type, throwOnError: true)
                ?? throw new InvalidOperationException($"Outbox message type '{message.Type}' could not be resolved.");

            var payload = JsonSerializer.Deserialize(message.Payload, messageType, new JsonSerializerOptions(JsonSerializerDefaults.Web))
                ?? throw new InvalidOperationException($"Outbox message '{message.Id}' payload could not be deserialized.");

            await publishEndpoint.Publish(payload, messageType, cancellationToken);

            message.Status = OutboxStatus.Processed;
            message.ProcessedOn = DateTimeOffset.UtcNow;
            message.Error = null;

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            message.RetryCount++;
            message.Status = message.RetryCount >= MaxRetries
                ? OutboxStatus.Failed
                : OutboxStatus.Pending;
            message.Error = exception.Message;

            logger.LogError(
                exception,
                "Failed to publish outbox message {OutboxMessageId} with retry count {RetryCount}",
                message.Id,
                message.RetryCount);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
