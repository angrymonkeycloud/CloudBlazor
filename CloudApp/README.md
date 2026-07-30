# CloudApp

[![Website](https://img.shields.io/badge/Website-angrymonkeycloud.com-0B5FFF?style=flat-square&logo=googlechrome&logoColor=white)](https://angrymonkeycloud.com/cloudapp)
[![GitHub repository](https://img.shields.io/badge/GitHub-CloudApp-181717?style=flat-square&logo=github)](https://github.com/angrymonkeycloud/CloudApp)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)

Two .NET 10 application foundation packages for Angry Monkey Cloud web and .NET MAUI projects. Shared navigation abstractions and `WebNavigationService` live together in the main package; `MauiNavigationService` remains isolated in its own package.

## Packages

| Package | Version | Downloads | Purpose |
| --- | --- | --- | --- |
| `AngryMonkey.CloudApp` | [![NuGet](https://img.shields.io/nuget/v/AngryMonkey.CloudApp?style=flat-square&logo=nuget)](https://www.nuget.org/packages/AngryMonkey.CloudApp) | [![Downloads](https://img.shields.io/nuget/dt/AngryMonkey.CloudApp?style=flat-square&logo=nuget)](https://www.nuget.org/packages/AngryMonkey.CloudApp) | Shared navigation foundations and browser integration |
| `AngryMonkey.CloudApp.Maui` | [![NuGet](https://img.shields.io/nuget/v/AngryMonkey.CloudApp.Maui?style=flat-square&logo=nuget)](https://www.nuget.org/packages/AngryMonkey.CloudApp.Maui) | [![Downloads](https://img.shields.io/nuget/dt/AngryMonkey.CloudApp.Maui?style=flat-square&logo=nuget)](https://www.nuget.org/packages/AngryMonkey.CloudApp.Maui) | .NET MAUI Blazor Hybrid integration |

## Package migration

| Previous package | Replacement |
| --- | --- |
| `AngryMonkey.CloudApp.Shared` | `AngryMonkey.CloudApp` |
| `AngryMonkey.CloudApp.Web` | `AngryMonkey.CloudApp` |
| `AngryMonkey.CloudApp.Mobile` | `AngryMonkey.CloudApp.Maui` |

Both packages use the `AngryMonkey.CloudApp` namespace for a consistent developer experience. The concrete `WebNavigationService` and `MauiNavigationService` names make the selected platform explicit, while the MAUI package references the main package for the shared contracts and base implementation.

## Development

The repository targets .NET 10. Restore and build the solution with the .NET SDK:

```bash
dotnet restore CloudApp.slnx
dotnet build CloudApp.slnx --configuration Release --no-restore
```

## Angry Monkey Cloud

CloudApp is part of the [Angry Monkey Cloud](https://angrymonkeycloud.com) open-source ecosystem. Follow the shared [AI development instructions](https://github.com/angrymonkeycloud/CloudDocs/blob/main/docs/ai/instructions.md) and browse the [GitHub organization](https://github.com/angrymonkeycloud) for related projects.
