namespace Banking.Infrastructure.Persistence.EventStore;

public sealed class EventStoreRecord
{
    public Guid Id { get; init; }

    public Guid AggregateId { get; init; }

    public string AggregateType { get; init; } = string.Empty;

    public string EventType { get; init; } = string.Empty;

    public int EventVersion { get; init; }

    public int AggregateVersion { get; init; }

    public string Payload { get; init; } = string.Empty;

    public string Metadata { get; init; } = string.Empty;

    public DateTimeOffset OccurredOn { get; init; }

    public Guid CorrelationId { get; init; }

    public Guid? CausationId { get; init; }

    public Guid? UserId { get; init; }
}

