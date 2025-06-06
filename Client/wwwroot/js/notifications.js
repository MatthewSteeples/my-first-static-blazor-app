// Notification helper functions

// Check if notification permission is granted
window.notifications = {
    // Check if notifications are supported
    isSupported: function() {
        return 'Notification' in window;
    },
    
    // Check if periodic background sync is supported
    isPBSSupported: function() {
        return 'serviceWorker' in navigator && 
               'ServiceWorkerRegistration' in window && 
               'periodicSync' in ServiceWorkerRegistration.prototype;
    },
    
    // Get current notification permission status
    getPermissionStatus: function() {
        if (!this.isSupported()) {
            return "unsupported";
        }
        return Notification.permission;
    },
    
    // Request notification permission
    requestPermission: async function() {
        if (!this.isSupported()) {
            return "unsupported";
        }
        
        try {
            const permission = await Notification.requestPermission();
            return permission;
        } catch (e) {
            alert('Error requesting notification permission: ' + e.message);
            return "error";
        }
    },
    
    // Register a periodic background sync task
    registerPeriodicSync: async function(itemId, itemName, intervalHours) {
        // Store the item information
        const tagName = `medication-reminder-${itemId}`;
        const itemInfo = {
            id: itemId,
            name: itemName,
            intervalHours: intervalHours
        };
        
        // Always store the item info, used by both PBS and fallback timer
        localStorage.setItem(tagName, JSON.stringify(itemInfo));
        
        // If PBS is supported, register it
        if (this.isPBSSupported()) {
            try {
                const registration = await navigator.serviceWorker.ready;
                
                await registration.periodicSync.register(tagName, {
                    minInterval: intervalHours * 60 * 60 * 1000 // Convert hours to milliseconds
                });
                
                return true;
            } catch (e) {
                alert('Error registering periodic sync: ' + e.message);
                // Fall through to the fallback
            }
        }
        
        // Fallback mechanism using a basic timer
        // This will only work while the tab is open
        console.log(`Periodic background sync not supported or failed. Using fallback timer for ${itemName}`);
        this.setupFallbackTimer(itemId, itemName, intervalHours);
        
        return true;
    },
    
    // Setup a fallback timer for browsers without periodic background sync
    setupFallbackTimer: function(itemId, itemName, intervalHours) {
        // Convert hours to milliseconds
        const interval = intervalHours * 60 * 60 * 1000;
        
        // Check when the item was last tracked
        const tagName = `medication-reminder-${itemId}`;
        const lastTrackedKey = `last-tracked-${itemId}`;
        const lastTracked = localStorage.getItem(lastTrackedKey) 
            ? new Date(localStorage.getItem(lastTrackedKey)) 
            : new Date();
        
        // Calculate when the next notification should show
        const now = new Date();
        const timeSinceLastTracked = now - lastTracked;
        
        // If it's time for a notification or past time, show it
        if (timeSinceLastTracked >= interval) {
            this.showNotification(`Time for ${itemName}`, `It's time to take your ${itemName}`);
            localStorage.setItem(lastTrackedKey, now.toISOString());
        }
        
        // Set a timer for the next interval
        setTimeout(() => {
            this.setupFallbackTimer(itemId, itemName, intervalHours);
        }, interval);
    },
    
    // Show a notification directly
    showNotification: async function(title, message) {
        if (Notification.permission !== 'granted') {
            alert('Notification permission not granted');
            return false;
        }
        
        try {
            const registration = await navigator.serviceWorker.ready;
            await registration.showNotification(title, {
                body: message,
                icon: '/icon-192.png',
                badge: '/icon-192.png',
                vibrate: [200, 100, 200]
            });
            return true;
        } catch (e) {
            alert('Error showing notification: ' + e.message);
            return false;
        }
    },
    
    // Record that an item was tracked (for fallback timer)
    recordTracking: function(itemId) {
        const lastTrackedKey = `last-tracked-${itemId}`;
        localStorage.setItem(lastTrackedKey, new Date().toISOString());
    }
};