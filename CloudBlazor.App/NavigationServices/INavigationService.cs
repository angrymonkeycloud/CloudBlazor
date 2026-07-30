namespace AngryMonkey.CloudBlazor.App;

/// <summary>
/// Platform-agnostic navigation contract shared by web and MAUI hosts.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigates back using platform-appropriate method
    /// </summary>
    Task NavigateBackAsync();

    /// <summary>
    /// Navigates to a specific route using platform-appropriate method
    /// </summary>
    /// <param name="route">The route to navigate to (e.g., "/", "/hotlines", "/vendor/123")</param>
    /// <param name="forceReload">Force a full page reload (web only or BlazorWebView)</param>
    Task NavigateToAsync(string route, bool forceReload = false);

    /// <summary>
    /// Navigates to an external URL using platform-appropriate method
    /// </summary>
    /// <param name="url">The external URL to navigate to</param>
    /// <param name="newTab">Open in new tab/window (web only)</param>
    Task NavigateToExternalAsync(string url, bool newTab = false);

    /// <summary>
    /// Records a history entry for the current URI and raises <see cref="NavigateRequest"/>
    /// so the application can swap views without a router navigation.
    /// </summary>
    void SoftNavigate(string url);

    /// <summary>
    /// Gets the current platform type for navigation decisions
    /// </summary>
    bool IsWebPlatform { get; }

    /// <summary>
    /// Attempts to navigate back with a synchronous result for hardware back button scenarios
    /// Returns true if navigation was initiated, false if no navigation is possible
    /// </summary>
    bool TryNavigateBack();

    /// <summary>
    /// Gets the current page hierarchy
    /// </summary>
    string CurrentPage { get; }

    /// <summary>
    /// Sets the current page hierarchy
    /// </summary>
    void SetCurrentPage(string page);

    bool IsCurrentPage(string page);

    /// <summary>
    /// Determines if the back button should be shown for the current page
    /// </summary>
    bool ShouldShowBackButton { get; }

    /// <summary>
    /// Event fired when the current page changes
    /// </summary>
    event Action<string>? OnPageChanged;

    /// <summary>
    /// Absolute current URI (e.g., https://app/route?query)
    /// </summary>
    string CurrentUri { get; }
    string BaseUri { get; }
    string PathUri { get; }

    /// <summary>
    /// Converts an absolute URI to a base-relative path understood by the router
    /// Returns empty string for base path.
    /// </summary>
    string ToBaseRelativePath(string absoluteUri);

    /// <summary>
    /// Indicates whether any popup is currently open in the UI.
    /// Used to modify back-navigation behavior (e.g. close popup first).
    /// </summary>
    bool IsPopupOpen { get; set; }

    /// <summary>
    /// Handles a platform deep link. Returns <c>true</c> when the link was consumed.
    /// Web hosts return <c>false</c>; MAUI hosts can override.
    /// </summary>
    bool TryHandleDeepLink(Uri uri);

    /// <inheritdoc cref="TryHandleDeepLink(Uri)" />
    bool TryHandleDeepLink(string uri);

    /// <summary>
    /// Raised by <see cref="SoftNavigate"/> with the requested URL.
    /// </summary>
    event EventHandler<string>? NavigateRequest;
}
