using Microsoft.JSInterop;

namespace BlazorApp.Client.Services;

public class PeriodicSyncService
{
	private readonly IJSRuntime _jsRuntime;
	private bool _initialized;

	public PeriodicSyncService(IJSRuntime jsRuntime)
	{
		_jsRuntime = jsRuntime;
	}

	public async Task InitializeAsync()
	{
		if (_initialized)
			return;

		_initialized = true;
		await _jsRuntime.InvokeVoidAsync("periodicSyncSetup.initialize");
	}
}
