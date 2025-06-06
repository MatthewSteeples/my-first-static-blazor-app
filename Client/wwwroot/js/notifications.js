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
    
    // Check if periodic background sync permission is granted
    getPBSPermissionStatus: async function() {
        if (!this.isPBSSupported() || !('permissions' in navigator)) {
            return "unsupported";
        }
        
        try {
            const permissionStatus = await navigator.permissions.query({ name: 'periodic-background-sync' });
            return permissionStatus.state;
        } catch (e) {
            // Some browsers might not support the permission query for PBS
            return "unknown";
        }
    },
    
    // Request periodic background sync permission
    requestPBSPermission: async function() {
        if (!this.isPBSSupported()) {
            return "unsupported";
        }
        
        // Note: Periodic Background Sync permission is typically granted implicitly
        // when the user adds the app to their home screen or when they use the app frequently.
        // There's no explicit permission request API for PBS like there is for notifications.
        // We'll check the current status and inform the user accordingly.
        
        const status = await this.getPBSPermissionStatus();
        if (status === "denied") {
            alert("Background sync permission is denied. Please enable it in your browser settings or add this app to your home screen.");
            return "denied";
        } else if (status === "prompt" || status === "unknown") {
            // For PBS, we can't explicitly request permission, but we can try to register
            // and see if it succeeds. The browser will handle the permission automatically.
            return "prompt";
        }
        
        return status;
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
        
        // If PBS is supported, check permission and register it
        if (this.isPBSSupported()) {
            // First check if we have permission for periodic background sync
            const pbsPermission = await this.requestPBSPermission();
            
            if (pbsPermission === "denied") {
                alert('Background sync permission denied. Using fallback timer instead.');
            } else if (pbsPermission === "unsupported") {
                alert('Background sync not supported on this browser. Using fallback timer instead.');
            } else {
                // Permission is granted, prompt, or unknown - try to register
                try {
                    const registration = await navigator.serviceWorker.ready;
                    
                    await registration.periodicSync.register(tagName, {
                        minInterval: intervalHours * 60 * 60 * 1000 // Convert hours to milliseconds
                    });
                    
                    return true;
                } catch (e) {
                    alert('Error registering periodic sync: ' + e.message + '. Using fallback timer instead.');
                    // Fall through to the fallback
                }
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