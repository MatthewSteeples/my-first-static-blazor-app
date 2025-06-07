let deferredPrompt;
let settingsPageRef;

window.setupPWAInstall = (pageRef) => {
    settingsPageRef = pageRef;
    
    console.log('Setting up PWA install functionality');
    
    // Check if app is already installed or running in standalone mode
    if (window.matchMedia('(display-mode: standalone)').matches || window.navigator.standalone === true) {
        console.log('PWA is already installed or running in standalone mode');
        return; // Keep button hidden if already installed
    }

    // Check if beforeinstallprompt has already fired
    if (deferredPrompt) {
        console.log('beforeinstallprompt already available, showing install button');
        if (settingsPageRef) {
            settingsPageRef.invokeMethodAsync('ShowInstallButton');
        }
        return;
    }

    // Listen for the beforeinstallprompt event
    window.addEventListener('beforeinstallprompt', (e) => {
        console.log('beforeinstallprompt event fired');
        // Prevent Chrome 67 and earlier from automatically showing the prompt
        e.preventDefault();
        // Stash the event so it can be triggered later
        deferredPrompt = e;
        
        // Show the install button
        if (settingsPageRef) {
            console.log('Showing install button');
            settingsPageRef.invokeMethodAsync('ShowInstallButton');
        }
    });

    // Listen for the appinstalled event
    window.addEventListener('appinstalled', (evt) => {
        console.log('PWA was installed');
        if (settingsPageRef) {
            settingsPageRef.invokeMethodAsync('HideInstallButton');
        }
    });

    // For debugging - try to show button anyway if certain conditions are met
    setTimeout(() => {
        if (!deferredPrompt && settingsPageRef) {
            console.log('No beforeinstallprompt event received after 2 seconds');
            // Check if we're in a development environment or certain browsers
            if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
                console.log('Development environment detected - showing install button anyway for testing');
                settingsPageRef.invokeMethodAsync('ShowInstallButton');
            }
        }
    }, 2000);

    // For debugging - log if the event listeners are set up
    console.log('PWA install event listeners set up');
};

window.installPWA = async () => {
    console.log('installPWA called');
    if (deferredPrompt) {
        console.log('Showing install prompt');
        try {
            // Show the prompt
            deferredPrompt.prompt();
            // Wait for the user to respond to the prompt
            const { outcome } = await deferredPrompt.userChoice;
            console.log(`User response to the install prompt: ${outcome}`);
            // We no longer need the prompt. Clear it up.
            deferredPrompt = null;
            
            if (settingsPageRef) {
                settingsPageRef.invokeMethodAsync('HideInstallButton');
            }
        } catch (error) {
            console.error('Error showing install prompt:', error);
        }
    } else {
        console.log('No deferred prompt available');
        
        // Provide more helpful messaging
        let message = 'This app cannot be installed on this device or browser. ';
        
        if (window.matchMedia('(display-mode: standalone)').matches || window.navigator.standalone === true) {
            message = 'This app is already installed!';
        } else if (!('serviceWorker' in navigator)) {
            message += 'Service workers are not supported.';
        } else if (!window.isSecureContext) {
            message += 'The app must be served over HTTPS.';
        } else {
            message += 'Your browser may not support PWA installation or the installation criteria are not met.';
        }
        
        alert(message);
    }
};

// For debugging - check PWA installation criteria
window.checkPWAInstallCriteria = () => {
    console.log('Checking PWA install criteria:');
    console.log('- Service worker registered:', 'serviceWorker' in navigator);
    console.log('- Secure context (HTTPS):', window.isSecureContext);
    console.log('- Standalone display mode:', window.matchMedia('(display-mode: standalone)').matches);
    console.log('- Navigator standalone:', window.navigator.standalone);
    console.log('- beforeinstallprompt support:', 'BeforeInstallPromptEvent' in window);
    console.log('- Current URL:', window.location.href);
    
    // Check if manifest is properly linked
    const manifestLink = document.querySelector('link[rel="manifest"]');
    console.log('- Manifest linked:', !!manifestLink);
    if (manifestLink) {
        console.log('- Manifest href:', manifestLink.href);
        
        // Try to fetch and validate manifest
        fetch(manifestLink.href)
            .then(response => response.json())
            .then(manifest => {
                console.log('- Manifest loaded successfully:', manifest);
                console.log('- Manifest name:', manifest.name);
                console.log('- Manifest start_url:', manifest.start_url);
                console.log('- Manifest scope:', manifest.scope);
                console.log('- Manifest display:', manifest.display);
                console.log('- Manifest icons:', manifest.icons?.length || 0, 'icons');
            })
            .catch(error => {
                console.error('- Error loading manifest:', error);
            });
    }
};

// Call this function on load for debugging
window.addEventListener('DOMContentLoaded', () => {
    window.checkPWAInstallCriteria();
});