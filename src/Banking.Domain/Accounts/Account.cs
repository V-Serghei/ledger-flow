using Banking.Domain.Accounts.Events;
using Banking.Domain.Events;
using Banking.Domain.Exceptions;
using Banking.Domain.ValueObjects;

namespace Banking.Domain.Accounts;

public sealed class Account
{
    private readonly List<IDomainEvent> _uncommittedEvents = [];

    private Account()
    {
    }

    public AccountId Id { get; private set; }

    public UserId OwnerUserId { get; private set; }

    public AccountStatus Status { get; private set; }

    public Money Balance { get; private set; }

    public int Version { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<IDomainEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();

    public static Account Open(AccountId accountId, UserId ownerUserId)
    {
        var account = new Account();

        account.Raise(new AccountOpened(accountId, ownerUserId, Money.Zero));

        return account;
    }

    public static Account Rehydrate(IEnumerable<IDomainEvent> events)
    {
        var account = new Account();

        foreach (var domainEvent in events)
        {
            account.Apply(domainEvent);
        }

        account.ClearUncommittedEvents();

        return account;
    }

    public void Activate()
    {
        if (Status == AccountStatus.Active)
        {
            return;
        }

        Raise(new AccountActivated(Id));
    }

    public void Deactivate()
    {
        if (Status == AccountStatus.Inactive)
        {
            return;
        }

        Raise(new AccountDeactivated(Id));
    }

    public void Deposit(Money amount, string? description = null)
    {
        EnsureActive();
        EnsureOperationAmount(amount);

        Raise(new MoneyDeposited(Id, amount, Balance + amount, description));
    }

    public void Debit(Money amount, string? description = null)
    {
        EnsureActive();
        EnsureOperationAmount(amount);

        if (Balance < amount)
        {
            throw new InsufficientFundsException("The account does not have enough balance to complete this operation.");
        }

        Raise(new AccountDebited(Id, amount, Balance - amount, description));
    }

    public void Credit(Money amount, string? description = null)
    {
        EnsureActive();
        EnsureOperationAmount(amount);

        Raise(new AccountCredited(Id, amount, Balance + amount, description));
    }

    public void TransferTo(Account destination, Money amount, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(destination);

        EnsureActive();
        destination.EnsureActive();
        EnsureOperationAmount(amount);

        if (Id == destination.Id)
        {
            throw new SameAccountTransferException("Source and destination accounts must be different.");
        }

        if (Balance < amount)
        {
            throw new InsufficientFundsException("The account does not have enough balance to complete this operation.");
        }

        Debit(amount, description);
        destination.Credit(amount, description);

        Raise(new TransferCompleted(Id, destination.Id, amount, Balance, destination.Balance, description));
    }

    public void ClearUncommittedEvents() => _uncommittedEvents.Clear();

    private void EnsureActive()
    {
        if (Status != AccountStatus.Active)
        {
            throw new InactiveAccountException("Inactive accounts cannot move money.");
        }
    }

    private static void EnsureOperationAmount(Money amount)
    {
        if (amount <= Money.Zero)
        {
            throw new InvalidMoneyAmountException("Operation amount must be greater than zero.");
        }
    }

    private void Raise(IDomainEvent domainEvent)
    {
        Apply(domainEvent);
        _uncommittedEvents.Add(domainEvent);
    }

    private void Apply(IDomainEvent domainEvent)
    {
        When(domainEvent);
        Version++;
    }

    private void When(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case AccountOpened accountOpened:
                Id = accountOpened.AccountId;
                OwnerUserId = accountOpened.OwnerUserId;
                Status = AccountStatus.Active;
                Balance = accountOpened.InitialBalance;
                CreatedAt = accountOpened.OccurredOn;
                UpdatedAt = accountOpened.OccurredOn;
                break;
            case AccountActivated accountActivated:
                Status = AccountStatus.Active;
                UpdatedAt = accountActivated.OccurredOn;
                break;
            case AccountDeactivated accountDeactivated:
                Status = AccountStatus.Inactive;
                UpdatedAt = accountDeactivated.OccurredOn;
                break;
            case MoneyDeposited moneyDeposited:
                Balance = moneyDeposited.BalanceAfter;
                UpdatedAt = moneyDeposited.OccurredOn;
                break;
            case AccountDebited accountDebited:
                Balance = accountDebited.BalanceAfter;
                UpdatedAt = accountDebited.OccurredOn;
                break;
            case AccountCredited accountCredited:
                Balance = accountCredited.BalanceAfter;
                UpdatedAt = accountCredited.OccurredOn;
                break;
            case TransferCompleted transferCompleted:
                UpdatedAt = transferCompleted.OccurredOn;
                break;
        }
    }
}
