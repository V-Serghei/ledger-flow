namespace Banking.Application.Messaging.IntegrationEvents;

public sealed record MoneyDepositedIntegrationEvent(
    Guid AccountId,
    decimal Amount,
    decimal BalanceAfter,
    string Description,
    DateTimeOffset OccurredOn);

