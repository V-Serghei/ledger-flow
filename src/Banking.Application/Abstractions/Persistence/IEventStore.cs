using Banking.Domain.Events;
using Banking.Domain.ValueObjects;

namespace Banking.Application.Abstractions.Persistence;

public interface IEventStore
{
    Task<IReadOnlyCollection<EventEnvelope>> LoadAsync(
        AccountId aggregateId,
        CancellationToken cancellationToken = default);

    Task AppendAsync(
        AccountId aggregateId,
        string aggregateType,
        int expectedVersion,
        IReadOnlyCollection<IDomainEvent> events,
        EventMetadata metadata,
        CancellationToken cancellationToken = default);
}

