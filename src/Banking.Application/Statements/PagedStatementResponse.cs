namespace Banking.Application.Statements;

public sealed record PagedStatementResponse(
    Guid AccountId,
    int Page,
    int PageSize,
    IReadOnlyCollection<StatementEntryResponse> Entries);

