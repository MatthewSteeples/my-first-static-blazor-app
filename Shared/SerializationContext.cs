using System.Text.Json.Serialization;

namespace BlazorApp.Shared
{
    [JsonSourceGenerationOptions(WriteIndented = false, Converters = [typeof(TimeSpanJsonConverter)])]

    [JsonSerializable(typeof(TrackedItem))]
    [JsonSerializable(typeof(TrackedItem[]))]
    [JsonSerializable(typeof(Target))]
    [JsonSerializable(typeof(Occurrence))]
    [JsonSerializable(typeof(StockAcquisition))]
    [JsonSerializable(typeof(BrowserIdentity))]
    [JsonSerializable(typeof(ExportData))]
    [JsonSerializable(typeof(SyncEvent))]
    [JsonSerializable(typeof(AppSettings))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(string[]))]
    
    public partial class SerializationContext : JsonSerializerContext
    {
        
    }
}