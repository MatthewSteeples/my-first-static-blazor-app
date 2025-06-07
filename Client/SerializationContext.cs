using BlazorApp.Shared;
using System.Text.Json.Serialization;

namespace BlazorApp.Client
{
    [JsonSourceGenerationOptions(WriteIndented = false)]

    [JsonSerializable(typeof(TrackedItem))]
    [JsonSerializable(typeof(TrackedItem[]))]
    [JsonSerializable(typeof(Target))]
    [JsonSerializable(typeof(Occurrence))]
    
    public partial class SerializationContext : JsonSerializerContext
    {
        
    }
}
