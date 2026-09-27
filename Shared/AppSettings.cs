using System.Text.Json.Serialization;

namespace BlazorApp.Shared;

public sealed class AppSettings
{
    [JsonPropertyName("experimentalCloudflareApiPostingEnabled")]
    public bool ExperimentalCloudflareApiPostingEnabled { get; set; }
}
