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
- Canonical URLs and `hreflang` language alternates, derived automatically on multilingual sites
- Open Graph and Twitter cards, falling back to the page's own title and description
- JSON-LD structured data, escaped so it cannot break out of its `<script>` element
- Robots directives (`noindex`, `nofollow`, `max-image-preview`, `max-snippet`, `noarchive`)
  with automatic preview-host protection
- `sitemap.xml` and `robots.txt` endpoints generated from code
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

`Canonical` is the one exception: it is never inherited from `PageDefaults`. A default canonical
would point every page at the same URL, which is the single fastest way to de-index a site.

---

## Canonical URLs and language alternates

```csharp
CloudPage
    .SetCanonical("/about")
    .AddAlternate("en", "/about")
    .AddAlternate("ar", "/ar/about")
    .AddAlternate(CloudAlternateLink.XDefault, "/about");
```

```html
<link rel="canonical" href="https://example.com/about" />
<link rel="alternate" hreflang="en" href="https://example.com/about" />
<link rel="alternate" hreflang="ar" href="https://example.com/ar/about" />
<link rel="alternate" hreflang="x-default" href="https://example.com/about" />
```

A canonical URL tells a search engine which address is authoritative when the same content
answers on more than one URL. Language alternates stop a set of translations being read as
duplicate content; `x-default` marks the fallback for unmatched locales.

Adding the same `hreflang` twice replaces it rather than emitting two contradictory tags.

### Absolute URLs

Relative values are resolved against `CloudWebConfig.BaseUrl`, falling back to the current
request's scheme and host. Configure `BaseUrl` when the public address differs from what the
application sees — behind a proxy or CDN, or when only one of several hosts is canonical.

```csharp
builder.Services.AddCloudWeb(config => config.BaseUrl = "https://example.com");
```

### Multilingual sites

Describe the languages once and the canonical link, the `hreflang` set and `og:locale` are
derived from the request path for every page:

```csharp
builder.Services.AddCloudWeb(config =>
{
    config.BaseUrl = "https://example.com";

    config.Localization = new CloudLocalizationOptions
    {
        DefaultCulture    = "en",
        SupportedCultures = ["en", "ar"],
        Locales           = new Dictionary<string, string> { ["en"] = "en_US", ["ar"] = "ar_AR" },
    };
});
```

The convention is the usual one: the default language at `/path`, every other language at
`/{culture}/path`. On `/ar/about` that produces:

```html
<link rel="canonical" href="https://example.com/ar/about" />
<link rel="alternate" hreflang="en" href="https://example.com/about" />
<link rel="alternate" hreflang="ar" href="https://example.com/ar/about" />
<link rel="alternate" hreflang="x-default" href="https://example.com/about" />
<meta property="og:locale" content="ar_AR" />
<meta property="og:locale:alternate" content="en_US" />
```

A page that sets its own canonical or alternates keeps them; derivation only fills gaps. A page
marked `SetIndexPage(false)` gets neither — an error page stands in for many URLs at once, so
naming one of them as canonical would be wrong.

| Property | Default | Description |
|---|---|---|
| `DefaultCulture` | required | Served from unprefixed URLs, and advertised as `x-default`. |
| `SupportedCultures` | required | Every language, including the default. |
| `Locales` | `{}` | Open Graph locale per culture. Unmapped cultures use the culture name with `-` replaced by `_`. |
| `AutoCanonical` | `true` | Derive the canonical URL. |
| `AutoAlternates` | `true` | Derive the `hreflang` set. |
| `AutoLocale` | `true` | Derive `og:locale` and its alternates. |

The same options build sitemap entries, so one description of the site's languages covers both:

```csharp
app.MapCloudSitemap(sitemap =>
{
    foreach (string path in new[] { "", "about", "contact" })
        sitemap.AddLocalized([.. localization.AlternatesFor(path)], xDefault: "en");
});
```

---

## Open Graph and Twitter cards

What a link looks like when it is pasted into a chat or a social post.

```csharp
CloudPage
    .SetSiteName("My Application")
    .SetOpenGraphType("article")
    .SetLocale("en_US")
    .AddLocaleAlternates("ar_AR")
    .SetImage(new CloudPageImage
    {
        Url    = "/img/og.png",
        Width  = 1200,
        Height = 630,
        Alt    = "Product screenshot",
    })
    .SetTwitterSite("@myapp");
```

| Method | Description |
|---|---|
| `SetSiteName(string)` | Site name shown alongside the title. |
| `SetOpenGraphType(string)` | Object type. Defaults to `website`. |
| `SetSocialTitle(string)` | Overrides the preview title. |
| `SetSocialDescription(string)` | Overrides the preview description. |
| `SetImage(string)` / `SetImage(CloudPageImage)` | Preview image, shared by both formats. |
| `SetLocale(string)` / `AddLocaleAlternates(params string[])` | This page's locale and the others it exists in. |
| `SetTwitterCard(CloudTwitterCards)` | Card layout. |
| `SetTwitterSite(string)` / `SetTwitterCreator(string)` | Site and author handles. |

Three defaults keep most pages to a single line of configuration:

- **Title and description fall back** to the page's own metadata, so only pages that need
  different wording set them.
- **The card layout** is `summary_large_image` once an image is set, and `summary` when not.
- **The image MIME type** is inferred from the file extension.

Tags are emitted only once a page has something to preview, so a page that sets none of this
carries no social markup. Setting `SetSiteName` in `PageDefaults` opts the whole site in.

> The social title uses the raw `SetTitle` value rather than the composed one. A site-wide
> title suffix reads as noise in a shared link, where the site name already appears separately
> as `og:site_name`.

---

## Structured data

JSON-LD describes what a page *is*, which is what makes rich results possible.

```csharp
CloudPage.AddStructuredData(new Dictionary<string, object>
{
    ["@context"] = "https://schema.org",
    ["@type"]    = "Organization",
    ["name"]     = "My Company",
    ["url"]      = "https://example.com",
});

// Or JSON you already have
CloudPage.AddStructuredData(jsonString);
```

Each call adds a document; every one renders as its own `application/ld+json` script. Anonymous
types, dictionaries and POCOs all work, which keeps `@context` and `@type` expressible without a
schema.org type library. Null properties are dropped from serialized objects.

Non-ASCII text is left readable rather than escaped, so Arabic or Chinese content does not
triple in size.

> **Escaping.** `<` is written as `<` in the rendered script. In well-formed JSON that
> character only occurs inside a string literal, where the escape is equivalent — but it means
> a value containing `</script>` cannot close the element and inject markup. Documents supplied
> as raw JSON are escaped the same way.

---

## Sitemap and robots.txt

Both are mapped as endpoints, so they are generated from code instead of being static files in
`wwwroot` that drift out of date.

```csharp
app.MapCloudSitemap(sitemap => sitemap
    .Add("/", changeFrequency: CloudChangeFrequencies.Weekly, priority: 1.0)
    .Add("/about", lastModified: DateTimeOffset.UtcNow)
    .Add("/contact"));

app.MapCloudRobotsTxt();
```

`MapCloudRobotsTxt()` with no arguments allows everything and advertises `/sitemap.xml`. Both
accept a route pattern if you need a different path.

### Localized sitemaps

`AddLocalized` writes one URL per language and cross-links every variant from each of them.
Search engines require that set to be complete and reciprocal:

```csharp
sitemap.AddLocalized(
[
    new CloudAlternateLink("en", "/about"),
    new CloudAlternateLink("ar", "/ar/about"),
], xDefault: "en");
```

### Dynamic entries

The delegate overload runs per request with access to the request scope, for sitemaps built
from a database:

```csharp
app.MapCloudSitemap(async (context, sitemap) =>
{
    ArticleService articles = context.RequestServices.GetRequiredService<ArticleService>();

    foreach (Article article in await articles.GetPublishedAsync())
        sitemap.Add($"/articles/{article.Slug}", article.UpdatedAt);
});
```

### robots.txt rules

```csharp
app.MapCloudRobotsTxt(robots => robots
    .Allow("/")
    .Disallow("/admin")
    .Disallow("/internal", "Googlebot")
    .CrawlDelay(1)
    .AddSitemap("/sitemap.xml"));
```

Rules are grouped per user agent, and a repeated user agent extends its existing group. A
request to a non-production host is served `Disallow: /` instead, matching the protection the
robots meta tag already applies to preview deployments.

> `robots.txt` governs **crawling**; the robots meta tag governs **indexing**. A URL disallowed
> here can still be indexed from an external link, so use `SetIndexPage(false)` to keep a page
> out of an index.

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

### Preview and snippet directives

Beyond indexing, a page can state how much of itself a search result may show:

```csharp
CloudPage
    .SetMaxImagePreview(CloudMaxImagePreviews.Large)
    .SetMaxSnippet(-1)        // -1 lifts the limit, 0 suppresses snippets
    .SetMaxVideoPreview(-1)   // seconds
    .SetNoArchive(true);      // no cached copy
```

```html
<meta name="robots" content="noarchive, max-image-preview:large, max-snippet:-1, max-video-preview:-1">
```

`max-image-preview:large` is what makes a page eligible for a large thumbnail, and for Google
Discover. These usually belong in `PageDefaults` rather than on individual pages.

They are dropped when the page is `noindex`: an excluded page has no preview to size, so
emitting both would be contradictory.

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
| `BaseUrl` | `string?` | Canonical origin used to make canonical URLs, alternates, preview images and sitemap locations absolute. Falls back to the request's own origin. |
| `Localization` | `CloudLocalizationOptions?` | Language-to-URL mapping. When set, canonical, `hreflang` and `og:locale` are derived per request. |
| `StaticFilesBaseDirectory` | `string?` | Base directory stripped and restored when resolving asset paths through `IFileVersionProvider`. |
| `PageDefaults` | `CloudPage` | Default metadata, bundles, features and robots settings. Canonical is never inherited. |
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
