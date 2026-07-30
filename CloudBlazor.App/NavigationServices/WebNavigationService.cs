using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AngryMonkey.CloudBlazor.App;

/// <summary>
/// Navigation service for browser-hosted applications: Blazor WebAssembly,
/// Blazor Server, and Blazor Web Apps.
/// </summary>
public class WebNavigationService(IJSRuntime jsRuntime, NavigationManager navigationManager) : NavigationServiceBase(navigationManager)
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;

    public override event EventHandler<string>? NavigateRequest;

    public override bool IsWebPlatform => true;

    public override bool TryNavigateBack()
    {
        if (!ShouldShowBackButton && !IsPopupOpen)
            return false;

        _ = NavigateBackAsync();

        return true;
    }

    public override Task NavigateToAsync(string route, bool forceReload = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);

        // Replacing the entry while a popup is open keeps the popup's own history
        // entry from piling up on the stack.
        if (IsPopupOpen)
            _navigationManager.NavigateTo(route, replace: true);
        else
            _navigationManager.NavigateTo(route, forceLoad: forceReload);

        return Task.CompletedTask;
    }

    public override async Task NavigateToExternalAsync(string url, bool newTab = false)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        try
        {
            // The URL is passed as an argument rather than concatenated into a script
            // string, so a URL containing quotes cannot break out and execute.
            if (newTab)
                await _jsRuntime.InvokeVoidAsync("window.open", url, "_blank", "noopener,noreferrer");
            else
                await _jsRuntime.InvokeVoidAsync("window.location.assign", url);
        }
        catch (JSException)
        {
            _navigationManager.NavigateTo(url, forceLoad: true);
        }
    }

    public override async Task NavigateBackAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("history.back");
        }
        catch (JSException)
        {
            _navigationManager.NavigateTo("/", forceLoad: true);
        }
    }

    public override void SoftNavigate(string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        PushCurrentHistoryState(_jsRuntime);

        NavigateRequest?.Invoke(this, url);
    }
}
