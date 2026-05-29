using Banking.Domain.Accounts;
using Banking.Domain.Accounts.Events;
using Banking.Domain.Exceptions;
using Banking.Domain.ValueObjects;
using FluentAssertions;

namespace Banking.UnitTests.Accounts;

public sealed class TransferTests
{
    [Fact]
    public void Should_transfer_between_active_accounts()
    {
        var source = Account.Open(AccountId.New(), UserId.New());
        var destination = Account.Open(AccountId.New(), UserId.New());
        source.Deposit(new Money(100m));
        source.ClearUncommittedEvents();
        destination.ClearUncommittedEvents();

        source.TransferTo(destination, new Money(50m), "Transfer to another account");

        source.Balance.Should().Be(new Money(50m));
        destination.Balance.Should().Be(new Money(50m));
    }

    [Fact]
    public void Should_raise_transfer_completed_event()
    {
        var source = Account.Open(AccountId.New(), UserId.New());
        var destination = Account.Open(AccountId.New(), UserId.New());
        source.Deposit(new Money(100m));
        source.ClearUncommittedEvents();
        destination.ClearUncommittedEvents();

        source.TransferTo(destination, new Money(50m), "Transfer to another account");

        source.UncommittedEvents.Should().ContainSingle(e => e is TransferCompleted)
            .Which.Should().BeOfType<TransferCompleted>()
            .Which.Amount.Should().Be(new Money(50m));
        source.UncommittedEvents.Should().ContainSingle(e => e is AccountDebited);
        destination.UncommittedEvents.Should().ContainSingle(e => e is AccountCredited);
    }

    [Fact]
    public void Should_not_transfer_from_inactive_account()
    {
        var source = Account.Open(AccountId.New(), UserId.New());
        var destination = Account.Open(AccountId.New(), UserId.New());
        source.Deposit(new Money(100m));
        source.Deactivate();

        var act = () => source.TransferTo(destination, new Money(50m));

        act.Should().Throw<InactiveAccountException>();
        source.Balance.Should().Be(new Money(100m));
        destination.Balance.Should().Be(Money.Zero);
    }

    [Fact]
    public void Should_not_transfer_to_inactive_account()
    {
        var source = Account.Open(AccountId.New(), UserId.New());
        var destination = Account.Open(AccountId.New(), UserId.New());
        source.Deposit(new Money(100m));
        destination.Deactivate();

        var act = () => source.TransferTo(destination, new Money(50m));

        act.Should().Throw<InactiveAccountException>();
        source.Balance.Should().Be(new Money(100m));
        destination.Balance.Should().Be(Money.Zero);
    }

    [Fact]
    public void Should_not_transfer_to_same_account()
    {
        var account = Account.Open(AccountId.New(), UserId.New());
        account.Deposit(new Money(100m));

        var act = () => account.TransferTo(account, new Money(50m));

        act.Should().Throw<SameAccountTransferException>();
        account.Balance.Should().Be(new Money(100m));
    }

    [Fact]
    public void Should_not_transfer_when_source_has_insufficient_funds()
    {
        var source = Account.Open(AccountId.New(), UserId.New());
        var destination = Account.Open(AccountId.New(), UserId.New());
        source.Deposit(new Money(30m));

        var act = () => source.TransferTo(destination, new Money(50m));

        act.Should().Throw<InsufficientFundsException>();
        source.Balance.Should().Be(new Money(30m));
        destination.Balance.Should().Be(Money.Zero);
    }
}

