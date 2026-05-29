namespace Banking.Infrastructure.Auth;

public sealed class UserRecord
{
    public Guid Id { get; init; }

    public string Email { get; init; } = string.Empty;

    public string NormalizedEmail { get; init; } = string.Empty;

    public string PasswordHash { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }

    public List<RefreshTokenRecord> RefreshTokens { get; init; } = [];
}

