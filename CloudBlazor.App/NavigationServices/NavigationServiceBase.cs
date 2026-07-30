using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AngryMonkey.CloudBlazor.App;

/// <summary>
/// Shared implementation of <see cref="INavigationService"/>: page hierarchy tracking,
/// popup state, and URI helpers. Platform behaviour lives in the derived services.
/// </summary>
public abstract class NavigationServiceBase(NavigationManager navigationManager) : INavigationService
{
    /// <summary>
    /// Name of the page treated as the root of the hierarchy.
    /// </summary>
    public const string HomePage = "Home";

    private string _currentPage = HomePage;

    protected readonly NavigationManager _navigationManager = navigationManager;

    // Platform specific flags
    public abstract bool IsWebPlatform { get; }

    // Page hierarchy management
    public string CurrentPage => _currentPage;
    public bool ShouldShowBackButton => !IsCurrentPage(HomePage);
    public event Action<string>? OnPageChanged;
    public abstract event EventHandler<string>? NavigateRequest;

    public void SetCurrentPage(string page)
    {
        ArgumentNullException.ThrowIfNull(page);

        if (IsCurrentPage(page))
            return;

        _currentPage = page;

        OnPageChanged?.Invoke(page);
    }

    public bool IsCurrentPage(string page) => string.Equals(_currentPage, page, StringComparison.OrdinalIgnoreCase);

    // Popup tracking
    public bool IsPopupOpen { get; set; }

    // Common helpers
    public string CurrentUri => _navigationManager.Uri;
    public string BaseUri => _navigationManager.BaseUri;

    public string PathUri => $"/{_navigationManager.ToBaseRelativePath(_navigationManager.Uri)}";

    public string ToBaseRelativePath(string absoluteUri)
    {
        // NavigationManager throws when the URI does not sit under the base URI.
        // Callers use this for routing, so an unrelated URI maps to the base path.
        try
        {
            return _navigationManager.ToBaseRelativePath(absoluteUri);
        }
        catch (ArgumentException)
        {
            return string.Empty;
        }
    }

    // INavigationService abstract/platform parts
    public abstract Task NavigateBackAsync();
    public abstract Task NavigateToAsync(string route, bool forceReload = false);
    public abstract Task NavigateToExternalAsync(string url, bool newTab = false);
    public abstract bool TryNavigateBack();

    // Deep link helpers - defaults do nothing; platform can override
    public virtual bool TryHandleDeepLink(Uri uri) => false;
    public virtual bool TryHandleDeepLink(string uri) => false;

    public abstract void SoftNavigate(string url);

    /// <summary>
    /// Records the current URI as a history entry so a following back navigation
    /// returns to it instead of leaving the application.
    /// </summary>
    /// <remarks>
    /// WebAssembly and Blazor Hybrid expose synchronous interop and get an immediate
    /// push. Interactive Server has no synchronous interop, so the call is queued;
    /// the result is deliberately not awaited because <see cref="SoftNavigate"/> is
    /// synchronous by contract.
    /// </remarks>
    protected void PushCurrentHistoryState(IJSRuntime jsRuntime)
    {
        ArgumentNullException.ThrowIfNull(jsRuntime);

        if (jsRuntime is IJSInProcessRuntime inProcessRuntime)
        {
            inProcessRuntime.InvokeVoid("history.pushState", null, string.Empty, CurrentUri);
            return;
        }

        _ = jsRuntime.InvokeVoidAsync("history.pushState", null, string.Empty, CurrentUri);
    }
}
