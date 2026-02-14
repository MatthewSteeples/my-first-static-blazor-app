// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).
self.addEventListener('fetch', () => { });

const authDbName = 'BlazorTracker';
const authDbVersion = 2;
const authStoreName = 'auth';
const authJwtKey = 'jwt';
const syncStoreName = 'syncQueue';

function openAuthDb() {
	return new Promise((resolve, reject) => {
		const request = indexedDB.open(authDbName, authDbVersion);
		request.onupgradeneeded = () => {
			const db = request.result;
			if (!db.objectStoreNames.contains(authStoreName)) {
				db.createObjectStore(authStoreName);
			}
			if (!db.objectStoreNames.contains(syncStoreName)) {
				db.createObjectStore(syncStoreName, { keyPath: 'id' });
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

		// Also flush any pending sync events as a catch-up
		await flushPendingSyncEvents(token);
	} catch (error) {
		console.warn('Service worker (dev): /api/sync threw', error);
	}
}

// Background Sync API handler for individual event push
self.addEventListener('sync', event => {
	if (event.tag === 'sync-event') {
		event.waitUntil(onBackgroundSync());
	}
});

async function getPendingSyncEvents() {
	const db = await openAuthDb();
	return new Promise((resolve, reject) => {
		const tx = db.transaction(syncStoreName, 'readonly');
		const req = tx.objectStore(syncStoreName).getAll();
		req.onsuccess = () => {
			const all = req.result || [];
			resolve(all.filter(e => e.status === 'pending'));
		};
		req.onerror = () => reject(req.error);
	});
}

async function markEventSent(eventId) {
	const db = await openAuthDb();
	return new Promise((resolve, reject) => {
		const tx = db.transaction(syncStoreName, 'readwrite');
		const store = tx.objectStore(syncStoreName);
		const req = store.get(eventId);
		req.onsuccess = () => {
			const record = req.result;
			if (record) {
				record.status = 'sent';
				store.put(record);
			}
		};
		tx.oncomplete = () => resolve();
		tx.onerror = () => reject(tx.error);
	});
}

async function clearSentEvents() {
	const db = await openAuthDb();
	return new Promise((resolve, reject) => {
		const tx = db.transaction(syncStoreName, 'readwrite');
		const store = tx.objectStore(syncStoreName);
		const req = store.getAll();
		req.onsuccess = () => {
			const all = req.result || [];
			for (const record of all) {
				if (record.status === 'sent') {
					store.delete(record.id);
				}
			}
		};
		tx.oncomplete = () => resolve();
		tx.onerror = () => reject(tx.error);
	});
}

async function flushPendingSyncEvents(token) {
	const pending = await getPendingSyncEvents();
	for (const event of pending) {
		try {
			const response = await fetch('/api/sync/event', {
				method: 'POST',
				headers: {
					'Authorization': `Bearer ${token}`,
					'Content-Type': 'application/json'
				},
				body: JSON.stringify({
					eventId: event.id,
					eventType: event.eventType,
					itemId: event.itemId,
					timestamp: event.timestamp,
					payload: event.payload
				}),
				cache: 'no-store'
			});
			if (response.ok) {
				await markEventSent(event.id);
			}
		} catch {
			// Will retry on next sync
		}
	}
	await clearSentEvents();
}

async function onBackgroundSync() {
	try {
		const token = await getStoredJwt();
		if (!token) {
			console.warn('Service worker (dev): No stored JWT; skipping sync-event flush');
			return;
		}
		await flushPendingSyncEvents(token);
	} catch (error) {
		console.warn('Service worker (dev): sync-event flush threw', error);
	}
}
