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
        if (!this.isPBSSupported()) {
            return "unsupported";
        }
        
        // Note: There's no standard way to check PBS permission directly.
        // PBS permission is usually granted automatically based on user engagement
        // or when the app is installed as a PWA. We'll try to determine status
        // by attempting registration in a safe way.
        
        try {
            // Check if we have a service worker
            if (!('serviceWorker' in navigator)) {
                return "unsupported";
            }
            
            // Try to get service worker registration
            const registration = await navigator.serviceWorker.getRegistration();
            if (!registration) {
                // No service worker registered yet, need to register first
                return "prompt";
            }
            
            // Check if periodicSync is available on the registration
            if (!('periodicSync' in registration)) {
                return "unsupported";
            }
            
            // Try to get existing periodic sync registrations
            // This might indicate if PBS is working
            try {
                const tags = await registration.periodicSync.getTags();
                // If we can get tags without error, PBS is likely supported
                return "granted";
            } catch (e) {
                // If getting tags fails, it might be due to permission issues
                if (e.name === 'NotAllowedError') {
                    return "denied";
                } else {
                    // Other errors might indicate unsupported or unknown state
                    return "unknown";
                }
            }
            
        } catch (e) {
            alert('Error checking PBS permission: ' + e.message);
            return "unknown";
        }
    },
    
    // Request periodic background sync permission
    requestPBSPermission: async function() {
        if (!this.isPBSSupported()) {
            return "unsupported";
        }
        
        const status = await this.getPBSPermissionStatus();
        
        if (status === "denied") {
            alert("Background sync appears to be denied. For reliable medication reminders:\n\n" +
                  "1. Add this app to your home screen (install as PWA)\n" +
                  "2. Use the app regularly to increase engagement score\n" +
                  "3. Check browser settings for background sync permissions\n\n" +
                  "We'll use a fallback timer system for now.");
            return "denied";
        } else if (status === "prompt" || status === "unknown") {
            alert("To enable background medication reminders:\n\n" +
                  "1. Install this app by clicking 'Add to Home Screen' or the install button\n" +
                  "2. Use the app regularly\n" +
                  "3. Keep the app open or check it frequently\n\n" +
                  "For now, we'll try to register and fall back to timers if needed.");
            return "prompt";
        } else if (status === "unsupported") {
            alert("Your browser doesn't fully support background sync. We'll use timer-based notifications instead.");
            return "unsupported";
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
                alert('Background sync denied. Using timer-based notifications instead.');
            } else if (pbsPermission === "unsupported") {
                alert('Background sync not supported. Using timer-based notifications instead.');
            } else {
                // Permission is granted, prompt, or unknown - try to register
                try {
                    const registration = await navigator.serviceWorker.ready;
                    
                    // Try to register the periodic sync
                    await registration.periodicSync.register(tagName, {
                        minInterval: Math.max(intervalHours * 60 * 60 * 1000, 60000) // At least 1 minute
                    });
                    
                    alert(`Background notifications enabled for ${itemName}! You'll receive reminders every ${intervalHours} hours even when the app is closed.`);
                    return true;
                } catch (e) {
                    if (e.name === 'NotAllowedError') {
                        alert(`Background sync permission denied for ${itemName}. To enable:\n\n` +
                              `1. Install this app (Add to Home Screen)\n` +
                              `2. Use the app regularly\n` +
                              `3. Check browser permissions\n\n` +
                              `Using timer-based notifications instead.`);
                    } else {
                        alert(`Background sync registration failed: ${e.message}. Using timer-based notifications instead.`);
                    }
                    // Fall through to the fallback
                }
            }
        }
        
        // Fallback mechanism using a basic timer
        // This will only work while the tab is open
        alert(`Setting up timer-based notifications for ${itemName}. Keep this tab open for notifications to work.`);
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