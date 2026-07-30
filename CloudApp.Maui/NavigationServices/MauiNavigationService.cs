using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AngryMonkey.CloudApp;

public class MauiNavigationService(NavigationManager navigationManager, IJSRuntime jsRuntime) : NavigationServiceBase(navigationManager)
{

    private readonly IJSRuntime _jsRuntime = jsRuntime;

    public override event EventHandler<string>? NavigateRequest;

    public override bool IsWebPlatform => false;


    public override bool TryNavigateBack()
    {
        if (!ShouldShowBackButton && !IsPopupOpen) return false;
        _ = Task.Run(NavigateBackAsync);
        return true;
    }

    public override async Task NavigateToAsync(string route, bool forceReload = false)
    {
        _navigationManager.NavigateTo(route, forceLoad: forceReload, replace: false);
        await Task.CompletedTask;
    }

    public override async Task NavigateToExternalAsync(string url, bool newTab = false)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        try
        {
            string trimmed = url.Trim();
            if (Uri.TryCreate(trimmed, UriKind.Absolute, out Uri? uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || uri.Scheme.Equals("tel", StringComparison.OrdinalIgnoreCase) || uri.Scheme.Equals("mailto", StringComparison.OrdinalIgnoreCase) || uri.Scheme.Equals("geo", StringComparison.OrdinalIgnoreCase)))
            {
                await Launcher.OpenAsync(uri);
                return;
            }
            if (!trimmed.Contains(' ') && (trimmed.StartsWith("www.", StringComparison.OrdinalIgnoreCase) || trimmed.Contains('.')))
            {
                string httpUrl = $"https://{trimmed}";
                if (Uri.TryCreate(httpUrl, UriKind.Absolute, out Uri? httpUri))
                {
                    await Launcher.OpenAsync(httpUri);
                    return;
                }
            }
            _navigationManager.NavigateTo(trimmed, forceLoad: true);
        }
        catch
        {
            try { _navigationManager.NavigateTo(url, forceLoad: true); } catch { }
        }
    }

    public override async Task NavigateBackAsync()
    {
        try
        {
            if (IsPopupOpen)
            {
                await _jsRuntime.InvokeVoidAsync("history.back");
                return;
            }
            await _jsRuntime.InvokeVoidAsync("history.back");
        }
        catch
        {
            _navigationManager.NavigateTo("/", forceLoad: true);
        }
    }

    public override bool TryHandleDeepLink(string uri)
    {
        if (Uri.TryCreate(uri, UriKind.Absolute, out var u))
            return TryHandleDeepLink(u);

        return false;
    }

    public override async void SoftNavigate(string url)
    {
        await _jsRuntime.InvokeVoidAsync("history.pushState", null, "", CurrentUri);

        NavigateRequest?.Invoke(null, url);
    }
}
