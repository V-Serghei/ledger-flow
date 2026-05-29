namespace Banking.Domain.Exceptions;

public sealed class InactiveAccountException : DomainException
{
    public InactiveAccountException(string message)
        : base(message)
    {
    }
}

