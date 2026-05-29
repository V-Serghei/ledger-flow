using Banking.Domain.ValueObjects;

namespace Banking.Application.Abstractions.Persistence;

public sealed record EventMetadata(
    Guid CorrelationId,
    Guid? CausationId,
    UserId? UserId);

