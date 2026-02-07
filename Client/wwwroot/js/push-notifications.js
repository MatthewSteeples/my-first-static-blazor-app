// Push Notification helpers for Blazor Tracker
// Handles permission requests, subscription management, and notification scheduling

(function () {
	const NOTIFICATION_PERMISSION_KEY = 'push-notifications:permission:v1';
	const NOTIFICATION_SUBSCRIPTION_KEY = 'push-notifications:subscription:v1';

	async function urlBase64ToUint8Array(base64String) {
		const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
		const base64 = (base64String + padding)
			.replace(/\-/g, '+')
			.replace(/_/g, '/');

		const rawData = window.atob(base64);
		const outputArray = new Uint8Array(rawData.length);

		for (let i = 0; i < rawData.length; ++i) {
			outputArray[i] = rawData.charCodeAt(i);
		}
		return outputArray;
	}

	async function subscribeToPushNotifications() {
		if (!('serviceWorker' in navigator)) {
			console.warn('Service Worker not supported');
			return null;
		}

		if (!('PushManager' in window)) {
			console.warn('Push notifications not supported');
			return null;
		}

		try {
			const registration = await navigator.serviceWorker.ready;

			// Check for existing subscription
			let subscription = await registration.pushManager.getSubscription();

			if (!subscription) {
				// TODO: Replace with actual VAPID public key
				// For now, using a placeholder - in production this would come from the server
				const vapidPublicKey = 'PLACEHOLDER_VAPID_PUBLIC_KEY';
				
				// Only try to subscribe if we have a valid VAPID key
				if (vapidPublicKey === 'PLACEHOLDER_VAPID_PUBLIC_KEY') {
					console.warn('VAPID public key not configured - skipping push subscription');
					return null;
				}

				const convertedVapidKey = await urlBase64ToUint8Array(vapidPublicKey);

				subscription = await registration.pushManager.subscribe({
					userVisibleOnly: true,
					applicationServerKey: convertedVapidKey
				});
			}

			// Store subscription in localStorage for easy access
			if (subscription) {
				window.localStorage.setItem(
					NOTIFICATION_SUBSCRIPTION_KEY,
					JSON.stringify(subscription.toJSON())
				);
			}

			return subscription ? subscription.toJSON() : null;
		} catch (error) {
			console.error('Failed to subscribe to push notifications:', error);
			return null;
		}
	}

	async function requestNotificationPermission() {
		const existing = window.localStorage?.getItem(NOTIFICATION_PERMISSION_KEY);
		
		if (existing === 'granted') {
			return 'granted';
		}
		
		if (existing === 'denied') {
			return 'denied';
		}

		if (!('Notification' in window)) {
			console.warn('Notifications not supported');
			window.localStorage?.setItem(NOTIFICATION_PERMISSION_KEY, 'unsupported');
			return 'unsupported';
		}

		// Check current permission
		if (Notification.permission === 'granted') {
			window.localStorage?.setItem(NOTIFICATION_PERMISSION_KEY, 'granted');
			return 'granted';
		}

		if (Notification.permission === 'denied') {
			window.localStorage?.setItem(NOTIFICATION_PERMISSION_KEY, 'denied');
			return 'denied';
		}

		// Request permission
		const permission = await Notification.requestPermission();
		window.localStorage?.setItem(NOTIFICATION_PERMISSION_KEY, permission);
		
		return permission;
	}

	async function scheduleNotification(trackedItemId, trackedItemName, nextOccurrenceTime) {
		try {
			// Get push subscription
			const subscriptionJson = window.localStorage?.getItem(NOTIFICATION_SUBSCRIPTION_KEY);
			const subscription = subscriptionJson ? JSON.parse(subscriptionJson) : null;

			// Get JWT for authentication (reusing existing periodic sync logic)
			let token = null;
			if (window.periodicSyncSetup?.getJwt) {
				token = await window.periodicSyncSetup.getJwt();
			}

			const payload = {
				trackedItemId,
				trackedItemName,
				nextOccurrenceTime: nextOccurrenceTime.getTime(), // Convert Date to Unix timestamp
				subscription
			};

			const headers = {
				'Content-Type': 'application/json'
			};

			if (token) {
				headers['Authorization'] = `Bearer ${token}`;
			}

			const response = await fetch('/api/notifications/schedule', {
				method: 'POST',
				headers,
				body: JSON.stringify(payload)
			});

			if (!response.ok) {
				const error = await response.json();
				console.error('Failed to schedule notification:', error);
				return { success: false, error };
			}

			const result = await response.json();
			console.log('Notification scheduled:', result);
			return { success: true, ...result };
		} catch (error) {
			console.error('Error scheduling notification:', error);
			return { success: false, error: error.message };
		}
	}

	// Export public API
	window.pushNotifications = {
		requestPermission: requestNotificationPermission,
		subscribe: subscribeToPushNotifications,
		scheduleNotification: scheduleNotification,
		
		// Initialize: request permission and subscribe
		initialize: async function () {
			const permission = await requestNotificationPermission();
			if (permission === 'granted') {
				await subscribeToPushNotifications();
				return true;
			}
			return false;
		}
	};
})();
