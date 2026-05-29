namespace Banking.Application.Transfers;

public sealed record GetTransferByIdQuery(
    Guid TransferId,
    Guid RequesterUserId,
    string RequesterRole);

