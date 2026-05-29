namespace Banking.Application.Messaging.IntegrationEvents;

public sealed record TransferCompletedIntegrationEvent(
    Guid TransferId,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    string Description,
    DateTimeOffset OccurredOn);

