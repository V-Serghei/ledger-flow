namespace Banking.Application.Pix;

public sealed record PixPaymentResponse(
    Guid PaymentId,
    Guid TransferId,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    string DestinationKey,
    decimal Amount,
    string Status,
    string Description,
    DateTimeOffset OccurredOn);

