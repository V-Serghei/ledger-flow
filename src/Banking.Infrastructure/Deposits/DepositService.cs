using System.Text.Json;
using Banking.Application.Abstractions.Persistence;
using Banking.Application.Auth;
using Banking.Application.Deposits;
using Banking.Application.Exceptions;
using Banking.Application.Messaging;
using Banking.Application.Messaging.IntegrationEvents;
using Banking.Domain.Accounts;
using Banking.Domain.Accounts.Events;
using Banking.Domain.ValueObjects;
using Banking.Infrastructure.Accounts;
using Banking.Infrastructure.Idempotency;
using Banking.Infrastructure.Persistence;
using Banking.Infrastructure.Statements;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Deposits;

internal sealed class DepositService(
    BankingDbContext dbContext,
    IEventStore eventStore,
    IOutboxWriter outboxWriter) : IDepositService
{
    private const string AggregateType = "Account";
    private const string OperationType = "Deposit";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<DepositResponse> DepositAsync(
        DepositMoneyCommand command,
        CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(command.IdempotencyKey);
        var requestHash = RequestHasher.Hash(command);

        var existingRecord = await dbContext.IdempotencyRecords
            .AsNoTracking()
            .SingleOrDefaultAsync(record =>
                record.UserId == command.RequesterUserId
                && record.OperationType == OperationType
                && record.IdempotencyKey == command.IdempotencyKey,
                cancellationToken);

        if (existingRecord is not null)
        {
            if (existingRecord.RequestHash != requestHash)
            {
                throw new IdempotencyConflictException(
                    "The same Idempotency-Key was already used with a different request payload.");
            }

            return JsonSerializer.Deserialize<DepositResponse>(existingRecord.ResponsePayload, JsonOptions)
                ?? throw new InvalidOperationException("Stored idempotency response could not be deserialized.");
        }

        var accountReadModel = await dbContext.AccountsReadModel
            .SingleOrDefaultAsync(account => account.Id == command.AccountId, cancellationToken)
            ?? throw new NotFoundException("Account was not found.");

        EnsureCanDeposit(accountReadModel, command.RequesterUserId, command.RequesterRole);

        var events = await eventStore.LoadAsync(new AccountId(command.AccountId), cancellationToken);
        var account = Account.Rehydrate(events.Select(envelope => envelope.Event));
        var expectedVersion = account.Version;
        var amount = Money.FromOperationAmount(command.Amount);

        account.Deposit(amount, command.Description);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await eventStore.AppendAsync(
            account.Id,
            AggregateType,
            expectedVersion,
            account.UncommittedEvents,
            CreateMetadata(command.RequesterUserId),
            cancellationToken);

        accountReadModel.Balance = account.Balance.Amount;
        accountReadModel.Status = account.Status.ToString();
        accountReadModel.Version = account.Version;
        accountReadModel.UpdatedAt = account.UpdatedAt;

        var depositedEvent = account.UncommittedEvents
            .OfType<MoneyDeposited>()
            .Single();

        var response = new DepositResponse(
            account.Id.Value,
            depositedEvent.Amount.Amount,
            depositedEvent.BalanceAfter.Amount,
            depositedEvent.Description ?? string.Empty,
            depositedEvent.OccurredOn);

        var correlationId = Guid.NewGuid();
        outboxWriter.Add(
            new MoneyDepositedIntegrationEvent(
                response.AccountId,
                response.Amount,
                response.BalanceAfter,
                response.Description,
                response.OccurredOn),
            correlationId);

        dbContext.StatementEntriesReadModel.Add(new StatementEntryReadModel
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id.Value,
            OperationId = Guid.NewGuid(),
            OperationType = OperationType,
            Direction = "Credit",
            Amount = response.Amount,
            BalanceAfter = response.BalanceAfter,
            Description = response.Description,
            OccurredOn = response.OccurredOn
        });

        dbContext.IdempotencyRecords.Add(new IdempotencyRecord
        {
            Id = Guid.NewGuid(),
            UserId = command.RequesterUserId,
            OperationType = OperationType,
            IdempotencyKey = command.IdempotencyKey,
            RequestHash = requestHash,
            ResponsePayload = JsonSerializer.Serialize(response, JsonOptions),
            StatusCode = StatusCodes.Created,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24)
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        account.ClearUncommittedEvents();

        return response;
    }

    private static EventMetadata CreateMetadata(Guid requesterUserId)
    {
        return new EventMetadata(
            Guid.NewGuid(),
            CausationId: null,
            new UserId(requesterUserId));
    }

    private static void EnsureIdempotencyKey(string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("Idempotency-Key header is required.", nameof(idempotencyKey));
        }
    }

    private static void EnsureCanDeposit(AccountReadModel account, Guid requesterUserId, string requesterRole)
    {
        if (requesterRole == UserRoles.Admin || account.OwnerUserId == requesterUserId)
        {
            return;
        }

        throw new ForbiddenException("Customers can deposit only into their own accounts.");
    }

    private static class StatusCodes
    {
        public const int Created = 201;
    }
}
