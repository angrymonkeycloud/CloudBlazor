using AngryMonkey.CloudWeb;
using CloudWeb.Demo.Components;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddCloudWeb(config =>
{
    config.TitleSuffix = " - CloudWeb Demo";

    config.PageDefaults
        .SetFavicon("/favicon.svg")
        .SetDescription("CloudWeb is an ASP.NET Core library that manages page head content and static asset bundles for Blazor and MVC applications.")
        .SetKeywords("cloudweb, blazor, seo, bundles, meta tags");
});

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
