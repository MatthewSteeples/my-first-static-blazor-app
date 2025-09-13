// Caution! Be sure you understand the caveats before publishing an application with
// offline support. See https://aka.ms/blazor-offline-considerations

self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

// Handle messages from the main thread
self.addEventListener('message', event => onMessage(event));

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [/\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/];
const offlineAssetsExclude = [/^service-worker\.js$/];

// Replace with your base path if you are hosting on a subfolder. Ensure there is a trailing '/'.
const base = "/";
const baseUrl = new URL(base, self.origin);
const manifestUrlList = self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href);

// Notification storage
const notificationStore = new Map();

// Handle messages from the main thread
async function onMessage(event) {
    console.log('Service Worker: Received message', event.data);
    const { type, data, itemName } = event.data;
    
    switch (type) {
        case 'SCHEDULE_NOTIFICATION':
            console.log('Service Worker: Scheduling notification for', data.itemName);
            await scheduleNotification(data);
            break;
        case 'CANCEL_NOTIFICATION':
            console.log('Service Worker: Cancelling notification for', itemName);
            await cancelNotification(itemName);
            break;
        default:
            console.log('Service Worker: Unknown message type', type);
    }
}

// Schedule a notification
async function scheduleNotification(notificationData) {
    const { itemName, scheduledTime, intervalHours } = notificationData;
    
    console.log('Service Worker: scheduleNotification called for', itemName, 'at', new Date(scheduledTime));
    
    // Cancel any existing notification for this item
    await cancelNotification(itemName);
    
    // Calculate delay until notification should be shown
    const delay = scheduledTime - Date.now();
    
    console.log('Service Worker: Delay is', delay, 'ms');
    
    if (delay > 0) {
        const timeoutId = setTimeout(() => {
            console.log('Service Worker: Timeout fired, showing notification for', itemName);
            showMedicationNotification(itemName, intervalHours);
        }, delay);
        
        // Store the timeout ID for later cancellation
        notificationStore.set(itemName, timeoutId);
        console.log('Service Worker: Notification scheduled with timeout ID', timeoutId);
    } else {
        console.log('Service Worker: Delay is not positive, not scheduling notification');
    }
}

// Cancel a scheduled notification
async function cancelNotification(itemName) {
    const timeoutId = notificationStore.get(itemName);
    if (timeoutId) {
        clearTimeout(timeoutId);
        notificationStore.delete(itemName);
    }
}

// Show a medication reminder notification
function showMedicationNotification(itemName, intervalHours) {
    const title = `Time for ${itemName}`;
    const body = `It's been ${intervalHours} hour${intervalHours !== 1 ? 's' : ''} since your last ${itemName}. Don't forget to take it!`;
    
    self.registration.showNotification(title, {
        body: body,
        icon: '/icon-192.png',
        badge: '/icon-192.png',
        tag: `medication-${itemName}`,
        requireInteraction: true,
        actions: [
            {
                action: 'track',
                title: 'Mark as Taken'
            },
            {
                action: 'dismiss',
                title: 'Dismiss'
            }
        ]
    });
    
    // Remove from store after showing
    notificationStore.delete(itemName);
}

// Handle notification clicks
self.addEventListener('notificationclick', event => {
    event.notification.close();
    
    if (event.action === 'track') {
        // TODO: We could send a message back to the main app to automatically track the item
        // For now, just open the app
        event.waitUntil(
            clients.openWindow('/')
        );
    } else if (event.action === 'dismiss') {
        // Just close the notification
        return;
    } else {
        // Default click action - open the app
        event.waitUntil(
            clients.openWindow('/')
        );
    }
});

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
