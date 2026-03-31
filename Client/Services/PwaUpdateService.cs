using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorApp.Client.Services;

public class PwaUpdateService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<PwaUpdateService>? _dotNetReference;
    private bool _isUpdateAvailable = false;
    private bool _isInitialized = false;

    public event Action? UpdateAvailable;

    public PwaUpdateService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public bool IsUpdateAvailable => _isUpdateAvailable;

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        _dotNetReference = DotNetObjectReference.Create(this);
        await _jsRuntime.InvokeVoidAsync("pwaUpdate.initialize", _dotNetReference);
        _isInitialized = true;
    }

    [JSInvokable]
    public void OnUpdateAvailable()
    {
        _isUpdateAvailable = true;
        UpdateAvailable?.Invoke();
    }

    public async Task ReloadApplicationAsync()
    {
        await _jsRuntime.InvokeVoidAsync("pwaUpdate.reloadApplication");
    }

    public async Task CheckForUpdatesAsync()
    {
        await _jsRuntime.InvokeVoidAsync("pwaUpdate.checkForUpdates");
    }

    public ValueTask DisposeAsync()
    {
        _dotNetReference?.Dispose();
        return ValueTask.CompletedTask;
    }
}
