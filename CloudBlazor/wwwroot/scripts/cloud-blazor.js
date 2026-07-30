const initializationKey = "__angryMonkeyCloudBlazorInitialized";
const enhancedNavigationKey = "__angryMonkeyCloudBlazorEnhancedNavigationDisabled";

const enhancedNavigationAttribute = "data-enhance-nav";

const selectors = Object.freeze({ homeLink: "[cloud-home-link]" });

const defaults = Object.freeze({ scrollThreshold: 8, scrollBehavior: "smooth" });

/**
 * Initializes all CloudBlazor browser behaviors.
 *
 * The handler is attached once to the document and uses event delegation,
 * allowing it to work with static SSR, enhanced navigation, interactive
 * Blazor, WebAssembly, and Blazor Hybrid.
 */
export function initializeCloudBlazor() {

    if (globalThis[initializationKey])
        return;

    globalThis[initializationKey] = true;

    document.addEventListener("click", handleDocumentClick);
}

/**
 * Disables Blazor enhanced navigation for the entire application.
 *
 * Applying the attribute to the body makes all descendant links use normal
 * browser navigation unless a descendant explicitly overrides the setting.
 *
 * Two cases are handled explicitly:
 *
 * 1. The body may not exist yet. A JS initializer's `beforeStart` runs before the
 *    document has finished parsing when the Blazor script sits in `<head>`, so the
 *    attribute is applied again once the DOM is ready.
 * 2. Enhanced navigation patches the live DOM against freshly rendered server
 *    markup, which does not carry the attribute, so it is reapplied after every
 *    enhanced page load.
 */
export function disableEnhancedNavigation() {

    applyEnhancedNavigationAttribute();

    if (!document.body)
        document.addEventListener("DOMContentLoaded", applyEnhancedNavigationAttribute, { once: true });

    // The Blazor global is not guaranteed to exist during `beforeStart`, so the
    // subscription is retried on every call until it succeeds. Initializers run
    // both before and after Blazor starts, which is enough to catch it.
    if (globalThis[enhancedNavigationKey])
        return;

    if (typeof globalThis.Blazor?.addEventListener !== "function")
        return;

    globalThis[enhancedNavigationKey] = true;

    globalThis.Blazor.addEventListener("enhancedload", applyEnhancedNavigationAttribute);
}

/**
 * Writes the opt-out attribute onto the body when it is not already set.
 */
function applyEnhancedNavigationAttribute() {
    const body = document.body;

    if (body && body.getAttribute(enhancedNavigationAttribute) !== "false")
        body.setAttribute(enhancedNavigationAttribute, "false");
}

/**
 * Handles delegated document clicks.
 *
 * @param {MouseEvent} event
 */
function handleDocumentClick(event) {
    if (event.defaultPrevented || event.button !== 0)
        return;

    if (event.ctrlKey || event.metaKey || event.shiftKey || event.altKey)
        return;

    const clickedElement = getElement(event.target);

    if (!clickedElement)
        return;

    const homeLink = clickedElement.closest(selectors.homeLink);

    if (homeLink instanceof HTMLAnchorElement)
        handleHomeLinkClick(event, homeLink);
}

/**
 * Implements the CloudBlazor home-link behavior:
 *
 * 1. If the page is scrolled beyond the configured threshold, prevent
 *    navigation and smoothly scroll to the top.
 * 2. If the user is already on the destination page, prevent redundant
 *    navigation.
 * 3. Otherwise, allow the anchor to navigate normally.
 *
 * @param {MouseEvent} event
 * @param {HTMLAnchorElement} link
 */
function handleHomeLinkClick(event, link) {
    if (!canHandleAnchor(link))
        return;

    const scrollThreshold = getNumberAttribute(link, "cloud-scroll-threshold", defaults.scrollThreshold);

    if (window.scrollY > scrollThreshold) {
        event.preventDefault();

        window.scrollTo({ top: 0, behavior: getScrollBehavior(link) });

        return;
    }

    const destination = new URL(link.href, window.location.href);

    if (isCurrentLocation(destination))
        event.preventDefault();
}

/**
 * Avoids overriding normal browser behavior for downloads, external targets,
 * unsupported protocols, or links explicitly excluded by the developer.
 *
 * @param {HTMLAnchorElement} link
 * @returns {boolean}
 */
function canHandleAnchor(link) {
    if (link.hasAttribute("cloud-behavior-disabled"))
        return false;

    if (link.hasAttribute("download"))
        return false;

    const target = link.getAttribute("target");

    if (target && target.toLowerCase() !== "_self")
        return false;

    const destination = new URL(link.href, window.location.href);

    return (destination.protocol === "http:" || destination.protocol === "https:");
}

/**
 * Determines whether a URL points at the current browser location.
 *
 * Hash fragments are intentionally ignored. A link to the same path and query
 * is considered the current location.
 *
 * @param {URL} destination
 * @returns {boolean}
 */
function isCurrentLocation(destination) {
    return (
        destination.origin === window.location.origin &&
        normalizePath(destination.pathname) ===
        normalizePath(window.location.pathname) &&
        destination.search === window.location.search
    );
}

/**
 * Normalizes paths so "/" and trailing-slash variations compare consistently.
 *
 * @param {string} path
 * @returns {string}
 */
function normalizePath(path) {
    if (!path || path === "/")
        return "/";


    return path.endsWith("/") ? path.slice(0, -1) : path;
}

/**
 * Reads the optional scroll behavior.
 *
 * Supported values:
 * - smooth
 * - auto
 * - instant
 *
 * @param {HTMLElement} element
 * @returns {"smooth" | "auto" | "instant"}
 */
function getScrollBehavior(element) {
    const value = element.getAttribute("cloud-scroll-behavior")?.trim().toLowerCase();

    switch (value) {
        case "auto":
        case "instant":
        case "smooth":
            return value;

        default:
            return defaults.scrollBehavior;
    }
}

/**
 * Reads a numeric HTML attribute and returns a fallback for invalid values.
 *
 * @param {HTMLElement} element
 * @param {string} attributeName
 * @param {number} fallback
 * @returns {number}
 */
function getNumberAttribute(element, attributeName, fallback) {
    const rawValue = element.getAttribute(attributeName);

    if (rawValue === null || rawValue.trim() === "")
        return fallback;

    const parsedValue = Number(rawValue);

    return Number.isFinite(parsedValue) ? Math.max(0, parsedValue) : fallback;
}

/**
 * Converts an event target into an Element when possible.
 *
 * @param {EventTarget | null} target
 * @returns {Element | null}
 */
function getElement(target) {
    if (target instanceof Element)
        return target;

    if (target instanceof Node)
        return target.parentElement;

    return null;
}