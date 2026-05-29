namespace Banking.Application.Messaging;

public interface IOutboxWriter
{
    void Add<TMessage>(
        TMessage message,
        Guid correlationId,
        IReadOnlyDictionary<string, string>? headers = null)
        where TMessage : notnull;
}

