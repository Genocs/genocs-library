# Genocs.Messaging.RabbitMQ

![Genocs Library Banner](https://raw.githubusercontent.com/Genocs/genocs-library/main/assets/genocs-library-banner.png)

RabbitMQ integration for Genocs messaging abstractions. Supports `net10.0`, `net9.0`, and `net8.0`.

## Installation

```bash
dotnet add package Genocs.Messaging.RabbitMQ
```

## Getting Started

Use this package to connect Genocs message broker abstractions to RabbitMQ transports and conventions.

Typical startup:

```csharp
using Genocs.Core.Builders;
using Genocs.Messaging.RabbitMQ;

IGenocsBuilder genocs = builder.AddGenocs();
await genocs.AddRabbitMQAsync();
genocs.Build();

app.UseRabbitMQ();
```

Runtime behavior highlights:

- Subscriber execution resolves handlers from per-message DI scopes.
- Message settlement is deterministic (`BasicAckAsync`/`BasicNackAsync` are awaited).
- Retry and dead-letter behavior are driven by the `rabbitmq` configuration section (`retries`, `retryInterval`, `deadLetter`, `requeueFailedMessages`).

## Main Entry Points

- `AddRabbitMQAsync`
- `UseRabbitMQ`

## Warning Policy

Messaging quality gate enforcement is defined in [validate-messaging.mk](../../validate-messaging.mk):

- `net10.0` build uses `-warnaserror`.
- RabbitMQ reliability tests are executed via `Genocs.Messaging.RabbitMQ.UnitTests`.

## Support

- Documentation Portal: https://genocs-blog.netlify.app/
- Documentation: https://github.com/Genocs/genocs-library/tree/main/docs
- Repository: https://github.com/Genocs/genocs-library

## Release Notes

- CHANGELOG: https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md
- Releases: https://github.com/Genocs/genocs-library/releases
