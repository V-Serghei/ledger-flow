using System.Text.Json.Serialization;
using Banking.Domain.Exceptions;

namespace Banking.Domain.ValueObjects;

public readonly record struct Money : IComparable<Money>
{
    [JsonConstructor]
    public Money(decimal amount)
    {
        if (amount < 0)
        {
            throw new InvalidMoneyAmountException("Money amount cannot be negative.");
        }

        Amount = decimal.Round(amount, 2, MidpointRounding.ToEven);
    }

    public decimal Amount { get; }

    public static Money Zero => new(0m);

    public static Money FromOperationAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new InvalidMoneyAmountException("Operation amount must be greater than zero.");
        }

        return new Money(amount);
    }

    public int CompareTo(Money other) => Amount.CompareTo(other.Amount);

    public static Money operator +(Money left, Money right) => new(left.Amount + right.Amount);

    public static Money operator -(Money left, Money right) => new(left.Amount - right.Amount);

    public static bool operator >(Money left, Money right) => left.Amount > right.Amount;

    public static bool operator <(Money left, Money right) => left.Amount < right.Amount;

    public static bool operator >=(Money left, Money right) => left.Amount >= right.Amount;

    public static bool operator <=(Money left, Money right) => left.Amount <= right.Amount;

    public override string ToString() => Amount.ToString("F2");
}
