import {
    initializeCloudBlazor,
    disableEnhancedNavigation
} from "./scripts/cloud-blazor.js";

export function beforeStart() {
    disableEnhancedNavigation();
    initializeCloudBlazor();
}

export function afterStarted() {
    disableEnhancedNavigation();
    initializeCloudBlazor();
}

export function beforeWebStart() {
    disableEnhancedNavigation();
    initializeCloudBlazor();
}

export function afterWebStarted(blazor) {
    disableEnhancedNavigation();
    initializeCloudBlazor();

    blazor?.addEventListener?.("enhancedload", () => {
        disableEnhancedNavigation();
        initializeCloudBlazor();
    });
}