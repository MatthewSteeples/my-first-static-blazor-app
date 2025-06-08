using BlazorApp.Shared;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorApp.Client.Services
{
    public class NotificationService
    {
        private readonly IJSRuntime _jsRuntime;

        public NotificationService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Checks if notifications are supported in the browser
        /// </summary>
        public async Task<bool> IsNotificationSupported()
        {
            return await _jsRuntime.InvokeAsync<bool>("notifications.isSupported");
        }

        /// <summary>
        /// Gets the current notification permission status
        /// </summary>
        /// <returns>Permission status: "granted", "denied", "default", or "unsupported"</returns>
        public async Task<string> GetPermissionStatus()
        {
            return await _jsRuntime.InvokeAsync<string>("notifications.getPermissionStatus");
        }

        /// <summary>
        /// Requests notification permission from the user
        /// </summary>
        /// <returns>Permission status: "granted", "denied", "default", or "unsupported"</returns>
        public async Task<string> RequestPermission()
        {
            return await _jsRuntime.InvokeAsync<string>("notifications.requestPermission");
        }

        /// <summary>
        /// Checks if periodic background sync is supported
        /// </summary>
        public async Task<bool> IsPeriodicBackgroundSyncSupported()
        {
            return await _jsRuntime.InvokeAsync<bool>("notifications.isPBSSupported");
        }

        /// <summary>
        /// Gets the current periodic background sync permission status
        /// </summary>
        /// <returns>Permission status: "granted", "denied", "prompt", "unknown", or "unsupported"</returns>
        public async Task<string> GetPBSPermissionStatus()
        {
            return await _jsRuntime.InvokeAsync<string>("notifications.getPBSPermissionStatus");
        }

        /// <summary>
        /// Requests periodic background sync permission
        /// </summary>
        /// <returns>Permission status: "granted", "denied", "prompt", "unknown", or "unsupported"</returns>
        public async Task<string> RequestPBSPermission()
        {
            return await _jsRuntime.InvokeAsync<string>("notifications.requestPBSPermission");
        }

        /// <summary>
        /// Registers a periodic background sync for medication reminders
        /// </summary>
        public async Task<bool> RegisterPeriodicSync(TrackedItem item)
        {
            // Check if the item has any scheduled targets
            var nextOccurrence = item.GetFutureOccurrences(DateTime.UtcNow, 1).FirstOrDefault();
            
            if (nextOccurrence == default)
                return false;
            
            // Add 10 minutes to the notification time before scheduling
            var notificationTime = nextOccurrence.AddMinutes(10);
            
            // Calculate hours until notification time (next occurrence + 10 minutes)
            double intervalHours = (notificationTime - DateTime.UtcNow).TotalHours;
            
            // Make sure we have at least a small interval (10 minutes minimum)
            if (intervalHours <= 0)
                intervalHours = 0.17; // About 10 minutes
            
            return await _jsRuntime.InvokeAsync<bool>(
                "notifications.registerPeriodicSync", 
                item.Id, 
                item.Name, 
                intervalHours);
        }

        /// <summary>
        /// Shows a notification directly (fallback option)
        /// </summary>
        public async Task<bool> ShowNotification(string title, string message)
        {
            return await _jsRuntime.InvokeAsync<bool>("notifications.showNotification", title, message);
        }
        
        /// <summary>
        /// Records that an item was tracked (for fallback timer)
        /// </summary>
        public async Task RecordTracking(TrackedItem item)
        {
            await _jsRuntime.InvokeVoidAsync("notifications.recordTracking", item.Id);
        }

        /// <summary>
        /// Gets the list of registered periodic sync tags
        /// </summary>
        /// <returns>Array of registered sync tag names</returns>
        public async Task<string[]> GetRegisteredTags()
        {
            return await _jsRuntime.InvokeAsync<string[]>("notifications.getRegisteredTags");
        }
    }
}