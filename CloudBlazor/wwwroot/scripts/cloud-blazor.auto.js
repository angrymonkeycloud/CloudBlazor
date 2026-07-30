import { initializeCloudBlazor, disableEnhancedNavigation } from "./cloud-blazor.js";

/**
 * Entry point for hosts that have no Blazor JS initializer pipeline, such as MVC,
 * Razor Pages, or a statically rendered site that never loads a Blazor script.
 *
 * Loading this module is equivalent to what the JS initializer does on Blazor
 * hosts. Both paths are guarded, so loading both is harmless.
 */
function initialize() {
    disableEnhancedNavigation();
    initializeCloudBlazor();
}

if (document.readyState === "loading")
    document.addEventListener("DOMContentLoaded", initialize, { once: true });
else
    initialize();
