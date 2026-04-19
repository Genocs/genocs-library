# Genocs.Messaging

![Genocs Library Banner](https://raw.githubusercontent.com/Genocs/genocs-library/main/assets/genocs-library-banner.png)

Abstractions for message broker publishing and consuming. Supports `net10.0`, `net9.0`, and `net8.0`.

## Installation

```bash
dotnet add package Genocs.Messaging
```

## Getting Started

Use this package to integrate broker-agnostic command and event dispatching abstractions in Genocs services.

Dispatcher methods forward `CancellationToken` to the underlying publisher for both command and event paths.

`AddServiceBusCommandDispatcher` and `AddServiceBusEventDispatcher` register bus-backed dispatcher bridges only. They do not install a concrete transport implementation by themselves.

Result-returning command dispatch (`ICommandDispatcher.SendAsync<TCommand, TResult>`) is intentionally not supported by the service-bus bridge and throws `NotSupportedException` with guidance to use an in-process dispatcher for request-response command flows.

## Main Entry Points

- `AddServiceBusCommandDispatcher`
- `AddServiceBusEventDispatcher`

## Warning Policy

This package follows the messaging quality gate enforced by [validate-messaging.mk](../../validate-messaging.mk):

- `net10.0` build must remain warning-free (`-warnaserror`).
- Unit tests must pass before merging messaging changes.

## Support

- Documentation Portal: https://learn.fiscanner.net/
- Documentation: https://github.com/Genocs/genocs-library/tree/main/docs
- Repository: https://github.com/Genocs/genocs-library

## Release Notes

- CHANGELOG: https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md
- Releases: https://github.com/Genocs/genocs-library/releases
