using System.Text.Json;
using Banking.Application.Messaging;
using Banking.Infrastructure.Persistence;

namespace Banking.Infrastructure.Outbox;

internal sealed class OutboxWriter(BankingDbContext dbContext) : IOutboxWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Add<TMessage>(
        TMessage message,
        Guid correlationId,
        IReadOnlyDictionary<string, string>? headers = null)
        where TMessage : notnull
    {
        dbContext.OutboxMessages.Add(new OutboxMessageRecord
        {
            Id = Guid.NewGuid(),
            Type = typeof(TMessage).AssemblyQualifiedName ?? typeof(TMessage).FullName!,
            Payload = JsonSerializer.Serialize(message, JsonOptions),
            Headers = JsonSerializer.Serialize(headers ?? new Dictionary<string, string>(), JsonOptions),
            OccurredOn = DateTimeOffset.UtcNow,
            CorrelationId = correlationId
        });
    }
}

