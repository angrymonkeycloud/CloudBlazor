using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AngryMonkey.CloudBlazor.App;

/// <summary>
/// Navigation service for .NET MAUI Blazor Hybrid hosts. External links are handed to
/// the platform launcher so they open in the system browser, dialer or mail client
/// rather than inside the <c>BlazorWebView</c>.
/// </summary>
public class MauiNavigationService(NavigationManager navigationManager, IJSRuntime jsRuntime) : NavigationServiceBase(navigationManager)
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;

    /// <summary>
    /// Non-HTTP URI schemes handed straight to the platform launcher.
    /// </summary>
    private static readonly string[] _launcherSchemes = ["tel", "mailto", "sms", "geo"];

    public override event EventHandler<string>? NavigateRequest;

    public override bool IsWebPlatform => false;

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

        _navigationManager.NavigateTo(route, forceLoad: forceReload, replace: false);

        return Task.CompletedTask;
    }

    public override async Task NavigateToExternalAsync(string url, bool newTab = false)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        string target = url.Trim();

        try
        {
            if (TryResolveLaunchUri(target, out Uri? launchUri))
            {
                await Launcher.OpenAsync(launchUri);
                return;
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // The launcher fails when no handler is installed for the scheme.
            // Falling back to the web view beats dropping the navigation.
        }

        _navigationManager.NavigateTo(target, forceLoad: true);
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

    public override bool TryHandleDeepLink(string uri) =>
        Uri.TryCreate(uri, UriKind.Absolute, out Uri? parsed) && TryHandleDeepLink(parsed);

    public override void SoftNavigate(string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        PushCurrentHistoryState(_jsRuntime);

        NavigateRequest?.Invoke(this, url);
    }

    /// <summary>
    /// Resolves a launchable absolute URI, promoting a bare host such as
    /// <c>www.example.com</c> to <c>https</c>.
    /// </summary>
    internal static bool TryResolveLaunchUri(string target, out Uri launchUri)
    {
        if (Uri.TryCreate(target, UriKind.Absolute, out Uri? absolute) && IsLaunchable(absolute))
        {
            launchUri = absolute;
            return true;
        }

        bool looksLikeHost = !target.Contains(' ') && target.Contains('.');

        if (looksLikeHost && Uri.TryCreate($"https://{target}", UriKind.Absolute, out Uri? promoted))
        {
            launchUri = promoted;
            return true;
        }

        launchUri = null!;
        return false;
    }

    private static bool IsLaunchable(Uri uri) =>
        uri.Scheme == Uri.UriSchemeHttp
        || uri.Scheme == Uri.UriSchemeHttps
        || _launcherSchemes.Contains(uri.Scheme, StringComparer.OrdinalIgnoreCase);
}
