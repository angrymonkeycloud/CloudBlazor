# CloudBlazor

[![Tests](https://img.shields.io/github/actions/workflow/status/angrymonkeycloud/CloudBlazor/tests.yml?branch=main&style=flat-square&logo=githubactions&logoColor=white&label=Tests)](https://github.com/angrymonkeycloud/CloudBlazor/actions/workflows/tests.yml)
[![Website](https://img.shields.io/badge/Website-angrymonkeycloud.com-0B5FFF?style=flat-square&logo=googlechrome&logoColor=white)](https://angrymonkeycloud.com/cloudblazor)
[![GitHub](https://img.shields.io/badge/GitHub-CloudBlazor-181717?style=flat-square&logo=github)](https://github.com/angrymonkeycloud/CloudBlazor)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com)
[![License](https://img.shields.io/badge/License-MIT-2F855A?style=flat-square)](LICENSE)

**CloudBlazor is the foundation of the Angry Monkey Cloud Blazor ecosystem, providing reusable UI, browser behaviors, website infrastructure, Blazor WebAssembly application features, and .NET MAUI Blazor Hybrid integrations.**

---

## Overview

Four NuGet packages, each depending only on the one above it, so an application takes exactly
what it needs:

```text
AngryMonkey.CloudBlazor
├── AngryMonkey.CloudBlazor.Web
└── AngryMonkey.CloudBlazor.App
    └── AngryMonkey.CloudBlazor.App.Maui
```

## Packages

| Package | Latest Version | Downloads | Depends On | Purpose |
|---|:---:|:---:|---|---|
| **[AngryMonkey.CloudBlazor](https://www.nuget.org/packages/AngryMonkey.CloudBlazor)** | ![NuGet](https://img.shields.io/nuget/v/AngryMonkey.CloudBlazor?style=flat-square&logo=nuget) | ![Downloads](https://img.shields.io/nuget/dt/AngryMonkey.CloudBlazor?style=flat-square&logo=nuget) | — | Core Razor Class Library containing reusable components, browser behaviors, JavaScript initializers, and shared Blazor functionality. |
| **[AngryMonkey.CloudBlazor.Web](https://www.nuget.org/packages/AngryMonkey.CloudBlazor.Web)** | ![NuGet](https://img.shields.io/nuget/v/AngryMonkey.CloudBlazor.Web?style=flat-square&logo=nuget) | ![Downloads](https://img.shields.io/nuget/dt/AngryMonkey.CloudBlazor.Web?style=flat-square&logo=nuget) | CloudBlazor | Server-side website package focused on HTML structure, layouts, SEO, metadata, routing, and website infrastructure. |
| **[AngryMonkey.CloudBlazor.App](https://www.nuget.org/packages/AngryMonkey.CloudBlazor.App)** | ![NuGet](https://img.shields.io/nuget/v/AngryMonkey.CloudBlazor.App?style=flat-square&logo=nuget) | ![Downloads](https://img.shields.io/nuget/dt/AngryMonkey.CloudBlazor.App?style=flat-square&logo=nuget) | CloudBlazor | Client-side framework for Blazor WebAssembly applications, including reusable application services and application-level UI. |
| **[AngryMonkey.CloudBlazor.App.Maui](https://www.nuget.org/packages/AngryMonkey.CloudBlazor.App.Maui)** | ![NuGet](https://img.shields.io/nuget/v/AngryMonkey.CloudBlazor.App.Maui?style=flat-square&logo=nuget) | ![Downloads](https://img.shields.io/nuget/dt/AngryMonkey.CloudBlazor.App.Maui?style=flat-square&logo=nuget) | CloudBlazor.App | Native .NET MAUI and Blazor Hybrid integrations built on CloudBlazor.App. |

---

## Installation

### Website (Blazor Web App / SSR)

```bash
dotnet add package AngryMonkey.CloudBlazor.Web
```

### Blazor WebAssembly

```bash
dotnet add package AngryMonkey.CloudBlazor.App
```

### .NET MAUI Blazor Hybrid

```bash
dotnet add package AngryMonkey.CloudBlazor.App.Maui
```

### Component Library

```bash
dotnet add package AngryMonkey.CloudBlazor
```

---

## Migrating from the previous packages

CloudBlazor merges the former **CloudWeb** and **CloudApp** repositories. Their packages are no
longer updated; each has a direct replacement.

| Previous package | Last published | Replacement |
|---|---|---|
| `AngryMonkey.CloudWeb` | 2.3.0 | `AngryMonkey.CloudBlazor.Web` |
| `AngryMonkey.CloudWeb.Server` | 2.4.3 | `AngryMonkey.CloudBlazor.Web` |
| `AngryMonkey.CloudApp` | 1.2.1 | `AngryMonkey.CloudBlazor.App` |
| `AngryMonkey.CloudApp.Maui` | 1.2.1 | `AngryMonkey.CloudBlazor.App.Maui` |
| `AngryMonkey.CloudApp.Shared` | 1.1.0 | `AngryMonkey.CloudBlazor.App` |
| `AngryMonkey.CloudApp.Web` | 1.1.0 | `AngryMonkey.CloudBlazor.App` |
| `AngryMonkey.CloudApp.Mobile` | 1.1.0 | `AngryMonkey.CloudBlazor.App.Maui` |

Namespaces move with the package identity:

| Previous namespace | New namespace |
|---|---|
| `AngryMonkey.CloudWeb` | `AngryMonkey.CloudBlazor.Web` |
| `AngryMonkey.CloudApp` | `AngryMonkey.CloudBlazor.App` |

Type and method names are unchanged. Per-package migration notes, including the behaviour
changes that came with the merge, are in each package readme:
[CloudBlazor.Web](CloudBlazor.Web/README.md#migrating-from-angrymonkeycloudweb) ·
[CloudBlazor.App](CloudBlazor.App/README.md#migrating-from-angrymonkeycloudapp) ·
[CloudBlazor.App.Maui](CloudBlazor.App.Maui/README.md#migrating)

Because all four are new package identities shipping as a matched set, versioning restarts at
`1.0.0`. The version histories of the previous packages do not carry over.

---

## Package Responsibilities

### AngryMonkey.CloudBlazor

- Reusable Razor components
- Browser behaviors
- JavaScript initializers
- Shared utilities
- Foundation for all packages

### AngryMonkey.CloudBlazor.Web

- HTML document generation
- Layout infrastructure
- Page metadata, canonical URLs and `hreflang` alternates
- Open Graph, Twitter cards and JSON-LD structured data
- `sitemap.xml` and `robots.txt` endpoints
- Robots directives and crawler detection
- Asset bundles with cache-busting
- Server-side website features

### AngryMonkey.CloudBlazor.App

- Blazor WebAssembly application shell
- Client-side services
- Authentication helpers
- Navigation services
- Shared application components
- Application framework

### AngryMonkey.CloudBlazor.App.Maui

- Android
- iOS
- macOS / Mac Catalyst
- Windows
- Blazor Hybrid integrations

---

## Supported Platforms

| Platform | CloudBlazor | Web | App | Maui |
|---|:---:|:---:|:---:|:---:|
| Static SSR | ✅ | ✅ | — | — |
| Interactive Server | ✅ | ✅ | ✅ | — |
| Interactive Auto | ✅ | ✅ | ✅ | — |
| Blazor WebAssembly | ✅ | — | ✅ | — |
| .NET MAUI Blazor Hybrid | ✅ | — | ✅ | ✅ |
| MVC / Razor Pages | ✅ | ✅ | — | — |

---

## Repository Structure

```text
CloudBlazor/
├── CloudBlazor/              # AngryMonkey.CloudBlazor
├── CloudBlazor.Web/          # AngryMonkey.CloudBlazor.Web
├── CloudBlazor.App/          # AngryMonkey.CloudBlazor.App
├── CloudBlazor.App.Maui/     # AngryMonkey.CloudBlazor.App.Maui
├── CloudBlazor.Demo/         # Unified demo covering every package
├── CloudBlazor.Tests/        # xUnit test project
├── CloudBlazor.Package/      # CloudMate packaging entry point
├── Directory.Build.props     # Shared build configuration
├── Directory.Build.targets   # Shared packaging configuration
└── README.md
```

---

## Initialization

CloudBlazor's browser behaviors start in one of two ways, and which applies depends only on
whether the host loads a Blazor script.

- **Blazor hosts** — nothing to configure. The runtime discovers the package's JS initializer
  from its static web assets. This works through a chain of references too: an application that
  references only `CloudBlazor.Web` or `CloudBlazor.App` still receives it, because both are
  Razor Class Libraries that forward CloudBlazor's static web assets.
- **Hosts without a Blazor script** (MVC, Razor Pages, static pages with no Blazor runtime) have
  no initializer pipeline at all. Render `<CloudBlazorScript />`, or let `CloudBlazor.Web` emit
  it as part of the managed head — which it does by default.

Both paths are guarded, so running both is safe. `CloudBlazor.Tests` asserts the whole asset
chain, including the `{PackageId}.lib.module.js` naming rule the SDK depends on, because nothing
fails at build time when it drifts.

> Adding a reference and seeing no behaviors usually means a stale incremental build. Static web
> assets are resolved during the build; rebuild rather than build.

---

## Design Guidelines

- Generic Razor components → **CloudBlazor**
- Browser behaviors → **CloudBlazor**
- HTML, SEO, layouts and website infrastructure → **CloudBlazor.Web**
- Blazor WebAssembly application features → **CloudBlazor.App**
- Native MAUI functionality → **CloudBlazor.App.Maui**

Static assets follow the shared Angry Monkey convention: author `.less` under `src/`, and let
CloudMate compile it into `wwwroot`. Never edit a generated `.css` file.

---

## Development

```bash
git clone https://github.com/angrymonkeycloud/CloudBlazor.git
cd CloudBlazor
dotnet restore
dotnet build
```

Run the tests:

```bash
dotnet test CloudBlazor.Tests/CloudBlazor.Tests.csproj
```

Run the demo:

```bash
dotnet run --project CloudBlazor.Demo/CloudBlazor.Demo.csproj
```

The demo is one Blazor Web App covering all three web-capable packages: CloudBlazor's browser
behaviors, CloudBlazor.Web's metadata, canonical and social tags, sitemap and robots.txt,
bundles, robots directives and crawler detection, and CloudBlazor.App's navigation service —
each on its own page with live state. It serves a real `/sitemap.xml` and `/robots.txt`.

### Continuous integration

[`.github/workflows/tests.yml`](.github/workflows/tests.yml) runs on every push and pull request
to `main`:

- **Build and test** — restores, builds the solution in Release, and runs the test suite with
  coverage collection.
- **Validate packages** — packs all four libraries and checks each one carries its readme and
  icon and stays inside a size budget. Both are regressions this repository has actually shipped
  before: every library once failed `dotnet pack` outright, and CloudBlazor.Web once shipped
  23 MB of unrelated content files.

---

## Releasing

`CloudBlazor.Package` is the single packaging entry point. It uses
[`AngryMonkey.CloudMate`](https://www.nuget.org/packages/AngryMonkey.CloudMate)'s `CloudPack` to
version, build, pack and publish all four packages in one run, in dependency order.

1. Bump `<Version>` in `CloudBlazor.Package/CloudBlazor.Package.csproj`. CloudPack propagates it,
   along with the shared metadata, into every library — so the individual project files are not
   edited by hand.
2. Store the NuGet API key in user secrets:

```bash
dotnet user-secrets --project CloudBlazor.Package set "NuGetApiKey" "<your-key>"
```

3. Run it. CloudPack shows the target version and each package's currently published version for
   confirmation before anything is changed:

```bash
dotnet run --project CloudBlazor.Package/CloudBlazor.Package.csproj
```

Packages are also written to `NugetPackages/`.

---

## License

MIT License © Angry Monkey Cloud

## Angry Monkey Cloud

This project follows the shared [AI development instructions](https://github.com/angrymonkeycloud/CloudDocs/blob/main/docs/ai/instructions.md).
