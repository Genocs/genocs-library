# Genocs.Core

![Genocs Library Banner](https://raw.githubusercontent.com/Genocs/genocs-library/main/assets/genocs-library-banner.png)

Core building blocks and abstractions for Genocs-based applications. Supports `net10.0`, `net9.0`, and `net8.0`.

## Installation

```bash
dotnet add package Genocs.Core
```

## Getting Started

Use this package to bootstrap core Genocs services and in-memory CQRS dispatching/handler registration for modular applications.

Service registration:

```csharp
using Genocs.Core.Builders;

var genocs = builder.AddGenocs();
genocs.AddDispatchers();
```

Pipeline setup:

```csharp
app.UseGenocs();
```

## Main Entry Points

- `AddGenocs`
- `AddHandlers`
- `AddDispatchers`
- `AddCommandHandlers`
- `AddQueryHandlers`
- `AddEventHandlers`
- `AddInMemoryCommandDispatcher`
- `AddInMemoryQueryDispatcher`
- `AddInMemoryEventDispatcher`
- `MapDefaultEndpoints`
- `UseGenocs`

## Support

- Documentation Portal: https://learn.fiscanner.net/
- Documentation: https://github.com/Genocs/genocs-library/tree/main/docs
- Repository: https://github.com/Genocs/genocs-library

## Release Notes

- CHANGELOG: https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md
- Releases: https://github.com/Genocs/genocs-library/releases
