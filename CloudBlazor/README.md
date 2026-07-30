# CloudBlazor

[![Website](https://img.shields.io/badge/Website-angrymonkeycloud.com-0B5FFF?style=flat-square&logo=googlechrome&logoColor=white)](https://angrymonkeycloud.com/cloudblazor)
[![GitHub repository](https://img.shields.io/badge/GitHub-CloudBlazor-181717?style=flat-square&logo=github)](https://github.com/angrymonkeycloud/CloudBlazor)
[![NuGet](https://img.shields.io/nuget/v/AngryMonkey.CloudBlazor?style=flat-square&logo=nuget&label=NuGet)](https://www.nuget.org/packages/AngryMonkey.CloudBlazor)
[![NuGet downloads](https://img.shields.io/nuget/dt/AngryMonkey.CloudBlazor?style=flat-square&logo=nuget&label=Downloads)](https://www.nuget.org/packages/AngryMonkey.CloudBlazor)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)

**Shared Blazor components and browser behaviors for every Blazor hosting model.**

`AngryMonkey.CloudBlazor` is the foundation of the CloudBlazor ecosystem. Everything else in
the repository builds on it.

## Supported hosting models

| Hosting model | Supported |
|---|:---:|
| Static server-side rendering | ✅ |
| Interactive Server | ✅ |
| Interactive WebAssembly | ✅ |
| Interactive Auto | ✅ |
| Standalone Blazor WebAssembly | ✅ |
| .NET MAUI Blazor Hybrid | ✅ |
| MVC / Razor Pages (no Blazor runtime) | ✅ — see [Initialization](#initialization) |

## Installation

```bash
dotnet add package AngryMonkey.CloudBlazor
```

Most applications do not install this package directly. It arrives as a dependency of
[`AngryMonkey.CloudBlazor.Web`](https://www.nuget.org/packages/AngryMonkey.CloudBlazor.Web) or
[`AngryMonkey.CloudBlazor.App`](https://www.nuget.org/packages/AngryMonkey.CloudBlazor.App).

---

## Initialization

CloudBlazor has two independent ways to start, and which one applies depends only on whether
the host loads a Blazor script.

### Blazor hosts — nothing to do

A host that loads `blazor.web.js`, `blazor.server.js` or `blazor.webassembly.js` needs no
configuration. The Blazor runtime discovers the package's JS initializer
(`AngryMonkey.CloudBlazor.lib.module.js`) from its static web assets and invokes it before and
after startup.

This also works through a chain of references. An application that references only
`CloudBlazor.Web` or `CloudBlazor.App` still receives the initializer, because both are Razor
Class Libraries and forward CloudBlazor's static web assets to their own consumers.

> If the behaviors do not start after adding a reference, rebuild rather than build. Static
> web assets are resolved during the build, and an incremental build after adding a project
> reference can serve a stale manifest.

### Hosts without a Blazor script

Plain MVC, Razor Pages, and statically rendered pages with no Blazor runtime have no
initializer pipeline at all, so the path above cannot run. Load the module explicitly:

```razor
<head>
    <CloudBlazorScript />
</head>
```

`AngryMonkey.CloudBlazor.Web` does this for you — because it already owns the head, it emits
the script as part of the managed head content. See
[its readme](https://www.nuget.org/packages/AngryMonkey.CloudBlazor.Web) for the opt-out.

Running both paths is safe. Initialization is guarded, so the second call returns immediately.

---

## Browser behaviors

Behaviors are attribute-driven and attach through a single delegated document handler, which is
what lets one implementation cover static SSR, enhanced navigation, interactive Blazor,
WebAssembly and Blazor Hybrid alike.

### Home link

Marks an anchor as the application's home link:

```razor
<a href="" cloud-home-link>My Application</a>
```

On a left click, in order:

1. Page scrolled past the threshold → prevent navigation and smooth-scroll to the top.
2. Already on the destination page → prevent the redundant navigation.
3. Otherwise → let the anchor navigate normally.

Modified clicks (<kbd>Ctrl</kbd>, <kbd>Shift</kbd>, <kbd>Alt</kbd>, <kbd>Cmd</kbd>), middle
clicks, downloads, external targets and non-HTTP protocols are always left to the browser.

| Attribute | Default | Description |
|---|---|---|
| `cloud-home-link` | — | Marks the anchor as a home link. Required to opt in. |
| `cloud-scroll-threshold` | `8` | Pixels of scroll before a click scrolls to the top instead of navigating. |
| `cloud-scroll-behavior` | `smooth` | `smooth`, `auto`, or `instant`. |
| `cloud-behavior-disabled` | — | Opts a single anchor out. |

### Enhanced navigation opt-out

CloudBlazor sets `data-enhance-nav="false"` on `<body>`, so links perform ordinary browser
navigation instead of Blazor enhanced navigation. The attribute is reapplied after every
enhanced page load, because enhanced navigation patches the live DOM against server markup that
does not carry it.

---

## API

### `CloudBlazorAssets`

Static web asset paths, so applications reference them through a symbol rather than a
hard-coded string.

| Member | Description |
|---|---|
| `PackageId` | `AngryMonkey.CloudBlazor` — also the asset base path segment. |
| `ContentRoot` | `_content/AngryMonkey.CloudBlazor` |
| `InitializerScriptName` | The Blazor JS initializer file name. |
| `ScriptPath` | The ES module exporting the browser behaviors. |
| `AutoInitializerScriptPath` | Module that imports and initializes on document ready. |

> The initializer file name must stay `{PackageId}.lib.module.js`. The SDK recognises no other
> name, and nothing fails at build time when it drifts — the behaviors just never start. A test
> in `CloudBlazor.Tests` asserts the two stay in sync.

### `CloudBlazorScript`

| Parameter | Default | Description |
|---|---|---|
| `Source` | `CloudBlazorAssets.AutoInitializerScriptPath` | Overrides the script URL, for example behind a CDN. |

---

## Related packages

| Package | Purpose |
|---|---|
| [`AngryMonkey.CloudBlazor.Web`](https://www.nuget.org/packages/AngryMonkey.CloudBlazor.Web) | Website infrastructure: head metadata, SEO, bundles. |
| [`AngryMonkey.CloudBlazor.App`](https://www.nuget.org/packages/AngryMonkey.CloudBlazor.App) | Application framework for WebAssembly and Hybrid clients. |
| [`AngryMonkey.CloudBlazor.App.Maui`](https://www.nuget.org/packages/AngryMonkey.CloudBlazor.App.Maui) | .NET MAUI Blazor Hybrid integration. |

## License

[MIT](https://github.com/angrymonkeycloud/CloudBlazor/blob/main/LICENSE) © [Angry Monkey Cloud](https://www.angrymonkeycloud.com/)
