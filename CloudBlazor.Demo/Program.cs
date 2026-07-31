using AngryMonkey.CloudBlazor.Web;
using CloudBlazor.Demo.Components;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Interactive server components power the CloudBlazor.App navigation pages; the
// CloudBlazor.Web pages are rendered statically, which is the mode most websites use.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// CloudBlazor.Web owns the <head>: metadata, robots directives and asset bundles.
builder.Services.AddCloudWeb(config =>
{
    config.TitleSuffix = " · CloudBlazor";

    config.PageDefaults
        .SetTitle("CloudBlazor")
        .SetDescription("CloudBlazor is the foundation of the Angry Monkey Cloud Blazor ecosystem: reusable UI, browser behaviors, website infrastructure and application features.")
        .SetKeywords("blazor, razor, components, seo, metadata, bundles, webassembly, maui")
        .SetFavicon("/favicon.png")
        .SetThemeColor("#4f8ef7")
        // Site-wide sharing defaults; a page overrides any of these individually.
        .SetSiteName("CloudBlazor")
        .SetLocale("en_US")
        .SetMaxImagePreview(CloudMaxImagePreviews.Large)
        .SetMaxSnippet(-1)
        .AppendBundle(new CloudBundle
        {
            Source = "css/app.css",
            MinOnRelease = false,
            AppendVersion = true
        });
});

// CloudBlazor.App supplies INavigationService for browser-hosted applications.
builder.Services.AddCloudApp();

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// MapStaticAssets serves the fingerprinted static web assets that CloudBlazor
// publishes, including its JS initializer.
app.MapStaticAssets();

app.UseAntiforgery();

// The two site-level SEO files, generated from code rather than kept in wwwroot.
app.MapCloudSitemap(sitemap => sitemap
    .Add("/", changeFrequency: CloudChangeFrequencies.Weekly, priority: 1.0)
    .Add("/behaviors/initialization")
    .Add("/behaviors/home-link")
    .Add("/web/metadata")
    .Add("/web/discovery")
    .Add("/web/sitemap")
    .Add("/web/bundles")
    .Add("/web/features")
    .Add("/web/robots")
    .Add("/web/crawler")
    .Add("/app/navigation"));

app.MapCloudRobotsTxt(robots => robots
    .Allow("/")
    .AddSitemap("/sitemap.xml"));

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
