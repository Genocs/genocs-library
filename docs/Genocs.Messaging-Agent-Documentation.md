# Genocs.Messaging Agent Reference

## Consumer Mode for Agents

- Assume package is installed from NuGet.
- Do not rely on repository source code access.
- Prefer stable public APIs and extension methods documented here.
- If behavior is uncertain, fail safely and request config/package version details.

## Purpose

`Genocs.Messaging` defines transport-agnostic messaging contracts and CQRS bridge helpers that are implemented by concrete transport providers.

## Quick Facts

| Key | Value |
|---|---|
| Package | `Genocs.Messaging` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | Broker abstraction layer |
| Core entry points | `IBusPublisher`, `IBusSubscriber`, `SubscribeCommand<T>`, `SubscribeEvent<T>`, `AddServiceBusCommandDispatcher`, `AddServiceBusEventDispatcher` |

## Install

```bash
dotnet add package Genocs.Messaging
```

## Minimal Integration Recipe (Program.cs)

```csharp
using Genocs.Core.Builders;
using Genocs.Messaging.CQRS;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder gnxBuilder = builder.AddGenocs();

gnxBuilder
    .AddServiceBusCommandDispatcher()
    .AddServiceBusEventDispatcher();

gnxBuilder.Build();

var app = builder.Build();
app.Run();
```

## Configuration

`Genocs.Messaging` does not define its own section.

Configuration is supplied by the concrete transport you install:

| Transport package | Expected settings section |
|---|---|
| `Genocs.Messaging.RabbitMQ` | `rabbitmq` |
| `Genocs.Messaging.AzureServiceBus` | Azure Service Bus queue/topic sections |
| `Genocs.Messaging.Outbox` | `outbox` |

The base package only contributes contracts and CQRS bridge helpers. If messaging behavior needs to change through configuration, change the installed provider package rather than this package.

## Decision Matrix For Agents

| Goal | Preferred API |
|---|---|
| Publish any message through the abstraction | `IBusPublisher.PublishAsync<T>(...)` |
| Subscribe command handlers via scoped resolution | `IBusSubscriber.SubscribeCommand<T>()` |
| Subscribe event handlers via scoped resolution | `IBusSubscriber.SubscribeEvent<T>()` |
| Bridge command dispatching to bus publishing | `AddServiceBusCommandDispatcher()` |
| Bridge event dispatching to bus publishing | `AddServiceBusEventDispatcher()` |

## Behavior Notes / Constraints

- This package does not provide a broker client by itself.
- Subscriber helper methods resolve handlers inside a per-message DI scope.
- CQRS extensions are thin wrappers around the bus publisher/subscriber contracts.

## Public Capability Map

- Publish contract: `IBusPublisher`.
- Subscribe contract: `IBusSubscriber`.
- CQRS publish/subscribe helpers in `Genocs.Messaging.CQRS`.
- CQRS dispatcher bridge registrations for command and event dispatchers.

## Dependencies

- `Genocs.Core`
- Transport package at runtime (for example RabbitMQ provider)

## Troubleshooting

1. Publish calls succeed but nothing reaches infrastructure.
Fix: Install and configure a concrete transport provider package.
2. Handler subscriptions never run.
Fix: Register transport subscriber startup flow and call `SubscribeCommand<T>()` or `SubscribeEvent<T>()` for each message type.
3. Command/event dispatch is not routed to bus.
Fix: Register `AddServiceBusCommandDispatcher()` and `AddServiceBusEventDispatcher()` during startup.

