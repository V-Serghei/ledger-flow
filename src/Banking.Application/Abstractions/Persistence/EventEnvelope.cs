using Banking.Domain.Events;
using Banking.Domain.ValueObjects;

namespace Banking.Application.Abstractions.Persistence;

public sealed record EventEnvelope(
    Guid Id,
    AccountId AggregateId,
    string AggregateType,
    string EventType,
    int EventVersion,
    int AggregateVersion,
    IDomainEvent Event,
    EventMetadata Metadata,
    DateTimeOffset OccurredOn);

