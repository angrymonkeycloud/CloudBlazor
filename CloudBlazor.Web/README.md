# CloudBlazor.Web

[![Website](https://img.shields.io/badge/Website-angrymonkeycloud.com-0B5FFF?style=flat-square&logo=googlechrome&logoColor=white)](https://angrymonkeycloud.com/cloudblazor)
[![GitHub repository](https://img.shields.io/badge/GitHub-CloudBlazor-181717?style=flat-square&logo=github)](https://github.com/angrymonkeycloud/CloudBlazor)
[![NuGet](https://img.shields.io/nuget/v/AngryMonkey.CloudBlazor.Web?style=flat-square&logo=nuget&label=NuGet)](https://www.nuget.org/packages/AngryMonkey.CloudBlazor.Web)
[![NuGet downloads](https://img.shields.io/nuget/dt/AngryMonkey.CloudBlazor.Web?style=flat-square&logo=nuget&label=Downloads)](https://www.nuget.org/packages/AngryMonkey.CloudBlazor.Web)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)

**Server-side website infrastructure for Blazor and MVC: page head metadata, SEO and robots
directives, asset bundles, and crawler detection.**

Formerly published as `AngryMonkey.CloudWeb` and `AngryMonkey.CloudWeb.Server`. See
[Migrating](#migrating-from-angrymonkeycloudweb).

## Features

- Fluent per-page metadata: title, description, keywords, favicon, theme colour, web app manifest
- Application-wide defaults through `CloudWebConfig`, overridden per page
- Title prefix and suffix, title add-ons, and automatic 64-character limiting
- Description truncation at 160 characters
- Robots directives (`noindex`, `nofollow`) with automatic preview-host protection
- CSS and JavaScript bundles with minified path insertion and cache-busting versions
- CDN feature flags and a legacy `exports` shim
- Crawler detection from an extensive user-agent list
- Works in Blazor and MVC, and initializes CloudBlazor in either

## Installation

```bash
dotnet add package AngryMonkey.CloudBlazor.Web
```

---

## Quick start

### 1. Register

```csharp
using AngryMonkey.CloudBlazor.Web;

builder.Services.AddCloudWeb(config =>
{
    config.TitleSuffix = " - My Application";

    config.PageDefaults
        .SetTitle("Home")
        .SetDescription("Default site description.")
        .SetKeywords("cloudblazor, aspnetcore, seo")
        .SetFavicon("/favicon.svg")
        .SetThemeColor("#0B5FFF")
        .SetManifest("/site.webmanifest");
});
```

### 2. Blazor

`App.razor` — mark where the managed head renders:

```razor
<head>
    <CloudHeadPlaceholder />
</head>
```

`Routes.razor` — render the managed head content, outside the router so it survives navigation:

```razor
<CloudHeadContent />

<Router AppAssembly="typeof(Program).Assembly">
    ...
</Router>
```

> Use `<CloudHeadPlaceholder />` rather than writing `<SectionOutlet SectionName="CloudWeb" />`
> by hand. There is no `SectionPlaceholder` component in Blazor — earlier documentation named
> one, and because a mistyped component name compiles to an inert HTML element, the head
> silently rendered nothing. `CloudWebSections.Head` exposes the raw value if you need it.

Per page:

```razor
@inject CloudPage CloudPage

@code {
    protected override void OnInitialized() =>
        CloudPage
            .SetTitle("Dashboard")
            .SetDescription("Operational dashboard")
            .SetKeywords("dashboard, analytics");
}
```

### 3. MVC

Derive from `CloudController` and call `CloudPage()` in each action:

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

`_Layout.cshtml`:

```html
<head>
    <component type="typeof(CloudHeadInit)" render-mode="Static" />
</head>
```

Add `@using AngryMonkey.CloudBlazor.Web` to `_ViewImports.cshtml` to use `@Html.Bundle(...)`
in views.

---

## Page metadata

All setters return `this` and raise `OnModified`, which re-renders the head.

```razor
@inject CloudPage CloudPage

@code {
    protected override void OnInitialized() =>
        CloudPage
            .SetTitle("Contact")
            .SetDescription("Get in touch with us.")
            .SetKeywords("contact, support")
            .SetFavicon("/icons/contact.svg")
            .SetThemeColor("#0B5FFF")
            .SetManifest("/site.webmanifest");
}
```

| Method | Description |
|---|---|
| `SetTitle(string)` | Sets the title. Prefix and suffix are applied automatically. |
| `SetDescription(string)` | Sets the meta description. Truncated past 160 characters. |
| `SetKeywords(string)` | Sets the meta keywords tag. |
| `SetFavicon(string)` | Sets the favicon href and infers its MIME type. |
| `SetThemeColor(string)` | Sets the browser UI theme colour. |
| `SetManifest(string)` | Sets the web app manifest href. |
| `SetTitleAddOns(IEnumerable<string>)` | Appends title tokens within the 64-character limit. |

### Title

```csharp
// Config: TitleSuffix = " - My App"
CloudPage.SetTitle("About");
// Renders: <title>About - My App</title>
```

With no per-page title, `PageDefaults.Title` is used without prefix or suffix.

Title add-ons are appended while the combined title stays within 64 characters; tokens that
would overflow are dropped rather than truncated:

```csharp
CloudPage.SetTitleAddOns(["Page 3", "Category A"]);
```

### Defaults

Per-page values override defaults through null-coalescing — an explicit page value wins,
otherwise the default from `PageDefaults` applies.

---

## Asset bundles

Global bundles from `PageDefaults` render before per-page bundles, so a page can override a
site-wide rule without touching configuration.

```csharp
// By path
CloudPage.AppendBundle("css/theme.css");
CloudPage.AppendBundles("css/a.css", "js/b.js");

// With options
CloudPage.AppendBundle(new CloudBundle
{
    Source        = "js/analytics.js",
    MinOnRelease  = true,
    AppendVersion = true,
    Defer         = true,
    Async         = false,
    UseMapping    = true,
    AddOns        = null,
});

// At a specific position — critical CSS first
CloudPage.InsertBundle(0, new CloudBundle { Source = "css/critical.css" });
```

| Property | Default | Description |
|---|---|---|
| `Source` | required | Relative path, or an absolute `http(s)` URL. |
| `MinOnRelease` | `true` | Inserts `.min.` before the extension outside Development. |
| `AppendVersion` | `true` | Appends a content-based version for cache busting. |
| `UseMapping` | `true` | Resolves through the static asset manifest; falls back to `IFileVersionProvider`. |
| `Defer` | `true` | Adds `defer` to `<script>` tags. |
| `Async` | `false` | Adds `async` to `<script>` tags. |
| `AddOns` | `null` | Attribute string appended verbatim to the tag. |

Only `.css` and `.js` sources render; anything else is ignored rather than emitted as a broken
tag.

> `CloudBundle` is a plain model. It used to double as the component that rendered it, which
> raised `BL0005` in every application that configured a bundle from code. `CloudBundleTag`
> renders it now.

---

## CDN features

```csharp
CloudPage.AddFeature(CloudPageFeatures.JQuery);
```

| Feature | Injected dependency |
|---|---|
| `CloudPageFeatures.JQuery` | jQuery 3.6.4 from `code.jquery.com`, with subresource integrity |

Feature dependencies render before page bundles, since a dependency has to load before the code
that uses it.

### Legacy exports shim

Older CommonJS bundles expect a global `exports` object:

```csharp
CloudPage.SetAddLegacyExportsCreation(true);
```

---

## Robots control

```csharp
CloudPage.SetIndexPage(false);   // noindex
CloudPage.SetFollowPage(false);  // nofollow
```

| `IndexPage` | `FollowPage` | Output |
|---|---|---|
| `true` | `true` | *(no tag emitted)* |
| `false` | `true` | `<meta name="robots" content="noindex">` |
| `true` | `false` | `<meta name="robots" content="nofollow">` |
| `false` | `false` | `<meta name="robots" content="noindex, nofollow">` |

Nothing is emitted when both are allowed: the absence of a robots tag already means index and
follow.

### Preview host protection

A request whose host ends in a suffix from `CloudWebConfig.NonProductionHostSuffixes`
(`azurewebsites.net` by default) is served `noindex, nofollow` regardless of per-page settings,
so a staging deployment cannot be indexed by accident. Blazor and MVC share the same check:

```csharp
bool isPreview = CloudWebConfig.IsNonProductionHost(host);
```

---

## Crawler detection

```razor
@inject CloudPage CloudPage

@if (CloudPage.IsCrawler)
{
    <ServerRenderedSummary />
}
else
{
    <InteractiveExperience />
}
```

MVC controllers use the helper on `CloudController`:

```csharp
public IActionResult Index() => IsCrawler() ? View("IndexSimple") : View();
```

The check is also available directly:

```csharp
bool isCrawler = CloudWebConfig.IsCrawler(userAgent);
```

The user agent is lower-cased and matched against a lower-cased, de-duplicated substring list
covering search engines (`googlebot`, `bingbot`, `baiduspider`), generic patterns (`bot`,
`crawler`, `spider`), command-line tools (`wget`, `curl`) and SEO crawlers (`ahrefsbot`,
`blexbot`).

---

## Configuration reference

| Property | Type | Description |
|---|---|---|
| `TitlePrefix` | `string` | Prepended to a per-page title. |
| `TitleSuffix` | `string` | Appended to a per-page title. |
| `StaticFilesBaseDirectory` | `string?` | Base directory stripped and restored when resolving asset paths through `IFileVersionProvider`. |
| `PageDefaults` | `CloudPage` | Default metadata, bundles, features and robots settings. |
| `IncludeCloudBlazorScript` | `bool` (`true`) | Emits the CloudBlazor browser-behavior module into the managed head. |

### Initializing CloudBlazor

`IncludeCloudBlazorScript` is enabled by default so a CloudWeb site initializes CloudBlazor in
every hosting model, including MVC and static pages that never load a Blazor script and
therefore have no JS initializer pipeline. Initialization is idempotent, so this is safe
alongside the initializer. Turn it off when the host does not need it:

```csharp
builder.Services.AddCloudWeb(config => config.IncludeCloudBlazorScript = false);
```

---

## Migrating from AngryMonkey.CloudWeb

| Previous package | Last version | Replacement |
|---|---|---|
| `AngryMonkey.CloudWeb` | 2.3.0 | `AngryMonkey.CloudBlazor.Web` |
| `AngryMonkey.CloudWeb.Server` | 2.4.3 | `AngryMonkey.CloudBlazor.Web` |

The previous packages are no longer updated. The repositories were merged into
[CloudBlazor](https://github.com/angrymonkeycloud/CloudBlazor), where all four packages ship as
a matched set.

### Required changes

| Before | After |
|---|---|
| `using AngryMonkey.CloudWeb;` | `using AngryMonkey.CloudBlazor.Web;` |
| `@using AngryMonkey.CloudWeb` | `@using AngryMonkey.CloudBlazor.Web` |
| `<SectionPlaceholder SectionName="CloudWeb" />` | `<CloudHeadPlaceholder />` |
| `@Html.Bundle(...)` with no import | Add `@using AngryMonkey.CloudBlazor.Web` to `_ViewImports.cshtml` |
| `CloudPageExtension.Current(viewData)` | `CloudPageExtensions.Current(viewData)` |

Unchanged: `AddCloudWeb`, `CloudPage`, `CloudWebConfig`, `CloudBundle`, `CloudPageFeatures`,
`CloudController`, `CloudHeadContent`, `CloudHeadInit`, and every method on `CloudPage`.

Two behaviours changed as bug fixes rather than API changes:

- **Crawler matching now works for most of the list.** Entries were authored with mixed casing
  (`Baiduspider`, `AhrefsBot`) but compared against a lower-cased user agent, so they could
  never match. Expect more requests to be identified as crawlers than before.
- **`CloudBundle` is no longer a component**, which removes `BL0005` warnings from application
  code. Constructing and configuring it is unchanged; rendering `<CloudBundle ... />` directly
  in markup is not supported.

---

## License

[MIT](https://github.com/angrymonkeycloud/CloudBlazor/blob/main/LICENSE) © [Angry Monkey Cloud](https://www.angrymonkeycloud.com/)
