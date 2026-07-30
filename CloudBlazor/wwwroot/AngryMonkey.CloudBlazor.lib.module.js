import {
    initializeCloudBlazor,
    disableEnhancedNavigation
} from "./scripts/cloud-blazor.js";

function initialize() {
    disableEnhancedNavigation();
    initializeCloudBlazor();
}

export function beforeStart() {
    initialize();
}

export function afterStarted() {
    initialize();
}

export function beforeWebStart() {
    initialize();
}

export function afterWebStarted() {
    initialize();
}