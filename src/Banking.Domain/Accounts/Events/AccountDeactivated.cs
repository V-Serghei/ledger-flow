using Banking.Domain.Events;
using Banking.Domain.ValueObjects;

namespace Banking.Domain.Accounts.Events;

public sealed record AccountDeactivated(AccountId AccountId) : DomainEvent;

