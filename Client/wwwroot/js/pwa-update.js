// PWA Update Detection and Management
const observedRegistrations = new WeakSet();

window.pwaUpdate = {
    isUpdateAvailable: false,
    dotNetReference: null,
    isInitialized: false,
    isReloadingForUpdate: false,

    // Initialize update detection
    initialize: function (dotNetRef) {
        this.dotNetReference = dotNetRef;

        if (!('serviceWorker' in navigator) || this.isInitialized) {
            return;
        }

        this.isInitialized = true;

        navigator.serviceWorker.addEventListener('controllerchange', () => {
            if (this.isReloadingForUpdate) {
                this.isReloadingForUpdate = false;
                console.log('Service worker controller changed - reloading updated application');
                window.location.reload();
            }
        });

        navigator.serviceWorker.ready.then((registration) => {
            this.observeRegistration(registration);

            if (registration.waiting) {
                console.log('Service worker waiting - update available');
                this.notifyUpdateAvailable();
            }
        });

        // Periodically check for updates (every 5 minutes)
        setInterval(() => {
            this.checkForUpdates();
        }, 5 * 60 * 1000);
    },

    observeRegistration: function (registration) {
        if (observedRegistrations.has(registration)) {
            return;
        }

        observedRegistrations.add(registration);

        const observeInstallingWorker = (worker) => {
            if (!worker) {
                return;
            }

            worker.addEventListener('statechange', () => {
                if (worker.state === 'installed' && navigator.serviceWorker.controller) {
                    console.log('Service worker installed - update available');
                    this.notifyUpdateAvailable();
                }
            });
        };

        observeInstallingWorker(registration.installing);

        registration.addEventListener('updatefound', () => {
            console.log('Service worker update found');
            observeInstallingWorker(registration.installing);
        });
    },

    // Manually check for updates
    checkForUpdates: function () {
        if ('serviceWorker' in navigator) {
            navigator.serviceWorker.ready.then((registration) => {
                this.observeRegistration(registration);
                registration.update();
            });
        }
    },

    // Notify Blazor component about available update
    notifyUpdateAvailable: function () {
        if (!this.isUpdateAvailable && this.dotNetReference) {
            this.isUpdateAvailable = true;
            this.dotNetReference.invokeMethodAsync('OnUpdateAvailable');
        }
    },

    // Reload the application to activate the update
    reloadApplication: async function () {
        if (!('serviceWorker' in navigator)) {
            window.location.reload();
            return;
        }

        const registration = await navigator.serviceWorker.ready;
        if (!registration.waiting) {
            window.location.reload();
            return;
        }

        this.isReloadingForUpdate = true;
        registration.waiting.postMessage({ type: 'SKIP_WAITING' });
    }
};
