using AngryMonkey.CloudWeb;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

namespace Microsoft.Extensions.DependencyInjection;

public static class MvcServiceCollectionExtensions
{
    public static IServiceCollection AddCloudWeb(this IServiceCollection services, Action<CloudWebConfig> defaultConfig)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(defaultConfig);
        
        services.Configure(defaultConfig);

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
