using Banking.Domain.Events;
using Banking.Domain.ValueObjects;

namespace Banking.Domain.Accounts.Events;

public sealed record AccountActivated(AccountId AccountId) : DomainEvent;

