# Genocs.Telemetry

![Genocs Library Banner](https://raw.githubusercontent.com/Genocs/genocs-library/main/assets/genocs-library-banner.png)

OpenTelemetry integration helpers for traces, metrics, and logs. Supports `net10.0`, `net9.0`, and `net8.0`.

## Installation

```bash
dotnet add package Genocs.Telemetry
```

## Getting Started

Use this package to configure OpenTelemetry pipelines and exporters in Genocs services.

Service registration:

```csharp
using Genocs.Telemetry;

genocs.AddTelemetry();
```

Configuration example:

```json
{
  "app": {
    "service": "My Service"
  },
  "telemetry": {
    "exporter": {
      "enabled": true,
      "otlpEndpoint": "http://localhost:4317"
    }
  }
}
```

## Main Entry Points

- `AddTelemetry`

## Support

- Documentation Portal: https://learn.fiscanner.net/
- Documentation: https://github.com/Genocs/genocs-library/tree/main/docs
- Repository: https://github.com/Genocs/genocs-library

## Release Notes

- CHANGELOG: https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md
- Releases: https://github.com/Genocs/genocs-library/releases
