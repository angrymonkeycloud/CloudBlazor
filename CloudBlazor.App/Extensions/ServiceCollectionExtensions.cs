using AngryMonkey.CloudBlazor.App;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registration helpers for CloudBlazor.App.
/// </summary>
public static class CloudAppServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="WebNavigationService"/> as the <see cref="INavigationService"/>
    /// for browser-hosted applications: Blazor WebAssembly, Blazor Server, and Blazor Web Apps.
    /// </summary>
    /// <remarks>
    /// MAUI Blazor Hybrid hosts call <c>AddCloudAppMaui</c> from
    /// <c>AngryMonkey.CloudBlazor.App.Maui</c> instead.
    /// </remarks>
    public static IServiceCollection AddCloudApp(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<INavigationService, WebNavigationService>();

        return services;
    }
}
