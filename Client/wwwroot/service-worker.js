// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).
self.addEventListener('fetch', () => { });

const authDbName = 'BlazorTracker';
const authDbVersion = 1;
const authStoreName = 'auth';
const authJwtKey = 'jwt';

function openAuthDb() {
	return new Promise((resolve, reject) => {
		const request = indexedDB.open(authDbName, authDbVersion);
		request.onupgradeneeded = () => {
			const db = request.result;
			if (!db.objectStoreNames.contains(authStoreName)) {
				db.createObjectStore(authStoreName);
			}
		};
		request.onsuccess = () => resolve(request.result);
		request.onerror = () => reject(request.error);
	});
}

async function getStoredJwt() {
	try {
		const db = await openAuthDb();
		return await new Promise((resolve, reject) => {
			const tx = db.transaction(authStoreName, 'readonly');
			const req = tx.objectStore(authStoreName).get(authJwtKey);
			req.onsuccess = () => resolve(req.result);
			req.onerror = () => reject(req.error);
		});
	} catch {
		return null;
	}
}

self.addEventListener('periodicsync', event => event.waitUntil(onPeriodicSync(event)));

async function onPeriodicSync(event) {
	if (event?.tag !== 'sync') {
		return;
	}

	try {
		const token = await getStoredJwt();
		if (!token) {
			console.warn('Service worker (dev): No stored JWT; skipping /api/sync');
			return;
		}

		const response = await fetch('/api/sync', {
			method: 'POST',
			headers: {
				'Authorization': `Bearer ${token}`,
				'Content-Type': 'application/json'
			},
			body: '{}',
			cache: 'no-store'
		});

		if (!response.ok) {
			console.warn('Service worker (dev): /api/sync failed', response.status);
		}
	} catch (error) {
		console.warn('Service worker (dev): /api/sync threw', error);
	}
}

// Handle push notifications
self.addEventListener('push', event => event.waitUntil(onPushNotification(event)));

async function onPushNotification(event) {
	try {
		let notificationData;

		if (event.data) {
			notificationData = event.data.json();
		} else {
			// Fallback notification if no data provided
			notificationData = {
				title: 'Medication Reminder',
				body: 'Time to check your medication tracker',
				icon: '/icon-192.png',
				badge: '/badge-72.png',
				data: {
					url: '/'
				}
			};
		}

		const { title, body, icon, badge, data } = notificationData;

		await self.registration.showNotification(title, {
			body,
			icon: icon || '/icon-192.png',
			badge: badge || '/badge-72.png',
			data: data || {},
			requireInteraction: true,
			tag: data?.trackedItemId || 'medication-reminder'
		});
	} catch (error) {
		console.error('Service worker (dev): Push notification error', error);
	}
}

// Handle notification clicks
self.addEventListener('notificationclick', event => {
	event.notification.close();

	event.waitUntil(
		clients.matchAll({ type: 'window', includeUncontrolled: true })
			.then(clientList => {
				// Check if there's already an open window/tab
				for (const client of clientList) {
					if (client.url === event.notification.data?.url && 'focus' in client) {
						return client.focus();
					}
				}

				// Open a new window/tab if none exists
				if (clients.openWindow) {
					return clients.openWindow(event.notification.data?.url || '/');
				}
			})
	);
});

