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
        } catch (error) {
            console.warn('Service worker: /api/sync threw', error);
        }
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
        console.error('Service worker: Push notification error', error);
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

