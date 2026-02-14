using BlazorApp.Shared;

namespace BlazorApp.Client.Services;

public interface ISyncEventService
{
	Task EnqueueAsync(SyncEvent syncEvent);
}
