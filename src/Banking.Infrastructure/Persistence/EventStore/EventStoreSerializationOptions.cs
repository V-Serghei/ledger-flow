using System.Text.Json;

namespace Banking.Infrastructure.Persistence.EventStore;

internal static class EventStoreSerializationOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web);
}

