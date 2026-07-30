using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AngryMonkey.CloudApp;

public class WebNavigationService(IJSRuntime jsRuntime, NavigationManager navigationManager) : NavigationServiceBase(navigationManager)
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;

    public override event EventHandler<string>? NavigateRequest;

    public override bool IsWebPlatform => true;

    public override bool TryNavigateBack()
    {
        if (!ShouldShowBackButton && !IsPopupOpen)
            return false;

        _ = Task.Run(NavigateBackAsync);

        return true;
    }

    public override async Task NavigateToAsync(string route, bool forceReload = false)
    {
        if (IsPopupOpen)
            _navigationManager.NavigateTo(route, replace: true);
        else
            _navigationManager.NavigateTo(route, forceLoad: forceReload);

        await Task.CompletedTask;
    }

    public override async Task NavigateToExternalAsync(string url, bool newTab = false)
    {
        try
        {
            if (newTab)
                await _jsRuntime.InvokeVoidAsync("window.open", url, "_blank");

            else
                await _jsRuntime.InvokeVoidAsync("eval", $"window.location.href = '{url}'");
        }
        catch
        {
            _navigationManager.NavigateTo(url, forceLoad: true);
        }
    }

    public override async Task NavigateBackAsync()
    {
        try
        {
            // If a popup is open, close it first by navigating back in history (when fragments are used)
            await _jsRuntime.InvokeVoidAsync("history.back");
        }
        catch
        {
            _navigationManager.NavigateTo("/", forceLoad: true);
        }
    }

    // Deep link helpers (no-op on pure web)
    public override bool TryHandleDeepLink(Uri uri) => false;
    public override bool TryHandleDeepLink(string uri) => false;

    public override void SoftNavigate(string url)
    {
        // Push the current state to the history stack
        ((IJSInProcessRuntime)_jsRuntime).InvokeVoid("history.pushState", null, "", CurrentUri);

        // Add an event listener for the popstate event
        ((IJSInProcessRuntime)_jsRuntime).InvokeVoid("window.addEventListener", "popstate", DotNetObjectReference.Create(this), "HandlePopState");
        
        NavigateRequest?.Invoke(null, url);
    }
}
