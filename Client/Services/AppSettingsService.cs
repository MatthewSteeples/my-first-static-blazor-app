using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using Microsoft.JSInterop;

namespace BlazorApp.Client.Services;

public interface IAppSettingsService
{
    Task<AppSettings> GetAsync();
    Task<bool> GetExperimentalCloudflareApiPostingEnabledAsync();
    Task SetExperimentalCloudflareApiPostingEnabledAsync(bool enabled);
}

public sealed class AppSettings
{
    [JsonPropertyName("experimentalCloudflareApiPostingEnabled")]
    public bool ExperimentalCloudflareApiPostingEnabled { get; set; }
}

public sealed class AppSettingsService : IAppSettingsService
{
    public const string StorageKey = "app-settings:v1";

    private readonly ILocalStorageService _localStorage;
    private readonly IJSRuntime _jsRuntime;
    private AppSettings? _cachedSettings;

    public AppSettingsService(ILocalStorageService localStorage, IJSRuntime jsRuntime)
    {
        _localStorage = localStorage;
        _jsRuntime = jsRuntime;
    }

    public async Task<AppSettings> GetAsync()
    {
        if (_cachedSettings != null)
            return _cachedSettings;

        _cachedSettings = await _localStorage.GetItemAsync<AppSettings>(StorageKey) ?? new AppSettings();
        return _cachedSettings;
    }

    public async Task<bool> GetExperimentalCloudflareApiPostingEnabledAsync()
    {
        var settings = await GetAsync();
        return settings.ExperimentalCloudflareApiPostingEnabled;
    }

    public async Task SetExperimentalCloudflareApiPostingEnabledAsync(bool enabled)
    {
        var settings = await GetAsync();
        if (settings.ExperimentalCloudflareApiPostingEnabled == enabled)
            return;

        settings.ExperimentalCloudflareApiPostingEnabled = enabled;
        await _localStorage.SetItemAsync(StorageKey, settings);
        await _jsRuntime.InvokeVoidAsync("syncPostingSettings.setEnabled", enabled);
    }
}
