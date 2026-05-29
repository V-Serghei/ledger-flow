using Banking.Domain.Events;
using Banking.Domain.ValueObjects;

namespace Banking.Domain.Accounts.Events;

public sealed record TransferCompleted(
    AccountId SourceAccountId,
    AccountId DestinationAccountId,
    Money Amount,
    Money SourceBalanceAfter,
    Money DestinationBalanceAfter,
    string? Description) : DomainEvent;

