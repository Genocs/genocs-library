# Genocs.Messaging.Outbox

![Genocs Library Banner](https://raw.githubusercontent.com/Genocs/genocs-library/main/assets/genocs-library-banner.png)

Outbox pattern abstractions for reliable message delivery. Supports `net10.0`, `net9.0`, and `net8.0`.

## Installation

```bash
dotnet add package Genocs.Messaging.Outbox
```

## Getting Started

Use this package to configure outbox processing and choose an in-memory or persistent outbox implementation.

`AddMessageOutbox` requires explicit provider configuration. The package does not auto-register `AddInMemory()` when no configurator is supplied.

Development-only setup:

```csharp
builder.AddMessageOutbox(outbox => outbox.AddInMemory());
```

Production-safe setup should use a durable provider (for example MongoDB outbox):

```csharp
builder.AddMessageOutbox(outbox => outbox.AddMongo());
```

## Main Entry Points

- `AddMessageOutbox`
- `AddInMemory`

## Warning Policy

Messaging warning baseline validation is enforced through [validate-messaging.mk](../../validate-messaging.mk):

- `net10.0` build must pass with `-warnaserror`.
- Messaging unit tests are executed as part of the same validation workflow.

## Support

- Documentation Portal: https://learn.fiscanner.net/
- Documentation: https://github.com/Genocs/genocs-library/tree/main/docs
- Repository: https://github.com/Genocs/genocs-library

## Release Notes

- CHANGELOG: https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md
- Releases: https://github.com/Genocs/genocs-library/releases
