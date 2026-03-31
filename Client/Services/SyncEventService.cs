using System.Text.Json;
using BlazorApp.Shared;
using Microsoft.JSInterop;

namespace BlazorApp.Client.Services;

public class SyncEventService : ISyncEventService
{
	private readonly IJSRuntime _jsRuntime;
	private readonly IAppSettingsService _appSettingsService;

	public SyncEventService(IJSRuntime jsRuntime, IAppSettingsService appSettingsService)
	{
		_jsRuntime = jsRuntime;
		_appSettingsService = appSettingsService;
	}

	public async Task EnqueueAsync(SyncEvent syncEvent)
	{
		if (!await _appSettingsService.GetExperimentalCloudflareApiPostingEnabledAsync())
			return;

		var json = JsonSerializer.Serialize(syncEvent, SerializationContext.Default.SyncEvent);
		await _jsRuntime.InvokeVoidAsync("syncPush.enqueueAndPushSyncEvent", json);
	}
}
