using System.Text.Json;
using BlazorApp.Shared;
using Microsoft.JSInterop;

namespace BlazorApp.Client.Services;

public class SyncEventService : ISyncEventService
{
	private readonly IJSRuntime _jsRuntime;

	public SyncEventService(IJSRuntime jsRuntime)
	{
		_jsRuntime = jsRuntime;
	}

	public async Task EnqueueAsync(SyncEvent syncEvent)
	{
		var json = JsonSerializer.Serialize(syncEvent, SerializationContext.Default.SyncEvent);
		await _jsRuntime.InvokeVoidAsync("syncPush.enqueueAndPushSyncEvent", json);
	}
}
