// Read-only probes used by the demo pages to show real browser state.
//
// Deliberately a named function rather than an eval call: the library itself avoids
// building script from strings, and the demo holds to the same rule.

window.cloudBlazorDemoDiagnostics = {
    readInitializationState: () => ({
        initialized: globalThis["__angryMonkeyCloudBlazorInitialized"] === true,
        enhanceNav: document.body.getAttribute("data-enhance-nav") ?? "(not set)"
    }),

    readScrollPosition: () => Math.round(window.scrollY)
};
