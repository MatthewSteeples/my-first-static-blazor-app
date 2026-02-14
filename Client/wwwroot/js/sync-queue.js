// IndexedDB sync queue for background event push.
// Shared DB constants must match periodic-sync.js and service workers.

(function () {
	const DB_NAME = 'BlazorTracker';
	const DB_VERSION = 2;
	const AUTH_STORE = 'auth';
	const SYNC_STORE = 'syncQueue';

	function openDb() {
		return new Promise((resolve, reject) => {
			const request = indexedDB.open(DB_NAME, DB_VERSION);
			request.onupgradeneeded = () => {
				const db = request.result;
				if (!db.objectStoreNames.contains(AUTH_STORE)) {
					db.createObjectStore(AUTH_STORE);
				}
				if (!db.objectStoreNames.contains(SYNC_STORE)) {
					db.createObjectStore(SYNC_STORE, { keyPath: 'id' });
				}
			};
			request.onsuccess = () => resolve(request.result);
			request.onerror = () => reject(request.error);
		});
	}

	async function enqueueSyncEvent(event) {
		const db = await openDb();
		return new Promise((resolve, reject) => {
			const tx = db.transaction(SYNC_STORE, 'readwrite');
			tx.oncomplete = () => resolve();
			tx.onerror = () => reject(tx.error);
			tx.objectStore(SYNC_STORE).put({
				id: event.EventId,
				eventType: event.EventType,
				itemId: event.ItemId,
				timestamp: event.Timestamp,
				payload: event.Payload || null,
				status: 'pending'
			});
		});
	}

	async function getPendingSyncEvents() {
		const db = await openDb();
		return new Promise((resolve, reject) => {
			const tx = db.transaction(SYNC_STORE, 'readonly');
			const req = tx.objectStore(SYNC_STORE).getAll();
			req.onsuccess = () => {
				const all = req.result || [];
				resolve(all.filter(e => e.status === 'pending'));
			};
			req.onerror = () => reject(req.error);
		});
	}

	async function markEventSent(eventId) {
		const db = await openDb();
		return new Promise((resolve, reject) => {
			const tx = db.transaction(SYNC_STORE, 'readwrite');
			const store = tx.objectStore(SYNC_STORE);
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
		const db = await openDb();
		return new Promise((resolve, reject) => {
			const tx = db.transaction(SYNC_STORE, 'readwrite');
			const store = tx.objectStore(SYNC_STORE);
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

	window.syncQueue = {
		enqueueSyncEvent,
		getPendingSyncEvents,
		markEventSent,
		clearSentEvents
	};
})();
