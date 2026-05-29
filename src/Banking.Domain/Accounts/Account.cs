using Banking.Domain.Accounts.Events;
using Banking.Domain.Events;
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

    public void ClearUncommittedEvents() => _uncommittedEvents.Clear();

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
        }
    }
}

