namespace Banking.Application.Statements;

public sealed record GetStatementQuery(
    Guid AccountId,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page,
    int PageSize,
    Guid RequesterUserId,
    string RequesterRole);

