using Banking.Domain.Exceptions;
using Banking.Domain.ValueObjects;
using FluentAssertions;

namespace Banking.UnitTests.ValueObjects;

public sealed class MoneyTests
{
    [Fact]
    public void Should_create_money_with_positive_amount()
    {
        var money = new Money(10.123m);

        money.Amount.Should().Be(10.12m);
    }

    [Fact]
    public void Should_create_zero_money()
    {
        var money = Money.Zero;

        money.Amount.Should().Be(0m);
    }

    [Fact]
    public void Should_not_create_money_with_negative_amount()
    {
        var act = () => new Money(-1m);

        act.Should().Throw<InvalidMoneyAmountException>()
            .WithMessage("Money amount cannot be negative.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_not_create_operation_money_with_non_positive_amount(decimal amount)
    {
        var act = () => Money.FromOperationAmount(amount);

        act.Should().Throw<InvalidMoneyAmountException>()
            .WithMessage("Operation amount must be greater than zero.");
    }

    [Fact]
    public void Should_add_money()
    {
        var result = new Money(10m) + new Money(15.5m);

        result.Amount.Should().Be(25.5m);
    }
}

