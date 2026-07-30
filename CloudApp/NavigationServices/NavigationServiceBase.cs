using Microsoft.AspNetCore.Components;

namespace AngryMonkey.CloudApp;

public abstract class NavigationServiceBase(NavigationManager navigationManager) : INavigationService
{
    private string _currentPage = "Home";
    protected readonly NavigationManager _navigationManager = navigationManager;

    // Platform specific flags
    public abstract bool IsWebPlatform { get; }

    // Page hierarchy management
    public string CurrentPage => _currentPage;
    public bool ShouldShowBackButton => _currentPage != "Home";
    public event Action<string>? OnPageChanged;
    public abstract event EventHandler<string>? NavigateRequest;

    public void SetCurrentPage(string page)
    {
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
        try { return _navigationManager.ToBaseRelativePath(absoluteUri); } catch { return string.Empty; }
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
}
