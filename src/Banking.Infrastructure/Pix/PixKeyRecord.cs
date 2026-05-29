namespace Banking.Infrastructure.Pix;

public sealed class PixKeyRecord
{
    public Guid Id { get; init; }

    public Guid AccountId { get; init; }

    public Guid OwnerUserId { get; init; }

    public string Type { get; init; } = string.Empty;

    public string Key { get; init; } = string.Empty;

    public string NormalizedKey { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }
}

