using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorApp.Shared
{
    public class ExportData
    {
        [JsonPropertyName("identity")]
        public BrowserIdentity Identity { get; set; } = new();
        
        [JsonPropertyName("trackedItems")]
        public List<string> TrackedItems { get; set; } = new();
    }
}