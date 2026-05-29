using Banking.Domain.Events;
using Banking.Domain.ValueObjects;

namespace Banking.Domain.Accounts.Events;

public sealed record AccountOpened(
    AccountId AccountId,
    UserId OwnerUserId,
    Money InitialBalance) : DomainEvent;

