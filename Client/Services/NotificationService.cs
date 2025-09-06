using Microsoft.JSInterop;

namespace BlazorApp.Client.Services
{
    public class NotificationService
    {
        private readonly IJSRuntime _jsRuntime;

        public NotificationService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<bool> IsSupportedAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<bool>("P4ndaNotifications.isSupported");
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetPermissionAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("P4ndaNotifications.getPermission");
            }
            catch
            {
                return "denied";
            }
        }

        public async Task<string> RequestPermissionAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("P4ndaNotifications.requestPermission");
            }
            catch
            {
                return "denied";
            }
        }

        public async Task<bool> ScheduleNotificationAsync(string itemName, double intervalHours)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<bool>("P4ndaNotifications.scheduleNotification", itemName, intervalHours);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CancelNotificationAsync(string itemName)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<bool>("P4ndaNotifications.cancelNotification", itemName);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ShowNotificationAsync(string title, string body, string? icon = null)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<bool>("P4ndaNotifications.showNotification", title, body, icon);
            }
            catch
            {
                return false;
            }
        }
    }
}