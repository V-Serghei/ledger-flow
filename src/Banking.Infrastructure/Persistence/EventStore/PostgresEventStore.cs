using System.Text.Json;
using Banking.Application.Abstractions.Persistence;
using Banking.Application.Exceptions;
using Banking.Domain.Events;
using Banking.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Banking.Infrastructure.Persistence.EventStore;

internal sealed class PostgresEventStore(BankingDbContext dbContext) : IEventStore
{
    private const string UniqueConstraintName = "ix_event_store_aggregate_id_aggregate_version";

    public async Task<IReadOnlyCollection<EventEnvelope>> LoadAsync(
        AccountId aggregateId,
        CancellationToken cancellationToken = default)
    {
        var records = await dbContext.EventStore
            .AsNoTracking()
            .Where(record => record.AggregateId == aggregateId.Value)
            .OrderBy(record => record.AggregateVersion)
            .ToListAsync(cancellationToken);

        return records.Select(ToEnvelope).ToArray();
    }

    public async Task AppendAsync(
        AccountId aggregateId,
        string aggregateType,
        int expectedVersion,
        IReadOnlyCollection<IDomainEvent> events,
        EventMetadata metadata,
        CancellationToken cancellationToken = default)
    {
        if (events.Count == 0)
        {
            return;
        }

        var currentVersion = await dbContext.EventStore
            .AsNoTracking()
            .Where(record => record.AggregateId == aggregateId.Value)
            .MaxAsync(record => (int?)record.AggregateVersion, cancellationToken)
            ?? 0;

        if (currentVersion != expectedVersion)
        {
            throw new ConcurrencyConflictException(
                $"Expected aggregate version {expectedVersion}, but current version is {currentVersion}.");
        }

        var nextVersion = expectedVersion;
        var records = events.Select(domainEvent =>
        {
            nextVersion++;

            return new EventStoreRecord
            {
                Id = Guid.NewGuid(),
                AggregateId = aggregateId.Value,
                AggregateType = aggregateType,
                EventType = domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.GetType().FullName!,
                EventVersion = 1,
                AggregateVersion = nextVersion,
                Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), EventStoreSerializationOptions.Default),
                Metadata = JsonSerializer.Serialize(metadata, EventStoreSerializationOptions.Default),
                OccurredOn = domainEvent.OccurredOn,
                CorrelationId = metadata.CorrelationId,
                CausationId = metadata.CausationId,
                UserId = metadata.UserId?.Value
            };
        });

        dbContext.EventStore.AddRange(records);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            throw new ConcurrencyConflictException(
                "The aggregate was modified by another operation before the events could be appended.",
                exception);
        }
    }

    private static EventEnvelope ToEnvelope(EventStoreRecord record)
    {
        var eventType = Type.GetType(record.EventType, throwOnError: true)
            ?? throw new InvalidOperationException($"Event type '{record.EventType}' could not be resolved.");

        var domainEvent = (IDomainEvent?)JsonSerializer.Deserialize(
            record.Payload,
            eventType,
            EventStoreSerializationOptions.Default);

        if (domainEvent is null)
        {
            throw new InvalidOperationException($"Event '{record.Id}' could not be deserialized.");
        }

        var metadata = JsonSerializer.Deserialize<EventMetadata>(
            record.Metadata,
            EventStoreSerializationOptions.Default)
            ?? throw new InvalidOperationException($"Metadata for event '{record.Id}' could not be deserialized.");

        return new EventEnvelope(
            record.Id,
            new AccountId(record.AggregateId),
            record.AggregateType,
            record.EventType,
            record.EventVersion,
            record.AggregateVersion,
            domainEvent,
            metadata,
            record.OccurredOn);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException postgresException
            && postgresException.SqlState == PostgresErrorCodes.UniqueViolation
            && postgresException.ConstraintName == UniqueConstraintName;
    }
}

