const initializationKey = "__angryMonkeyCloudBlazorInitialized";

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
 */
export function disableEnhancedNavigation() {
    document.body?.setAttribute("data-enhance-nav", "false");
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