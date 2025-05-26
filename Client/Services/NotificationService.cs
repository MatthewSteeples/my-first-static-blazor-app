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
        /// Registers a periodic background sync for medication reminders
        /// </summary>
        public async Task<bool> RegisterPeriodicSync(TrackedItem item)
        {
            // Check if the item has any scheduled targets
            var eligibleTargets = item.Targets.Where(t => t.Qty > 0 && t.Frequency > TimeSpan.Zero).ToList();
            
            if (!eligibleTargets.Any())
                return false;
            
            // Get the interval in hours from the first eligible target
            double intervalHours = eligibleTargets.First().Frequency.TotalHours;
            
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
    }
}