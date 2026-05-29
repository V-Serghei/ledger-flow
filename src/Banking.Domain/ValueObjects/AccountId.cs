using System.Text.Json.Serialization;

namespace Banking.Domain.ValueObjects;

public readonly record struct AccountId
{
    [JsonConstructor]
    public AccountId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Account id cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static AccountId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
