namespace Banking.Domain.Exceptions;

public sealed class SameAccountTransferException : DomainException
{
    public SameAccountTransferException(string message)
        : base(message)
    {
    }
}

