# CloudBlazor.App

[![Website](https://img.shields.io/badge/Website-angrymonkeycloud.com-0B5FFF?style=flat-square&logo=googlechrome&logoColor=white)](https://angrymonkeycloud.com/cloudblazor)
[![GitHub repository](https://img.shields.io/badge/GitHub-CloudBlazor-181717?style=flat-square&logo=github)](https://github.com/angrymonkeycloud/CloudBlazor)
[![NuGet](https://img.shields.io/nuget/v/AngryMonkey.CloudBlazor.App?style=flat-square&logo=nuget&label=NuGet)](https://www.nuget.org/packages/AngryMonkey.CloudBlazor.App)
[![NuGet downloads](https://img.shields.io/nuget/dt/AngryMonkey.CloudBlazor.App?style=flat-square&logo=nuget&label=Downloads)](https://www.nuget.org/packages/AngryMonkey.CloudBlazor.App)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)

**Application framework for Blazor WebAssembly and Blazor Hybrid clients: one navigation
contract, page hierarchy tracking, and popup-aware back navigation.**

Formerly published as `AngryMonkey.CloudApp`. See [Migrating](#migrating-from-angrymonkeycloudapp).

## Installation

```bash
dotnet add package AngryMonkey.CloudBlazor.App
```

For .NET MAUI Blazor Hybrid hosts, install
[`AngryMonkey.CloudBlazor.App.Maui`](https://www.nuget.org/packages/AngryMonkey.CloudBlazor.App.Maui)
instead — it references this package and supplies the native navigation service.

---

## Quick start

Register the implementation that matches the host. Application code depends only on
`INavigationService`, so it is identical either way.

```csharp
// Browser: WebAssembly, Server, Blazor Web Apps
builder.Services.AddCloudApp();

// .NET MAUI Blazor Hybrid (from AngryMonkey.CloudBlazor.App.Maui)
builder.Services.AddCloudAppMaui();
```

```razor
@inject INavigationService Navigation

<button @onclick="() => Navigation.NavigateToAsync(&quot;/details/1&quot;)">Open details</button>

@if (Navigation.ShouldShowBackButton)
{
    <button @onclick="Navigation.NavigateBackAsync">Back</button>
}
```

---

## Page hierarchy

Back-button visibility is driven by a named page hierarchy rather than browser history, so the
UI can decide what "back" means before touching history.

```csharp
Navigation.SetCurrentPage("Details");

bool showBack = Navigation.ShouldShowBackButton;   // true — Home is the root
```

| Member | Description |
|---|---|
| `CurrentPage` | The current page name. Starts at `NavigationServiceBase.HomePage` (`"Home"`). |
| `SetCurrentPage(string)` | Sets it and raises `OnPageChanged`, only when the value actually changes. |
| `IsCurrentPage(string)` | Case-insensitive comparison against the current page. |
| `ShouldShowBackButton` | `false` on the root page, `true` elsewhere. |
| `OnPageChanged` | Raised with the new page name. |

---

## Navigation

| Member | Description |
|---|---|
| `NavigateToAsync(route, forceReload)` | Routes to a path. Replaces the history entry while a popup is open. |
| `NavigateBackAsync()` | Goes back, falling back to the root when history is unavailable. |
| `TryNavigateBack()` | Attempts back navigation; `false` when none is possible. |
| `NavigateToExternalAsync(url, newTab)` | Leaves the application through the platform's mechanism. |
| `SoftNavigate(url)` | Records a history entry and raises `NavigateRequest`. |
| `CurrentUri` / `BaseUri` / `PathUri` | URI helpers. `PathUri` is rooted and base-relative. |
| `ToBaseRelativePath(uri)` | Absolute URI to router-relative path; empty for anything outside the base. |
| `IsPopupOpen` | Popup state that alters back-navigation behaviour. |
| `IsWebPlatform` | `true` for browser hosts, `false` for MAUI. |
| `TryHandleDeepLink(uri)` | Platform deep-link hook. `false` on web. |

### Popups

With `IsPopupOpen` set, `NavigateToAsync` replaces the current history entry instead of pushing
a new one, so dismissing the popup does not leave a dead entry behind.

`TryNavigateBack` returns `false` on the root page with no popup open — the signal a MAUI host
uses to let the hardware back button exit the application:

```csharp
protected override bool OnBackButtonPressed() => Navigation.TryNavigateBack();
```

### External links

```csharp
await Navigation.NavigateToExternalAsync("https://angrymonkeycloud.com", newTab: true);
```

`WebNavigationService` calls `window.open` or `window.location.assign`, passing the URL as an
interop argument rather than concatenating it into a script string, so a URL containing quotes
cannot break out and execute. `MauiNavigationService` hands it to the platform launcher, so
`tel:`, `mailto:`, `sms:` and `geo:` open their native handler.

Both fall back to a routed navigation when the platform call fails.

---

## Extending

`NavigationServiceBase` implements the shared behaviour — page hierarchy, popup state and URI
helpers — leaving the platform-specific members abstract:

```csharp
public class CustomNavigationService(IJSRuntime js, NavigationManager navigation)
    : NavigationServiceBase(navigation)
{
    public override bool IsWebPlatform => true;
    public override event EventHandler<string>? NavigateRequest;

    public override Task NavigateToAsync(string route, bool forceReload = false) { ... }
    public override Task NavigateBackAsync() { ... }
    public override Task NavigateToExternalAsync(string url, bool newTab = false) { ... }
    public override bool TryNavigateBack() { ... }
    public override void SoftNavigate(string url) { ... }
}
```

`PushCurrentHistoryState(IJSRuntime)` is available to derived types. It uses synchronous interop
where the host provides it (WebAssembly, Blazor Hybrid) and queues the call on Interactive
Server, which has none.

---

## Migrating from AngryMonkey.CloudApp

| Previous package | Last version | Replacement |
|---|---|---|
| `AngryMonkey.CloudApp` | 1.2.1 | `AngryMonkey.CloudBlazor.App` |
| `AngryMonkey.CloudApp.Maui` | 1.2.1 | `AngryMonkey.CloudBlazor.App.Maui` |
| `AngryMonkey.CloudApp.Shared` | 1.1.0 | `AngryMonkey.CloudBlazor.App` |
| `AngryMonkey.CloudApp.Web` | 1.1.0 | `AngryMonkey.CloudBlazor.App` |
| `AngryMonkey.CloudApp.Mobile` | 1.1.0 | `AngryMonkey.CloudBlazor.App.Maui` |

The previous packages are no longer updated. The repositories were merged into
[CloudBlazor](https://github.com/angrymonkeycloud/CloudBlazor), where all four packages ship as
a matched set.

### Required changes

| Before | After |
|---|---|
| `using AngryMonkey.CloudApp;` | `using AngryMonkey.CloudBlazor.App;` |
| Manual `services.AddScoped<INavigationService, WebNavigationService>()` | `services.AddCloudApp()` |
| Manual `services.AddScoped<INavigationService, MauiNavigationService>()` | `services.AddCloudAppMaui()` |

Type names are unchanged: `INavigationService`, `NavigationServiceBase`, `WebNavigationService`
and `MauiNavigationService` keep their names and members.

Behaviour changed in three places, all bug fixes:

- **`SoftNavigate` no longer throws on Interactive Server.** It used to cast unconditionally to
  `IJSInProcessRuntime`, which only exists in WebAssembly and Hybrid hosts.
- **`SoftNavigate` no longer leaks an event listener.** Each call registered a `popstate`
  listener bound to a `HandlePopState` method that did not exist. The dead registration is gone.
- **External navigation no longer uses `eval`.** The URL is passed as an interop argument.

---

## Related packages

| Package | Purpose |
|---|---|
| [`AngryMonkey.CloudBlazor`](https://www.nuget.org/packages/AngryMonkey.CloudBlazor) | Shared components and browser behaviors. |
| [`AngryMonkey.CloudBlazor.Web`](https://www.nuget.org/packages/AngryMonkey.CloudBlazor.Web) | Website infrastructure: head metadata, SEO, bundles. |
| [`AngryMonkey.CloudBlazor.App.Maui`](https://www.nuget.org/packages/AngryMonkey.CloudBlazor.App.Maui) | .NET MAUI Blazor Hybrid integration. |

## License

[MIT](https://github.com/angrymonkeycloud/CloudBlazor/blob/main/LICENSE) © [Angry Monkey Cloud](https://www.angrymonkeycloud.com/)
