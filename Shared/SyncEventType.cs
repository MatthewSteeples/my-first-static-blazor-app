using System.Text.Json.Serialization;

namespace BlazorApp.Shared
{
    [JsonConverter(typeof(JsonStringEnumConverter<SyncEventType>))]
    public enum SyncEventType
    {
        Created,
        Updated,
        Deleted
    }
}
