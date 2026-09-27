// Keeps Bootstrap's colour mode (data-bs-theme on <html>) in sync with the Fluent UI theme.
// Fluent applies the mode chosen on the Settings page (stored in localStorage) or, when set to Auto,
// the OS light/dark preference. It marks dark mode with body[data-theme="dark"] and dispatches a
// "themeChanged" event on <body> whenever it applies a theme.
(function () {
    const root = document.documentElement;
    const media = window.matchMedia ? window.matchMedia('(prefers-color-scheme: dark)') : null;
    const apply = isDark => root.setAttribute('data-bs-theme', isDark ? 'dark' : 'light');

    const storedMode = () => {
        try {
            return JSON.parse(localStorage.getItem('fluentui-blazor:theme-settings') || '{}').mode;
        } catch {
            return undefined;
        }
    };

    // Set early (before Fluent loads) to avoid a flash of the wrong theme.
    const mode = storedMode();
    apply(mode === 'dark' || (mode !== 'light' && !!(media && media.matches)));

    document.addEventListener('DOMContentLoaded', () => {
        const body = document.body;
        body.addEventListener('themeChanged', e => apply(!!(e.detail && e.detail.isDark)));
        if (body.getAttribute('data-theme') === 'dark') {
            apply(true);
        }
    });
})();
