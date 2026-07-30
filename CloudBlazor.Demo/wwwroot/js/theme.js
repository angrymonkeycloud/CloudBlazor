// Light/dark theme for the demo shell.
//
// Loaded as a classic script before blazor.web.js so the stored theme is applied
// during the first paint and the page never flashes the wrong palette.

(function () {
    const storageKey = "cloudblazor-demo-theme";

    function readStoredTheme() {
        try {
            return localStorage.getItem(storageKey);
        } catch {
            // Storage is unavailable in private mode on some browsers.
            return null;
        }
    }

    function storeTheme(theme) {
        try {
            localStorage.setItem(storageKey, theme);
        } catch {
            // Persisting the choice is best effort.
        }
    }

    function preferredTheme() {
        return window.matchMedia?.("(prefers-color-scheme: light)").matches ? "light" : "dark";
    }

    function apply(theme) {
        document.documentElement.setAttribute("data-theme", theme);
        return theme;
    }

    window.cloudBlazorDemoTheme = {
        get: () => document.documentElement.getAttribute("data-theme") ?? "dark",

        toggle: () => {
            const next = window.cloudBlazorDemoTheme.get() === "light" ? "dark" : "light";

            storeTheme(next);

            return apply(next);
        }
    };

    apply(readStoredTheme() ?? preferredTheme());
})();
