// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).
self.addEventListener('fetch', () => { });

// Handle messages from the main thread for notifications
self.addEventListener('message', event => onMessage(event));

// Notification storage
const notificationStore = new Map();

// Handle messages from the main thread
async function onMessage(event) {
    console.log('Service Worker: Received message', event.data);
    const { type, data, itemName } = event.data;
    
    switch (type) {
        case 'SCHEDULE_NOTIFICATION':
            console.log('Service Worker: Scheduling notification for', data.itemName);
            await scheduleNotification(data);
            break;
        case 'CANCEL_NOTIFICATION':
            console.log('Service Worker: Cancelling notification for', itemName);
            await cancelNotification(itemName);
            break;
        default:
            console.log('Service Worker: Unknown message type', type);
    }
}

// Schedule a notification
async function scheduleNotification(notificationData) {
    const { itemName, scheduledTime, intervalHours } = notificationData;
    
    console.log('Service Worker: scheduleNotification called for', itemName, 'at', new Date(scheduledTime));
    
    // Cancel any existing notification for this item
    await cancelNotification(itemName);
    
    // Calculate delay until notification should be shown
    const delay = scheduledTime - Date.now();
    
    console.log('Service Worker: Delay is', delay, 'ms');
    
    if (delay > 0) {
        const timeoutId = setTimeout(() => {
            console.log('Service Worker: Timeout fired, showing notification for', itemName);
            showMedicationNotification(itemName, intervalHours);
        }, delay);
        
        // Store the timeout ID for later cancellation
        notificationStore.set(itemName, timeoutId);
        console.log('Service Worker: Notification scheduled with timeout ID', timeoutId);
    } else {
        console.log('Service Worker: Delay is not positive, not scheduling notification');
    }
}

// Cancel a scheduled notification
async function cancelNotification(itemName) {
    const timeoutId = notificationStore.get(itemName);
    if (timeoutId) {
        clearTimeout(timeoutId);
        notificationStore.delete(itemName);
    }
}

// Show a medication reminder notification
function showMedicationNotification(itemName, intervalHours) {
    const title = `Time for ${itemName}`;
    const body = `It's been ${intervalHours} hour${intervalHours !== 1 ? 's' : ''} since your last ${itemName}. Don't forget to take it!`;
    
    self.registration.showNotification(title, {
        body: body,
        icon: '/icon-192.png',
        badge: '/icon-192.png',
        tag: `medication-${itemName}`,
        requireInteraction: true,
        actions: [
            {
                action: 'track',
                title: 'Mark as Taken'
            },
            {
                action: 'dismiss',
                title: 'Dismiss'
            }
        ]
    });
    
    // Remove from store after showing
    notificationStore.delete(itemName);
}

// Handle notification clicks
self.addEventListener('notificationclick', event => {
    event.notification.close();
    
    if (event.action === 'track') {
        // TODO: We could send a message back to the main app to automatically track the item
        // For now, just open the app
        event.waitUntil(
            clients.openWindow('/')
        );
    } else if (event.action === 'dismiss') {
        // Just close the notification
        return;
    } else {
        // Default click action - open the app
        event.waitUntil(
            clients.openWindow('/')
        );
    }
});
