namespace Banking.Domain.Exceptions;

public sealed class InvalidMoneyAmountException : DomainException
{
    public InvalidMoneyAmountException(string message)
        : base(message)
    {
    }
}

