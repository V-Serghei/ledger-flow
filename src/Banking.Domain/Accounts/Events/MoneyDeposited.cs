using Banking.Domain.Events;
using Banking.Domain.ValueObjects;

namespace Banking.Domain.Accounts.Events;

public sealed record MoneyDeposited(
    AccountId AccountId,
    Money Amount,
    Money BalanceAfter,
    string? Description) : DomainEvent;

