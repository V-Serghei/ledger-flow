namespace Banking.Domain.Events;

public abstract record DomainEvent : IDomainEvent
{
    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
    }

    public Guid EventId { get; init; }

    public DateTimeOffset OccurredOn { get; init; }
}

