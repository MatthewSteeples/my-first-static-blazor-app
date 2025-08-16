// Notification management for P4nda Tracker
window.P4ndaNotifications = {
    // Check if notifications are supported
    isSupported: function() {
        return 'Notification' in window && 'serviceWorker' in navigator;
    },

    // Get current notification permission status
    getPermission: function() {
        if (!this.isSupported()) return 'denied';
        return Notification.permission;
    },

    // Request notification permission
    requestPermission: async function() {
        if (!this.isSupported()) return 'denied';
        
        try {
            const permission = await Notification.requestPermission();
            return permission;
        } catch (error) {
            console.error('Error requesting notification permission:', error);
            return 'denied';
        }
    },

    // Schedule a notification using the service worker
    scheduleNotification: async function(itemName, intervalHours) {
        console.log('P4ndaNotifications: Attempting to schedule notification for', itemName, 'in', intervalHours, 'hours');
        
        if (!this.isSupported()) {
            console.log('P4ndaNotifications: Notifications not supported');
            return false;
        }
        
        if (Notification.permission !== 'granted') {
            console.log('P4ndaNotifications: Permission not granted, current permission:', Notification.permission);
            return false;
        }

        try {
            const registration = await navigator.serviceWorker.ready;
            const scheduledTime = Date.now() + (intervalHours * 60 * 60 * 1000);
            
            console.log('P4ndaNotifications: Scheduling notification for', new Date(scheduledTime));
            
            // Store the scheduled notification data
            const notificationData = {
                itemName: itemName,
                scheduledTime: scheduledTime,
                intervalHours: intervalHours
            };

            // Use postMessage to communicate with service worker
            registration.active.postMessage({
                type: 'SCHEDULE_NOTIFICATION',
                data: notificationData
            });

            console.log('P4ndaNotifications: Notification scheduled successfully');
            return true;
        } catch (error) {
            console.error('Error scheduling notification:', error);
            return false;
        }
    },

    // Cancel a scheduled notification
    cancelNotification: async function(itemName) {
        try {
            const registration = await navigator.serviceWorker.ready;
            registration.active.postMessage({
                type: 'CANCEL_NOTIFICATION',
                itemName: itemName
            });
            return true;
        } catch (error) {
            console.error('Error cancelling notification:', error);
            return false;
        }
    },

    // Show an immediate notification (for testing)
    showNotification: function(title, body, icon) {
        if (!this.isSupported() || Notification.permission !== 'granted') {
            return false;
        }

        try {
            new Notification(title, {
                body: body,
                icon: icon || '/icon-192.png',
                badge: '/icon-192.png',
                tag: 'medication-reminder'
            });
            return true;
        } catch (error) {
            console.error('Error showing notification:', error);
            return false;
        }
    }
};