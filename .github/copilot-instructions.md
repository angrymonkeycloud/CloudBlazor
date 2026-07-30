# Copilot Instructions

## General AI-Assisted Development

For general AI-assisted development guidance, C# style, static assets, and documentation
standards that apply to this repository, see:

- [AI Instructions](https://github.com/angrymonkeycloud/CloudDocs/blob/main/docs/ai/instructions.md)

**Note**: Project-specific instructions below take precedence when conflicts exist.

## What this repo is

`CloudBlazor` is the foundation of the Angry Monkey Cloud Blazor ecosystem. It publishes four
NuGet packages from one repository, targeting .NET 10:

| Project | Package | Role |
|---|---|---|
| `CloudBlazor/` | `AngryMonkey.CloudBlazor` | Razor Class Library: shared components and browser behaviors. |
| `CloudBlazor.Web/` | `AngryMonkey.CloudBlazor.Web` | Server-side website infrastructure: head metadata, SEO, bundles, crawler detection. |
| `CloudBlazor.App/` | `AngryMonkey.CloudBlazor.App` | Application framework for WebAssembly and Hybrid clients. |
| `CloudBlazor.App.Maui/` | `AngryMonkey.CloudBlazor.App.Maui` | .NET MAUI Blazor Hybrid integration. |

Supporting projects, none of them packable:

| Project | Role |
|---|---|
| `CloudBlazor.Demo/` | One Blazor Web App demonstrating every web-capable package. |
| `CloudBlazor.Tests/` | xUnit + FluentAssertions, organised as `Web/`, `App/`, `Packaging/`. |
| `CloudBlazor.Package/` | CloudMate `CloudPack` entry point that releases all four packages together. |

Dependencies flow one way: `App.Maui` → `App` → `CloudBlazor`, and `Web` → `CloudBlazor`.
Nothing depends on `Web` and `App` at once.

## Build configuration

Shared settings are centralised, so individual project files stay small:

- `Directory.Build.props` — target framework, nullability, shared package metadata. Imported
  **before** each project, so it must not condition on anything the project sets.
- `Directory.Build.targets` — everything conditioned on `IsPackable`: documentation, SourceLink,
  symbol packages, readme and icon packing, plus a target that fails the pack when a packable
  project is missing its readme or icon.

`Version`, `Authors`, `Company`, `AssemblyVersion`, `FileVersion` and `PackageIcon` stay in the
individual project files on purpose: CloudPack rewrites them in place at release time.

## Non-negotiables

### Never suppress a warning to make a build clean

The repository builds with zero warnings and zero `NoWarn` entries. Two bugs shipped because a
warning was suppressed instead of fixed:

- `RZ10012` hid a `SectionPlaceholder` component that does not exist in Blazor. It compiled to an
  inert HTML element, so the managed `<head>` silently rendered nothing.
- `BL0005` hid `CloudBundle` doubling as both a model and a component, which pushed the warning
  into every consuming application.

Fix the cause. If a suppression is genuinely required, it needs a comment stating why.

### The JS initializer file name is a contract

`CloudBlazor/wwwroot/AngryMonkey.CloudBlazor.lib.module.js` must always be named
`{PackageId}.lib.module.js`. The Blazor SDK recognises no other name, and nothing fails at build
time when it drifts — the browser behaviors just stop starting, in every consuming application at
once. Renaming the package means renaming the file.

`CloudBlazor.Tests/Packaging/` asserts this, and asserts that CloudBlazor's static web assets
still reach a consumer that references only `CloudBlazor.Web` or `CloudBlazor.App`.

### Both initialization paths must keep working

`CloudBlazor` starts either through its Blazor JS initializer, or explicitly through
`CloudBlazorScript` / `cloud-blazor.auto.js` for hosts with no Blazor runtime. Initialization is
idempotent so both can run. `CloudBlazor.Web` emits the script by default
(`CloudWebConfig.IncludeCloudBlazorScript`), which is what makes an MVC or static site work.

### Never build a script string from a URL

Interop passes values as arguments. `NavigateToExternalAsync` used to call
`eval($"window.location.href = '{url}'")`; a URL containing a quote could break out and execute.

## Core abstractions

| Type | Lifetime | Purpose |
|---|---|---|
| `CloudPage` | Scoped | Per-request page metadata: title, description, keywords, favicon, robots, bundles, features. |
| `CloudWebConfig` | Options (`IOptionsSnapshot`) | Application-wide defaults, plus the static `IsCrawler` and `IsNonProductionHost` checks. |
| `CloudController` | — | MVC base controller; publishes `CloudPage` through `ViewData`. |
| `INavigationService` | Scoped | Platform-agnostic navigation. `WebNavigationService` or `MauiNavigationService`. |

`CloudPage` is a fluent builder: every setter returns `this` and raises `OnModified`, which is
what `CloudHeadInit` subscribes to in order to re-render the head.

## Blazor rendering pipeline

`CloudHeadPlaceholder` (in `<head>`) marks the section that `CloudHeadContent` (outside the
router) renders into. `CloudHeadContent` → `CloudHeadInit` (merges `PageDefaults` with the
per-page `CloudPage` by null-coalescing) → `CloudHead` (meta tags) + `CloudBundles` (features,
shim, then bundles) + `CloudBlazorScript`.

Prefer `CloudHeadPlaceholder` over spelling out the section name; `CloudWebSections.Head` exposes
the raw value.

## C# conventions (from CloudMate)

- Prefer explicit types; use `var` only when the type is obvious from the right-hand side.
- Use primary constructors, expression-bodied members, collection expressions (`[]`, `[.. other]`),
  and pattern matching.
- Omit braces for single-statement `if`/`for`/`foreach` bodies.
- Name enums in the plural form (`CloudPageFeatures`, `SourceTypes`).
- Never author `.css` directly. Author `.less` under `src/` and let CloudMate compile it into
  `wwwroot`; the generated `.css` is committed. Razor isolated styles use `.razor.less` →
  `.razor.css`.

## Testing

One test project, organised by the area under test. Prefer testing the real contract over
mocking: `CloudBlazor.Tests/Packaging/` reads the actual static web asset manifests from build
output, and `CloudBlazor.Tests/App/` uses a recording `NavigationManager` and `IJSRuntime` so
tests can assert which browser API was reached for and with which arguments.

Add a regression test whenever a fix concerns something that fails silently.

## Releasing

Bump `<Version>` in `CloudBlazor.Package/CloudBlazor.Package.csproj` only — CloudPack propagates
it to all four libraries. Then run `dotnet run --project CloudBlazor.Package`. Do not edit the
version in the individual libraries.
