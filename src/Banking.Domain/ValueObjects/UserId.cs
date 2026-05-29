using System.Text.Json.Serialization;

namespace Banking.Domain.ValueObjects;

public readonly record struct UserId
{
    [JsonConstructor]
    public UserId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static UserId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
