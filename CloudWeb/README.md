# CloudWeb

[![Website](https://img.shields.io/badge/Website-angrymonkeycloud.com-0B5FFF?style=flat-square&logo=googlechrome&logoColor=white)](https://angrymonkeycloud.com/cloudweb)
[![GitHub repository](https://img.shields.io/badge/GitHub-CloudWeb-181717?style=flat-square&logo=github)](https://github.com/angrymonkeycloud/CloudWeb)
[![NuGet](https://img.shields.io/nuget/v/AngryMonkey.CloudWeb?style=flat-square&logo=nuget&label=NuGet)](https://www.nuget.org/packages/AngryMonkey.CloudWeb)
[![NuGet downloads](https://img.shields.io/nuget/dt/AngryMonkey.CloudWeb?style=flat-square&logo=nuget&label=Downloads)](https://www.nuget.org/packages/AngryMonkey.CloudWeb)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)

CloudWeb is an ASP.NET Core library for managing page metadata and static asset bundles in Blazor and MVC applications.

## Demo

https://angrymonkeycloud.github.io/CloudWeb/

## Features

- Fluent per-page metadata (`title`, `description`, `keywords`, `favicon`, `theme-color`, web app manifest)
- Global defaults through `CloudWebConfig` with per-page overrides
- Title prefix/suffix, add-ons, and automatic 64-character length limiting
- Description auto-truncation at 160 characters
- Robots directives (`noindex`, `nofollow`) and automatic staging protection
- CSS/JS bundle injection with minified path insertion and cache-busting versioning
- Built-in CDN features (`Maps`, `TextEditor`, `JQuery`) and a legacy exports shim
- Crawler detection using an extensive built-in user-agent list
- Native support for both Blazor and MVC

## Installation

```bash
dotnet add package AngryMonkey.CloudWeb
```

## Quick Start

### 1. Register the library

```csharp
builder.Services.AddCloudWeb(config =>
{
    config.TitleSuffix = " - My Application";

    config.PageDefaults
        .SetTitle("Home")
        .SetDescription("Default site description.")
        .SetKeywords("cloudweb, aspnetcore, seo")
        .SetFavicon("/favicon.svg")
        .SetThemeColor("#0B5FFF")
        .SetManifest("/site.webmanifest");
});
```

### 2. Blazor integration

`App.razor` — add a `SectionPlaceholder` inside `<head>`:

```razor
<head>
    <SectionPlaceholder SectionName="CloudWeb" />
</head>
```

`Routes.razor` — render the managed head content:

```razor
<CloudHeadContent />
```

Per-page usage:

```razor
@inject CloudPage CloudPage

@code {
    protected override void OnInitialized()
    {
        CloudPage
            .SetTitle("Dashboard")
            .SetDescription("Operational dashboard")
            .SetKeywords("dashboard, analytics");
    }
}
```

### 3. MVC integration

Inherit from `CloudController` and call `CloudPage()` in each action:

```csharp
public class HomeController(CloudPage cloudPage) : CloudController(cloudPage)
{
    public IActionResult Index()
    {
        CloudPage("Home").SetDescription("Home page");
        return View();
    }
}
```

Layout (`_Layout.cshtml`) — render the head component:

```html
<head>
    <component type="typeof(CloudHeadInit)" render-mode="Static" />
</head>
```

---

## Page Metadata

`CloudPage` exposes a fluent API for setting all `<head>` metadata. All methods return `this` and fire the `OnModified` event, which triggers a Blazor `StateHasChanged()`.

```razor
@inject CloudPage CloudPage

@code {
    protected override void OnInitialized()
    {
        CloudPage
            .SetTitle("Contact")
            .SetDescription("Get in touch with us.")
            .SetKeywords("contact, support")
            .SetFavicon("/icons/contact.svg")
            .SetThemeColor("#0B5FFF")
            .SetManifest("/site.webmanifest");
    }
}
```

### Title

`SetTitle(string)` sets the page title. `CloudWebConfig.TitlePrefix` and `TitleSuffix` are automatically prepended/appended:

```csharp
// Config: TitleSuffix = " - My App"
CloudPage.SetTitle("About");
// Renders: <title>About - My App</title>
```

When no title is set on the page, the global `PageDefaults.Title` is used instead (without prefix/suffix).

#### Title add-ons

Additional tokens can be appended to the title. The combined title is limited to **64 characters** — tokens that would exceed the limit are silently dropped:

```csharp
CloudPage.SetTitleAddOns(["Page 3", "Category A"]);
```

### Description

`SetDescription(string)` sets the `<meta name="description">` tag. Descriptions longer than **160 characters** are automatically truncated with `...`.

### Keywords

`SetKeywords(string)` sets the `<meta name="keywords">` tag.

### Favicon

`SetFavicon(string)` sets the `<link rel="icon">` href. CloudWeb automatically emits the matching MIME type for common icon formats and allows a global default to be overridden per page.

### Browser theme and web app manifest

`SetThemeColor(string)` sets the browser UI theme color, while `SetManifest(string)` adds the optional web app manifest link. Both can be configured globally through `PageDefaults` or overridden per page.

### Global defaults

Per-page values override the global defaults using null-coalescing — if a page explicitly sets a value it wins; otherwise the global default from `PageDefaults` applies.

```csharp
builder.Services.AddCloudWeb(config =>
{
    config.TitlePrefix = "";
    config.TitleSuffix = " - My App";

    config.PageDefaults
        .SetFavicon("/favicon.svg")
        .SetThemeColor("#0B5FFF")
        .SetManifest("/site.webmanifest")
        .SetDescription("Site-wide default description.")
        .SetKeywords("default, keywords");
});
```

### Metadata API reference

| Method | Description |
|---|---|
| `SetTitle(string)` | Sets the page title. Prefix/suffix are applied automatically. |
| `SetDescription(string)` | Sets the meta description. Values over 160 characters are truncated with an ellipsis. |
| `SetKeywords(string)` | Sets the meta keywords tag. |
| `SetFavicon(string)` | Sets the favicon href. Overrides the global default when provided. |
| `SetThemeColor(string)` | Sets the browser UI theme color. |
| `SetManifest(string)` | Sets the web app manifest href. |
| `SetTitleAddOns(IEnumerable<string>)` | Appends additional tokens to the title, respecting the 64-character limit. |

---

## Asset Bundles

CloudWeb manages CSS and JavaScript file injection into the `<head>`. Global bundles (from `PageDefaults`) are rendered before per-page bundles.

### Appending bundles

```csharp
// Simple string overload
CloudPage.AppendBundle("~/css/theme.css");
CloudPage.AppendBundle("~/js/app.js");

// Multiple at once
CloudPage.AppendBundles("~/css/a.css", "~/js/b.js");

// Full options
CloudPage.AppendBundle(new CloudBundle
{
    Source        = "~/js/analytics.js",
    MinOnRelease  = true,   // inserts .min. before the extension in non-Development environments
    AppendVersion = true,   // appends ?v= cache-busting query string
    Defer         = true,   // adds defer attribute to script tags
    Async         = false,
    UseMapping    = true,   // resolve path through the static asset manifest
    AddOns        = null,   // freeform attribute string appended verbatim to the tag
});
```

### Inserting bundles at a specific position

```csharp
CloudPage.InsertBundle(0, new CloudBundle { Source = "~/css/critical.css" });
```

### Bundle options

| Property | Default | Description |
|---|---|---|
| `Source` | required | Relative (`~/`) or absolute (`http`) URL of the asset. |
| `MinOnRelease` | `true` | Inserts `.min.` before the file extension in non-Development environments. |
| `AppendVersion` | `true` | Appends a content-based `?v=` query string for cache busting. |
| `UseMapping` | `true` | Resolves the path through the static asset manifest; falls back to `IFileVersionProvider` when `false`. |
| `Defer` | `true` | Adds the `defer` attribute to `<script>` tags. |
| `Async` | `false` | Adds the `async` attribute to `<script>` tags. |
| `AddOns` | `null` | Freeform attribute string appended verbatim to the rendered tag. |

### Global bundles

Bundles added to `config.PageDefaults` are injected on every page before any per-page bundles:

```csharp
builder.Services.AddCloudWeb(config =>
{
    config.PageDefaults
        .AppendBundle("~/css/site.css")
        .AppendBundle("~/js/site.js");
});
```

### MVC / Razor views

In MVC, use the `@Html.Bundle()` HTML helper extension in views:

```csharp
// Controller
public class HomeController(CloudPage cloudPage) : CloudController(cloudPage)
{
    public IActionResult Index()
    {
        CloudPage("Home Page");
        return View();
    }
}
```

```html
<!-- View or _Layout.cshtml -->
@Html.Bundle("~/css/site.css")
```

---

## CDN Features

Add a `CloudPageFeatures` flag to `CloudPage` and the corresponding CDN dependency is injected automatically into the page `<head>`. Multiple features can be added in a single call.

```csharp
CloudPage.AddFeature(CloudPageFeatures.Maps);
CloudPage.AddFeature(CloudPageFeatures.JQuery);

// Or multiple at once:
CloudPage.AddFeatures(CloudPageFeatures.Maps, CloudPageFeatures.JQuery);
```

### Available features

| Feature | CDN dependency injected |
|---|---|
| `CloudPageFeatures.Maps` | Azure Maps SDK v2 — `atlas.min.css` + `atlas.min.js` |
| `CloudPageFeatures.TextEditor` | TinyMCE 5 — `tinymce.min.js` |
| `CloudPageFeatures.JQuery` | jQuery 3.6.4 — `jquery-3.6.4.min.js` |

### Legacy exports shim

Some older CommonJS modules expect a global `exports` object. Enable the compatibility shim with:

```csharp
CloudPage.SetAddLegacyExportsCreation(true);
```

This injects `<script>var exports = {};</script>` before all bundles on that page.

---

## Robots Control

Control search engine indexing and link-following on a per-page basis. When both are `true` (the default), no robots tag is emitted.

```csharp
// Prevent indexing on this page
CloudPage.SetIndexPage(false);

// Prevent following links on this page
CloudPage.SetFollowPage(false);

// Explicitly allow both (identical to the default)
CloudPage
    .SetIndexPage(true)
    .SetFollowPage(true);
```

### Rendered output

| `IndexPage` | `FollowPage` | Meta tag output |
|---|---|---|
| `true` | `true` | *(no robots tag emitted)* |
| `false` | `true` | `<meta name="robots" content="noindex">` |
| `true` | `false` | `<meta name="robots" content="nofollow">` |
| `false` | `false` | `<meta name="robots" content="noindex, nofollow">` |

### Automatic staging protection

Any request arriving from a host ending in `azurewebsites.net` automatically receives `noindex, nofollow`, regardless of per-page settings. This prevents staging or preview deployments from being indexed by search engines. The same logic applies in the MVC `CloudController` base class, ensuring consistent behaviour across Blazor and MVC applications.

### Global defaults

```csharp
builder.Services.AddCloudWeb(config =>
{
    config.PageDefaults
        .SetIndexPage(true)
        .SetFollowPage(true);
});
```

---

## Crawler Detection

`CloudPage` detects search engine crawlers on construction by matching the request `User-Agent` header against an extensive built-in list (`CloudWebConfig.CrawlersUserAgents`). The `IsCrawler` property is set to `true` if any substring matches.

### Blazor usage

```razor
@inject CloudPage CloudPage

@if (CloudPage.IsCrawler)
{
    // Serve a simplified, text-only response for crawlers
}
else
{
    // Serve the full interactive experience
}
```

### MVC usage

Use the `IsCrawler()` helper method on `CloudController`:

```csharp
public class HomeController(CloudPage cloudPage) : CloudController(cloudPage)
{
    public IActionResult Index()
    {
        if (IsCrawler())
            return View("IndexSimple");

        return View();
    }
}
```

### Detection coverage

The built-in list covers hundreds of user-agent substrings including:

| Category | Examples |
|---|---|
| Search engines | Googlebot, Bingbot, YandexBot, Baiduspider |
| Generic patterns | `bot`, `crawler`, `spider` |
| Download tools | `wget`, `curl` |
| SEO / auditing tools | AhrefsBot, BLEXBot, SemrushBot |

---

## Configuration Reference

All configuration is passed to `AddCloudWeb` via `CloudWebConfig`:

| Property | Type | Description |
|---|---|---|
| `TitlePrefix` | `string` | String prepended to every page title (applied when a per-page title is set). |
| `TitleSuffix` | `string` | String appended to every page title (applied when a per-page title is set). |
| `StaticFilesBaseDirectory` | `string?` | Base directory prefix stripped/prepended when resolving asset paths with `IFileVersionProvider`. |
| `PageDefaults` | `CloudPage` | Default metadata, bundles, features, and robots settings applied to every page. |

---

## Repository Structure

```text
CloudWeb/
├── CloudWeb/          # Razor Class Library (NuGet package)
├── CloudWeb.Demo/     # Blazor demo application
├── CloudWeb.Nuget/    # Console app that packs and publishes the NuGet package via CloudPack
└── CloudWeb.Tests/    # xUnit test project
```

## Publishing a New Version

`CloudWeb.Nuget` uses [`AngryMonkey.CloudMate`](https://www.nuget.org/packages/AngryMonkey.CloudMate) (`CloudPack`) to pack and push the `CloudWeb` library to NuGet.

1. Bump `<Version>` in `CloudWeb/CloudWeb.csproj`.
2. Store your NuGet API key in user secrets for `CloudWeb.Nuget`:

```bash
dotnet user-secrets --project CloudWeb.Nuget set "NuGetApiKey" "<your-key>"
```

3. Run `CloudWeb.Nuget` to pack and publish — the `.nupkg` is also saved to `NugetPackages/`:

```bash
dotnet run --project CloudWeb.Nuget/CloudWeb.Nuget.csproj
```

## Local Development

Run tests:

```bash
dotnet test CloudWeb.Tests/CloudWeb.Tests.csproj
```

Run demo:

```bash
dotnet run --project CloudWeb.Demo/CloudWeb.Demo.csproj
```

## License

[MIT](LICENSE) © [Angry Monkey Cloud](https://www.angrymonkeycloud.com/)

## Angry Monkey Cloud

This project follows the shared [AI development instructions](https://github.com/angrymonkeycloud/CloudDocs/blob/main/docs/ai/instructions.md).
