using AngryMonkey.CloudBlazor.App;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registration helpers for CloudBlazor.App.Maui.
/// </summary>
public static class CloudAppMauiServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="MauiNavigationService"/> as the <see cref="INavigationService"/>
    /// for .NET MAUI Blazor Hybrid hosts, so external links open through the platform
    /// launcher instead of navigating the <c>BlazorWebView</c>.
    /// </summary>
    public static IServiceCollection AddCloudAppMaui(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<INavigationService, MauiNavigationService>();

        return services;
    }
}
