namespace Banking.Application.Messaging.IntegrationEvents;

public sealed record PixPaymentCompletedIntegrationEvent(
    Guid PaymentId,
    Guid TransferId,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    string DestinationKey,
    decimal Amount,
    DateTimeOffset OccurredOn);

