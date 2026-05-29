namespace Banking.Infrastructure.Auth;

public sealed class RefreshTokenRecord
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public string TokenHash { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset ExpiresOn { get; init; }

    public DateTimeOffset? RevokedOn { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public UserRecord User { get; init; } = null!;

    public bool IsActive(DateTimeOffset utcNow) => RevokedOn is null && ExpiresOn > utcNow;
}

