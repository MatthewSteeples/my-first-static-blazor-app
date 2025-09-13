// PWA Update Detection and Management
window.pwaUpdate = {
    isUpdateAvailable: false,
    dotNetReference: null,

    // Initialize update detection
    initialize: function (dotNetRef) {
        this.dotNetReference = dotNetRef;

        if ('serviceWorker' in navigator) {
            // Listen for service worker registration updates
            navigator.serviceWorker.addEventListener('controllerchange', () => {
                console.log('Service worker controller changed - update available');
                this.notifyUpdateAvailable();
            });

            // Check for waiting service worker on page load
            navigator.serviceWorker.ready.then((registration) => {
                if (registration.waiting) {
                    console.log('Service worker waiting - update available');
                    this.notifyUpdateAvailable();
                }

                // Listen for new service worker installations
                registration.addEventListener('updatefound', () => {
                    console.log('Service worker update found');
                    const newWorker = registration.installing;
                    
                    if (newWorker) {
                        newWorker.addEventListener('statechange', () => {
                            if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
                                console.log('Service worker installed - update available');
                                this.notifyUpdateAvailable();
                            }
                        });
                    }
                });
            });

            // Periodically check for updates (every 5 minutes)
            setInterval(() => {
                this.checkForUpdates();
            }, 5 * 60 * 1000);
        }
    },

    // Manually check for updates
    checkForUpdates: function () {
        if ('serviceWorker' in navigator) {
            navigator.serviceWorker.ready.then((registration) => {
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
    reloadApplication: function () {
        window.location.reload();
    }
};