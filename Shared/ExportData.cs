using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorApp.Shared
{
    public class ExportData
    {
        public BrowserIdentity Identity { get; set; } = new();
        
        public List<TrackedItem> TrackedItems { get; set; } = new();
    }
}