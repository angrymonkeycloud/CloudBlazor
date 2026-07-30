using AngryMonkey.CloudBlazor.Web;
using System.Text.Json.Serialization;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registration helpers for CloudBlazor.Web.
/// </summary>
public static class CloudWebServiceCollectionExtensions
{
    /// <summary>
    /// Registers CloudBlazor.Web with the built-in defaults.
    /// </summary>
    public static IServiceCollection AddCloudWeb(this IServiceCollection services) =>
        services.AddCloudWeb(static _ => { });

    /// <summary>
    /// Registers CloudBlazor.Web and applies application-wide defaults.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="defaultConfig">Configures page defaults, title affixes and bundles.</param>
    public static IServiceCollection AddCloudWeb(this IServiceCollection services, Action<CloudWebConfig> defaultConfig)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(defaultConfig);

        services.Configure(defaultConfig);

        // MVC brings in IFileVersionProvider, which CloudBundle uses to stamp asset
        // versions, and the controller/view stack for the MVC integration.
        services.AddMvc().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.PropertyNamingPolicy = null;

            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddHttpContextAccessor();

        services.AddScoped<CloudPage>();

        return services;
    }
}
