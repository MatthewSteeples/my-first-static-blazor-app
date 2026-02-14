// Caution! Be sure you understand the caveats before publishing an application with
// offline support. See https://aka.ms/blazor-offline-considerations

self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));
// Periodic Background Sync (https://web.dev/periodic-background-sync/)
// Note: This only handles the service worker side. The page must request permission and
// register a periodic sync via registration.periodicSync.register(...).
self.addEventListener('periodicsync', event => event.waitUntil(onPeriodicSync(event)));

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [/\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/];
const offlineAssetsExclude = [/^service-worker\.js$/];

// Replace with your base path if you are hosting on a subfolder. Ensure there is a trailing '/'.
const base = "/";
const baseUrl = new URL(base, self.origin);
const manifestUrlList = self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href);

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

async function onInstall(event) {
    console.info('Service worker: Install');

    // Fetch and cache all matching items from the assets manifest
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));
    await caches.open(cacheName).then(cache => cache.addAll(assetsRequests));
}

async function onActivate(event) {
    console.info('Service worker: Activate');

    // Delete unused caches
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));
}

async function onFetch(event) {
    let cachedResponse = null;
    if (event.request.method === 'GET') {
        // For all navigation requests, try to serve index.html from cache,
        // unless that request is for an offline resource.
        // If you need some URLs to be server-rendered, edit the following check to exclude those URLs
        const shouldServeIndexHtml = event.request.mode === 'navigate'
            && !manifestUrlList.some(url => url === event.request.url)
            && !event.request.url.includes('/.auth/')
            && !event.request.url.includes('/api/');

        const request = shouldServeIndexHtml ? 'index.html' : event.request;
        const cache = await caches.open(cacheName);
        cachedResponse = await cache.match(request);

        if (cachedResponse && cachedResponse.redirected) {
            cachedResponse = new Response(cachedResponse.body,
                {
                    headers: cachedResponse.headers,
                    status: cachedResponse.status,
                    statusText: cachedResponse.statusText
                });
        }
    }

    return cachedResponse || fetch(event.request);
}

async function onPeriodicSync(event) {
    // Not all browsers support this, and the event will only fire when the user
    // has granted permission and the UA decides conditions are appropriate.
    const tag = event?.tag ?? '(unknown)';
    console.info(`Service worker: Periodic Background Sync (${tag})`);

    // Use tags to scope work. Keep work small and resilient.
    // - sync: call the API using a JWT created by the app
    if (event?.tag === 'sync') {
        try {
            const token = await getStoredJwt();
            if (!token) {
                console.warn('Service worker: No stored JWT; skipping /api/sync');
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
                console.warn('Service worker: /api/sync failed', response.status);
            }

            // Also flush any pending sync events as a catch-up
            await flushPendingSyncEvents(token);
        } catch (error) {
            console.warn('Service worker: /api/sync threw', error);
        }
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
            console.warn('Service worker: No stored JWT; skipping sync-event flush');
            return;
        }
        await flushPendingSyncEvents(token);
    } catch (error) {
        console.warn('Service worker: sync-event flush threw', error);
    }
}
