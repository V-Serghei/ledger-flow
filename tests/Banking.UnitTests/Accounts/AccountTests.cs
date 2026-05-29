using Banking.Domain.Accounts;
using Banking.Domain.Accounts.Events;
using Banking.Domain.Exceptions;
using Banking.Domain.ValueObjects;
using FluentAssertions;

namespace Banking.UnitTests.Accounts;

public sealed class AccountTests
{
    [Fact]
    public void Should_open_account_with_zero_balance()
    {
        var ownerUserId = UserId.New();

        var account = Account.Open(AccountId.New(), ownerUserId);

        account.OwnerUserId.Should().Be(ownerUserId);
        account.Status.Should().Be(AccountStatus.Active);
        account.Balance.Should().Be(Money.Zero);
        account.Version.Should().Be(1);
    }

    [Fact]
    public void Should_raise_account_opened_event()
    {
        var account = Account.Open(AccountId.New(), UserId.New());

        account.UncommittedEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AccountOpened>();
    }

    [Fact]
    public void Should_deposit_money_when_account_is_active()
    {
        var account = Account.Open(AccountId.New(), UserId.New());
        account.ClearUncommittedEvents();

        account.Deposit(new Money(100m), "Initial deposit");

        account.Balance.Should().Be(new Money(100m));
        account.UncommittedEvents.Should().ContainSingle()
            .Which.Should().BeOfType<MoneyDeposited>()
            .Which.BalanceAfter.Should().Be(new Money(100m));
    }

    [Fact]
    public void Should_not_deposit_negative_amount()
    {
        var account = Account.Open(AccountId.New(), UserId.New());

        var act = () => account.Deposit(new Money(-10m));

        act.Should().Throw<InvalidMoneyAmountException>();
    }

    [Fact]
    public void Should_not_deposit_zero_amount()
    {
        var account = Account.Open(AccountId.New(), UserId.New());

        var act = () => account.Deposit(Money.Zero);

        act.Should().Throw<InvalidMoneyAmountException>()
            .WithMessage("Operation amount must be greater than zero.");
    }

    [Fact]
    public void Should_not_deposit_when_account_is_inactive()
    {
        var account = Account.Open(AccountId.New(), UserId.New());
        account.Deactivate();

        var act = () => account.Deposit(new Money(10m));

        act.Should().Throw<InactiveAccountException>();
    }

    [Fact]
    public void Should_debit_when_account_has_enough_balance()
    {
        var account = Account.Open(AccountId.New(), UserId.New());
        account.Deposit(new Money(100m));
        account.ClearUncommittedEvents();

        account.Debit(new Money(40m), "Payment");

        account.Balance.Should().Be(new Money(60m));
        account.UncommittedEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AccountDebited>()
            .Which.BalanceAfter.Should().Be(new Money(60m));
    }

    [Fact]
    public void Should_not_withdraw_when_insufficient_funds()
    {
        var account = Account.Open(AccountId.New(), UserId.New());
        account.Deposit(new Money(30m));

        var act = () => account.Debit(new Money(50m));

        act.Should().Throw<InsufficientFundsException>();
        account.Balance.Should().Be(new Money(30m));
    }

    [Fact]
    public void Should_not_debit_when_account_is_inactive()
    {
        var account = Account.Open(AccountId.New(), UserId.New());
        account.Deposit(new Money(100m));
        account.Deactivate();

        var act = () => account.Debit(new Money(10m));

        act.Should().Throw<InactiveAccountException>();
    }

    [Fact]
    public void Should_activate_inactive_account()
    {
        var account = Account.Open(AccountId.New(), UserId.New());
        account.Deactivate();
        account.ClearUncommittedEvents();

        account.Activate();

        account.Status.Should().Be(AccountStatus.Active);
        account.UncommittedEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AccountActivated>();
    }
}

