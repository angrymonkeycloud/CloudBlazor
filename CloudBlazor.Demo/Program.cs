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

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
