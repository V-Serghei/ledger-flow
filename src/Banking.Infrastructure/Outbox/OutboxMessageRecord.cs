namespace Banking.Infrastructure.Outbox;

public sealed class OutboxMessageRecord
{
    public Guid Id { get; init; }

    public string Type { get; init; } = string.Empty;

    public string Payload { get; init; } = string.Empty;

    public string Headers { get; init; } = "{}";

    public DateTimeOffset OccurredOn { get; init; }

    public DateTimeOffset? ProcessedOn { get; set; }

    public int RetryCount { get; set; }

    public string Status { get; set; } = OutboxStatus.Pending;

    public string? Error { get; set; }

    public Guid CorrelationId { get; init; }
}

