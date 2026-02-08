using Microsoft.JSInterop;

namespace BlazorApp.Client.Services;

public class PushNotificationService
{
	private readonly IJSRuntime _jsRuntime;
	private bool _initialized;

	public PushNotificationService(IJSRuntime jsRuntime)
	{
		_jsRuntime = jsRuntime;
	}

	public async Task<bool> InitializeAsync()
	{
		if (_initialized)
			return true;

		try
		{
			_initialized = await _jsRuntime.InvokeAsync<bool>("pushNotifications.initialize");
			return _initialized;
		}
		catch
		{
			return false;
		}
	}

	public async Task<string> RequestPermissionAsync()
	{
		try
		{
			return await _jsRuntime.InvokeAsync<string>("pushNotifications.requestPermission");
		}
		catch
		{
			return "error";
		}
	}

	public async Task<bool> SubscribeAsync()
	{
		try
		{
			var result = await _jsRuntime.InvokeAsync<object>("pushNotifications.subscribe");
			return result != null;
		}
		catch
		{
			return false;
		}
	}

	public async Task<NotificationScheduleResult> ScheduleNotificationAsync(
		string trackedItemId, 
		string trackedItemName, 
		DateTime nextOccurrenceTime)
	{
		try
		{
			var result = await _jsRuntime.InvokeAsync<NotificationScheduleResult>(
				"pushNotifications.scheduleNotification",
				trackedItemId,
				trackedItemName,
				nextOccurrenceTime);
			
			return result ?? new NotificationScheduleResult { Success = false };
		}
		catch (Exception ex)
		{
			return new NotificationScheduleResult 
			{ 
				Success = false, 
				Error = ex.Message 
			};
		}
	}
}

public class NotificationScheduleResult
{
	public bool Success { get; set; }
	public string? ScheduledFor { get; set; }
	public int? DelayMs { get; set; }
	public string? Message { get; set; }
	public string? Error { get; set; }
}
