using AngryMonkey.CloudBlazor.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Endpoint helpers that serve the site-level SEO files.
/// </summary>
public static class CloudWebEndpointRouteBuilderExtensions
{
    private const string SitemapPath = "/sitemap.xml";
    private const string RobotsPath = "/robots.txt";

    /// <summary>
    /// Maps <c>/sitemap.xml</c> from a static set of entries.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="sitemap">Adds the entries to serve.</param>
    /// <param name="pattern">Route pattern. Defaults to <c>/sitemap.xml</c>.</param>
    public static IEndpointConventionBuilder MapCloudSitemap(
        this IEndpointRouteBuilder endpoints,
        Action<CloudSitemap> sitemap,
        string pattern = SitemapPath)
    {
        ArgumentNullException.ThrowIfNull(sitemap);

        return endpoints.MapCloudSitemap((_, builder) =>
        {
            sitemap(builder);

            return Task.CompletedTask;
        }, pattern);
    }

    /// <summary>
    /// Maps <c>/sitemap.xml</c> from entries built per request, for sitemaps that come from a
    /// database or any other service resolved from the request scope.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="sitemap">Populates the sitemap for the current request.</param>
    /// <param name="pattern">Route pattern. Defaults to <c>/sitemap.xml</c>.</param>
    public static IEndpointConventionBuilder MapCloudSitemap(
        this IEndpointRouteBuilder endpoints,
        Func<HttpContext, CloudSitemap, Task> sitemap,
        string pattern = SitemapPath)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(sitemap);

        return endpoints.MapGet(pattern, async (HttpContext context) =>
        {
            CloudSitemap builder = new();

            await sitemap(context, builder);

            string xml = builder.ToXml(ResolveBaseUrl(context));

            return Results.Text(xml, "application/xml", Encoding.UTF8);
        });
    }

    /// <summary>
    /// Maps <c>/robots.txt</c>, advertising <c>/sitemap.xml</c> and allowing everything unless
    /// configured otherwise.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="pattern">Route pattern. Defaults to <c>/robots.txt</c>.</param>
    public static IEndpointConventionBuilder MapCloudRobotsTxt(
        this IEndpointRouteBuilder endpoints,
        string pattern = RobotsPath) =>
        endpoints.MapCloudRobotsTxt(static robots => robots.Allow("/").AddSitemap(SitemapPath), pattern);

    /// <summary>
    /// Maps <c>/robots.txt</c> from an explicit rule set.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="robots">Configures the groups, rules and sitemap URLs.</param>
    /// <param name="pattern">Route pattern. Defaults to <c>/robots.txt</c>.</param>
    /// <remarks>
    /// A request whose host matches <see cref="CloudWebConfig.IsNonProductionHost"/> is served
    /// <c>Disallow: /</c> instead, so a staging deployment is never crawled — the same
    /// protection the robots meta tag already applies to preview hosts.
    /// </remarks>
    public static IEndpointConventionBuilder MapCloudRobotsTxt(
        this IEndpointRouteBuilder endpoints,
        Action<CloudRobotsFile> robots,
        string pattern = RobotsPath)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(robots);

        return endpoints.MapGet(pattern, (HttpContext context) =>
        {
            CloudRobotsFile file = new();

            if (CloudWebConfig.IsNonProductionHost(context.Request.Host.Host))
                file.DisallowAll();
            else
                robots(file);

            return Results.Text(file.ToFileContent(ResolveBaseUrl(context)), "text/plain", Encoding.UTF8);
        });
    }

    /// <summary>
    /// The configured base URL, falling back to the request's own origin so a site works
    /// without configuring one.
    /// </summary>
    private static string ResolveBaseUrl(HttpContext context)
    {
        string? configured = context.RequestServices.GetService<IOptions<CloudWebConfig>>()?.Value.BaseUrl;

        if (!string.IsNullOrWhiteSpace(configured))
            return configured;

        return $"{context.Request.Scheme}://{context.Request.Host}";
    }
}
