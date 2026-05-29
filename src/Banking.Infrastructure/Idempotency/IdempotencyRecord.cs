namespace Banking.Infrastructure.Idempotency;

public sealed class IdempotencyRecord
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public string OperationType { get; init; } = string.Empty;

    public string IdempotencyKey { get; init; } = string.Empty;

    public string RequestHash { get; init; } = string.Empty;

    public string ResponsePayload { get; set; } = string.Empty;

    public int StatusCode { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset ExpiresAt { get; init; }
}

