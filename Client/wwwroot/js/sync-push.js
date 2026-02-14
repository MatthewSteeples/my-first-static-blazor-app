// Immediate push of sync events with Background Sync API fallback.
// Depends on sync-queue.js and periodic-sync.js (for JWT).

(function () {
	const SYNC_TAG = 'sync-event';

	async function getJwt() {
		if (window.periodicSyncSetup?.getJwt) {
			return await window.periodicSyncSetup.getJwt();
		}
		return null;
	}

	async function pushSingleEvent(event, token) {
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
		return response.ok;
	}

	async function registerBackgroundSync() {
		try {
			if (!('serviceWorker' in navigator)) return;
			const registration = await navigator.serviceWorker.ready;
			if (registration?.sync) {
				await registration.sync.register(SYNC_TAG);
			}
		} catch {
			// Background Sync not supported or failed to register
		}
	}

	// Enqueue event in IndexedDB and attempt immediate push.
	// Falls back to Background Sync API on network failure.
	async function enqueueAndPushSyncEvent(eventJson) {
		try {
			const event = typeof eventJson === 'string' ? JSON.parse(eventJson) : eventJson;

			// Always enqueue first for durability
			await window.syncQueue.enqueueSyncEvent(event);

			// Try immediate push
			const token = await getJwt();
			if (!token) {
				await registerBackgroundSync();
				return;
			}

			const pendingEvents = await window.syncQueue.getPendingSyncEvents();
			let allSent = true;

			for (const pending of pendingEvents) {
				try {
					const ok = await pushSingleEvent(pending, token);
					if (ok) {
						await window.syncQueue.markEventSent(pending.id);
					} else {
						allSent = false;
					}
				} catch {
					allSent = false;
				}
			}

			// Clean up sent events
			await window.syncQueue.clearSentEvents();

			// If any failed, register Background Sync for retry
			if (!allSent) {
				await registerBackgroundSync();
			}
		} catch {
			// Last resort: register Background Sync
			await registerBackgroundSync();
		}
	}

	window.syncPush = {
		enqueueAndPushSyncEvent
	};
})();
