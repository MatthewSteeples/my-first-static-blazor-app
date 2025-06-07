using System.Text.Json.Serialization;

namespace BlazorApp.Shared
{
    [JsonSourceGenerationOptions(WriteIndented = false)]

    [JsonSerializable(typeof(TrackedItem))]
    [JsonSerializable(typeof(TrackedItem[]))]
    [JsonSerializable(typeof(Target))]
    [JsonSerializable(typeof(Occurrence))]
    [JsonSerializable(typeof(StockAcquisition))]
    
    public partial class SerializationContext : JsonSerializerContext
    {
        
    }
}