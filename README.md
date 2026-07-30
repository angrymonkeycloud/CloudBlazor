
# CloudBlazor

[![Website](https://img.shields.io/badge/Website-angrymonkeycloud.com-0B5FFF?style=flat-square&logo=googlechrome&logoColor=white)](https://angrymonkeycloud.com/cloudblazor)
[![GitHub](https://img.shields.io/badge/GitHub-CloudBlazor-181717?style=flat-square&logo=github)](https://github.com/angrymonkeycloud/CloudBlazor)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com)
[![License](https://img.shields.io/badge/License-MIT-2F855A?style=flat-square)](LICENSE)

**CloudBlazor is the foundation of the Angry Monkey Cloud Blazor ecosystem, providing reusable UI, browser behaviors, website infrastructure, Blazor WebAssembly application features, and .NET MAUI Blazor Hybrid integrations.**

---

## Overview

The repository contains four NuGet packages with clear responsibilities:

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
- SEO helpers
- Metadata and Open Graph
- Website routing
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
| Interactive Server | ✅ | ✅ | — | — |
| Interactive Auto | ✅ | ✅ | ✅ | — |
| Blazor WebAssembly | ✅ | — | ✅ | — |
| .NET MAUI Blazor Hybrid | ✅ | — | ✅ | ✅ |

---

## Repository Structure

```text
CloudBlazor/
├── CloudBlazor/
├── CloudWeb/
├── CloudApp/
├── CloudApp.Maui/
├── CloudBlazor.Demo/
└── README.md
```

---

## Design Guidelines

- Generic Razor components → **CloudBlazor**
- Browser behaviors → **CloudBlazor**
- HTML, SEO, layouts and website infrastructure → **CloudBlazor.Web**
- Blazor WebAssembly application features → **CloudBlazor.App**
- Native MAUI functionality → **CloudBlazor.App.Maui**

---

## Building

```bash
git clone https://github.com/angrymonkeycloud/CloudBlazor.git
cd CloudBlazor
dotnet restore
dotnet build
```

## License

MIT License © Angry Monkey Cloud
