# CloudBlazor.App.Maui

[![Website](https://img.shields.io/badge/Website-angrymonkeycloud.com-0B5FFF?style=flat-square&logo=googlechrome&logoColor=white)](https://angrymonkeycloud.com/cloudblazor)
[![GitHub repository](https://img.shields.io/badge/GitHub-CloudBlazor-181717?style=flat-square&logo=github)](https://github.com/angrymonkeycloud/CloudBlazor)
[![NuGet](https://img.shields.io/nuget/v/AngryMonkey.CloudBlazor.App.Maui?style=flat-square&logo=nuget&label=NuGet)](https://www.nuget.org/packages/AngryMonkey.CloudBlazor.App.Maui)
[![NuGet downloads](https://img.shields.io/nuget/dt/AngryMonkey.CloudBlazor.App.Maui?style=flat-square&logo=nuget&label=Downloads)](https://www.nuget.org/packages/AngryMonkey.CloudBlazor.App.Maui)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)

**.NET MAUI Blazor Hybrid integration for CloudBlazor.App: native navigation, hardware back
button handling, and platform launcher support.**

Formerly published as `AngryMonkey.CloudApp.Maui` and `AngryMonkey.CloudApp.Mobile`. See
[Migrating](#migrating).

## Supported platforms

Android · iOS · macOS (Mac Catalyst) · Windows

## Installation

```bash
dotnet add package AngryMonkey.CloudBlazor.App.Maui
```

This package references
[`AngryMonkey.CloudBlazor.App`](https://www.nuget.org/packages/AngryMonkey.CloudBlazor.App), so
the shared navigation contract comes with it.

---

## Quick start

```csharp
public static MauiApp CreateMauiApp()
{
    MauiAppBuilder builder = MauiApp.CreateBuilder();

    builder
        .UseMauiApp<App>()
        .ConfigureFonts(fonts => fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"));

    builder.Services.AddMauiBlazorWebView();

    // Registers MauiNavigationService as the INavigationService.
    builder.Services.AddCloudAppMaui();

    return builder.Build();
}
```

Application code depends only on `INavigationService`, so the same components run unchanged in a
browser host that calls `AddCloudApp()` instead.

### Hardware back button

`TryNavigateBack` returns `false` on the root page with no popup open, which is exactly the
signal needed to let the platform close the application:

```csharp
protected override bool OnBackButtonPressed() => _navigation.TryNavigateBack();
```

Returning `true` means the service handled the navigation, so the platform should not.

---

## What differs from the web service

`MauiNavigationService` shares all of `NavigationServiceBase` with `WebNavigationService`. Only
external navigation behaves differently.

```csharp
await Navigation.NavigateToExternalAsync("https://angrymonkeycloud.com");
await Navigation.NavigateToExternalAsync("tel:+96112345678");
await Navigation.NavigateToExternalAsync("www.angrymonkeycloud.com");   // promoted to https
```

| URL form | Handling |
|---|---|
| `http:` / `https:` | Platform launcher — opens the system browser. |
| `tel:` `mailto:` `sms:` `geo:` | Platform launcher — opens the dialer, mail client, or maps. |
| Bare host containing a dot | Promoted to `https://` and launched. |
| Anything else | Routed inside the `BlazorWebView`. |

Opening these through the launcher rather than the web view is the point: a `tel:` link inside a
`BlazorWebView` does nothing useful on its own.

When the launcher fails — typically because no handler is installed for the scheme — the
navigation falls back to the web view rather than being dropped.

### Deep links

```csharp
public override bool TryHandleDeepLink(Uri uri)
{
    // Return true once the link has been consumed.
    return false;
}
```

The string overload parses and delegates to the `Uri` overload, so only one needs overriding.

---

## Migrating

| Previous package | Last version | Replacement |
|---|---|---|
| `AngryMonkey.CloudApp.Maui` | 1.2.1 | `AngryMonkey.CloudBlazor.App.Maui` |
| `AngryMonkey.CloudApp.Mobile` | 1.1.0 | `AngryMonkey.CloudBlazor.App.Maui` |

The previous packages are no longer updated. The repositories were merged into
[CloudBlazor](https://github.com/angrymonkeycloud/CloudBlazor), where all four packages ship as
a matched set.

### Required changes

| Before | After |
|---|---|
| `using AngryMonkey.CloudApp;` | `using AngryMonkey.CloudBlazor.App;` |
| Manual `services.AddScoped<INavigationService, MauiNavigationService>()` | `services.AddCloudAppMaui()` |

`MauiNavigationService` keeps its name and members. Two behaviours changed as bug fixes:

- **`SoftNavigate` is no longer `async void`.** An exception inside it used to be unobservable
  and could tear down the process.
- **A wider set of URI schemes reaches the launcher.** `sms:` was previously routed into the web
  view.

---

## Related packages

| Package | Purpose |
|---|---|
| [`AngryMonkey.CloudBlazor`](https://www.nuget.org/packages/AngryMonkey.CloudBlazor) | Shared components and browser behaviors. |
| [`AngryMonkey.CloudBlazor.App`](https://www.nuget.org/packages/AngryMonkey.CloudBlazor.App) | Navigation contract and browser implementation. |
| [`AngryMonkey.CloudBlazor.Web`](https://www.nuget.org/packages/AngryMonkey.CloudBlazor.Web) | Website infrastructure: head metadata, SEO, bundles. |

## License

[MIT](https://github.com/angrymonkeycloud/CloudBlazor/blob/main/LICENSE) © [Angry Monkey Cloud](https://www.angrymonkeycloud.com/)
