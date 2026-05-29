using Banking.Application.Auth;
using Banking.Application.Exceptions;
using Banking.Application.Statements;
using Banking.Infrastructure.Persistence;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Banking.Infrastructure.Statements;

internal sealed class StatementService(
    BankingDbContext dbContext,
    IConfiguration configuration) : IStatementService
{
    public async Task<PagedStatementResponse> GetStatementAsync(
        GetStatementQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var account = await dbContext.AccountsReadModel
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == query.AccountId, cancellationToken)
            ?? throw new NotFoundException("Account was not found.");

        if (query.RequesterRole != UserRoles.Admin && account.OwnerUserId != query.RequesterUserId)
        {
            throw new ForbiddenException("Customers can access only their own statement.");
        }

        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' was not configured.");

        await using var connection = new NpgsqlConnection(connectionString);
        var entries = await connection.QueryAsync<StatementEntryRow>(
            new CommandDefinition(
                """
                select
                    id as Id,
                    account_id as AccountId,
                    operation_id as OperationId,
                    operation_type as OperationType,
                    direction as Direction,
                    amount as Amount,
                    balance_after as BalanceAfter,
                    description as Description,
                    occurred_on as OccurredOn
                from statement_entries_read_model
                where account_id = @AccountId
                  and (@From is null or occurred_on >= @From)
                  and (@To is null or occurred_on <= @To)
                order by occurred_on desc
                limit @PageSize offset @Offset;
                """,
                new
                {
                    query.AccountId,
                    query.From,
                    query.To,
                    PageSize = pageSize,
                    Offset = (page - 1) * pageSize
                },
                cancellationToken: cancellationToken));

        return new PagedStatementResponse(
            query.AccountId,
            page,
            pageSize,
            entries.Select(entry => new StatementEntryResponse(
                entry.Id,
                entry.AccountId,
                entry.OperationId,
                entry.OperationType,
                entry.Direction,
                entry.Amount,
                entry.BalanceAfter,
                entry.Description,
                new DateTimeOffset(DateTime.SpecifyKind(entry.OccurredOn, DateTimeKind.Utc))))
                .ToArray());
    }

    private sealed class StatementEntryRow
    {
        public Guid Id { get; init; }

        public Guid AccountId { get; init; }

        public Guid OperationId { get; init; }

        public string OperationType { get; init; } = string.Empty;

        public string Direction { get; init; } = string.Empty;

        public decimal Amount { get; init; }

        public decimal BalanceAfter { get; init; }

        public string Description { get; init; } = string.Empty;

        public DateTime OccurredOn { get; init; }
    }
}
