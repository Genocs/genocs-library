# Genocs.Messaging Agent Reference

## Purpose

This document is optimized for AI-assisted development sessions.
It prioritizes fast retrieval of:

- What Genocs.Messaging is responsible for
- Which APIs to call for specific goals
- Where source of truth lives
- What constraints and runtime behaviors matter

## Quick Facts

| Key | Value |
|---|---|
| Package | Genocs.Messaging |
| Project file | [src/Genocs.Messaging/Genocs.Messaging.csproj](src/Genocs.Messaging/Genocs.Messaging.csproj) |
| Target frameworks | net10.0, net9.0, net8.0 |
| Primary role | Broker-agnostic messaging abstractions for publish/subscribe and CQRS dispatcher bridge wiring |
| Core themes | IBusPublisher and IBusSubscriber contracts, message metadata accessors, AsyncLocal context propagation, CQRS integration helpers, routing metadata attribute |

## Use This Package When

- Defining broker-agnostic publish and subscribe contracts in application code.
- Bridging CQRS ICommandDispatcher and IEventDispatcher to a bus publisher.
- Registering command and event subscriber handlers via scoped DI resolution.
- Passing correlation context and message metadata across async execution flow.
- Annotating message types with exchange, routing key, queue, and external flags.

## Do Not Assume

- This package does not provide a broker implementation; transport packages implement interfaces.
- Subscriber helper methods create a new DI scope per delivered message.
- Correlation and message properties accessors are AsyncLocal-based and flow with async context.

## High-Value Entry Points

### Broker abstraction contracts

- IBusPublisher in [src/Genocs.Messaging/IBusPublisher.cs](src/Genocs.Messaging/IBusPublisher.cs)
- IBusSubscriber in [src/Genocs.Messaging/IBusSubscriber.cs](src/Genocs.Messaging/IBusSubscriber.cs)
- MessageAttribute in [src/Genocs.Messaging/MessageAttribute.cs](src/Genocs.Messaging/MessageAttribute.cs)

### Message metadata contracts

- IMessageProperties in [src/Genocs.Messaging/IMessageProperties.cs](src/Genocs.Messaging/IMessageProperties.cs)
- MessageProperties in [src/Genocs.Messaging/MessageProperties.cs](src/Genocs.Messaging/MessageProperties.cs)
- IMessagePropertiesAccessor in [src/Genocs.Messaging/IMessagePropertiesAccessor.cs](src/Genocs.Messaging/IMessagePropertiesAccessor.cs)
- MessagePropertiesAccessor in [src/Genocs.Messaging/MessagePropertiesAccessor.cs](src/Genocs.Messaging/MessagePropertiesAccessor.cs)

### Correlation context access

- ICorrelationContextAccessor in [src/Genocs.Messaging/ICorrelationContextAccessor.cs](src/Genocs.Messaging/ICorrelationContextAccessor.cs)
- CorrelationContextAccessor in [src/Genocs.Messaging/CorrelationContextAccessor.cs](src/Genocs.Messaging/CorrelationContextAccessor.cs)

### CQRS bus helper methods

- SendAsync<TCommand>(this IBusPublisher, ...) in [src/Genocs.Messaging/CQRS/Extensions.cs](src/Genocs.Messaging/CQRS/Extensions.cs)
- PublishAsync<TEvent>(this IBusPublisher, ...) in [src/Genocs.Messaging/CQRS/Extensions.cs](src/Genocs.Messaging/CQRS/Extensions.cs)
- SubscribeCommand<T>(this IBusSubscriber) in [src/Genocs.Messaging/CQRS/Extensions.cs](src/Genocs.Messaging/CQRS/Extensions.cs)
- SubscribeEvent<T>(this IBusSubscriber) in [src/Genocs.Messaging/CQRS/Extensions.cs](src/Genocs.Messaging/CQRS/Extensions.cs)

### CQRS dispatcher registration

- AddServiceBusCommandDispatcher(this IGenocsBuilder) in [src/Genocs.Messaging/CQRS/Extensions.cs](src/Genocs.Messaging/CQRS/Extensions.cs)
- AddServiceBusEventDispatcher(this IGenocsBuilder) in [src/Genocs.Messaging/CQRS/Extensions.cs](src/Genocs.Messaging/CQRS/Extensions.cs)

### CQRS dispatcher implementation

- ServiceBusMessageDispatcher in [src/Genocs.Messaging/CQRS/Dispatchers/ServiceBusMessageDispatcher.cs](src/Genocs.Messaging/CQRS/Dispatchers/ServiceBusMessageDispatcher.cs)

## Decision Matrix For Agents

| Goal | Preferred API | Notes |
|---|---|---|
| Publish any message through transport | IBusPublisher.PublishAsync<T> | Core abstraction; transport package handles wire protocol |
| Publish command with message context | IBusPublisher.SendAsync<TCommand> extension | CQRS helper delegates to PublishAsync with messageContext |
| Publish event with message context | IBusPublisher.PublishAsync<TEvent> extension | CQRS helper for IEvent messages |
| Subscribe command and resolve handler | IBusSubscriber.SubscribeCommand<T> | Creates scope and resolves ICommandHandler<T> per message |
| Subscribe event and resolve handler | IBusSubscriber.SubscribeEvent<T> | Creates scope and resolves IEventHandler<T> per message |
| Bridge command dispatcher to bus | AddServiceBusCommandDispatcher | Registers ICommandDispatcher -> ServiceBusMessageDispatcher |
| Bridge event dispatcher to bus | AddServiceBusEventDispatcher | Registers IEventDispatcher -> ServiceBusMessageDispatcher |
| Access ambient correlation context | ICorrelationContextAccessor | AsyncLocal-backed value consumed by dispatcher bridge |

## Minimal Integration Recipe

### Install

```bash
dotnet add package Genocs.Messaging
```

### Setup in Program.cs

```csharp
using Genocs.Core.Builders;
using Genocs.Messaging;
using Genocs.Messaging.CQRS;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder gnxBuilder = builder.AddGenocs();

gnxBuilder
    .AddServiceBusCommandDispatcher()
    .AddServiceBusEventDispatcher();

builder.Services.AddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
builder.Services.AddSingleton<IMessagePropertiesAccessor, MessagePropertiesAccessor>();

gnxBuilder.Build();

var app = builder.Build();

app.Run();
```

## Behavior Notes That Affect Agent Decisions

- IBusPublisher is the single publishing abstraction; CQRS SendAsync and PublishAsync helpers are thin wrappers.
- SubscribeCommand and SubscribeEvent create IServiceScope for each message, ensuring handler dependencies are resolved per-message.
- ServiceBusMessageDispatcher reads ICorrelationContextAccessor.CorrelationContext and forwards it as messageContext.
- AddServiceBusCommandDispatcher and AddServiceBusEventDispatcher register ServiceBusMessageDispatcher as transient.
- MessagePropertiesAccessor and CorrelationContextAccessor both use AsyncLocal holder classes for ambient context flow.
- Setting accessor values clears previously held context in the same AsyncLocal slot before assigning a new one.
- MessageAttribute carries routing metadata fields but does not enforce broker behavior by itself.

## Source-Accurate Capability Map

### Publish and subscribe abstractions

- Defines a generic publish contract with optional IDs, headers, span context, and message context.
- Defines a generic subscribe contract receiving service provider, message payload, and message context.
- Exposes routing metadata container attribute for message classes.

Files:

- [src/Genocs.Messaging/IBusPublisher.cs](src/Genocs.Messaging/IBusPublisher.cs)
- [src/Genocs.Messaging/IBusSubscriber.cs](src/Genocs.Messaging/IBusSubscriber.cs)
- [src/Genocs.Messaging/MessageAttribute.cs](src/Genocs.Messaging/MessageAttribute.cs)

### Message metadata model

- Defines immutable metadata contract for MessageId, CorrelationId, Timestamp, and headers.
- Provides mutable concrete metadata model.
- Provides accessor interface for ambient metadata retrieval.
- Provides AsyncLocal-backed accessor implementation.

Files:

- [src/Genocs.Messaging/IMessageProperties.cs](src/Genocs.Messaging/IMessageProperties.cs)
- [src/Genocs.Messaging/MessageProperties.cs](src/Genocs.Messaging/MessageProperties.cs)
- [src/Genocs.Messaging/IMessagePropertiesAccessor.cs](src/Genocs.Messaging/IMessagePropertiesAccessor.cs)
- [src/Genocs.Messaging/MessagePropertiesAccessor.cs](src/Genocs.Messaging/MessagePropertiesAccessor.cs)

### Correlation context propagation

- Defines ambient correlation context accessor contract.
- Stores correlation context in AsyncLocal to flow through async calls.
- Supports clearing and replacing context safely.

Files:

- [src/Genocs.Messaging/ICorrelationContextAccessor.cs](src/Genocs.Messaging/ICorrelationContextAccessor.cs)
- [src/Genocs.Messaging/CorrelationContextAccessor.cs](src/Genocs.Messaging/CorrelationContextAccessor.cs)

### CQRS publish helper extensions

- Provides command send extension targeting ICommand.
- Provides event publish extension targeting IEvent.
- Normalizes messageContext forwarding to underlying IBusPublisher.

Files:

- [src/Genocs.Messaging/CQRS/Extensions.cs](src/Genocs.Messaging/CQRS/Extensions.cs)

### CQRS subscriber helper extensions

- Subscribes command type and resolves ICommandHandler<T> from scoped provider.
- Subscribes event type and resolves IEventHandler<T> from scoped provider.
- Encapsulates handler invocation wiring and disposal scope.

Files:

- [src/Genocs.Messaging/CQRS/Extensions.cs](src/Genocs.Messaging/CQRS/Extensions.cs)

### CQRS dispatcher bridge to bus

- Registers command and event dispatchers mapped to ServiceBusMessageDispatcher.
- Implements ICommandDispatcher and IEventDispatcher on one class.
- Forwards CQRS operations to IBusPublisher with ambient correlation context.

Files:

- [src/Genocs.Messaging/CQRS/Extensions.cs](src/Genocs.Messaging/CQRS/Extensions.cs)
- [src/Genocs.Messaging/CQRS/Dispatchers/ServiceBusMessageDispatcher.cs](src/Genocs.Messaging/CQRS/Dispatchers/ServiceBusMessageDispatcher.cs)

## Dependencies

From [src/Genocs.Messaging/Genocs.Messaging.csproj](src/Genocs.Messaging/Genocs.Messaging.csproj):

- Genocs.Core

## Related Docs

- NuGet package readme: [src/Genocs.Messaging/README_NUGET.md](src/Genocs.Messaging/README_NUGET.md)
- Repository guide: [README.md](README.md)
- Package documentation: [docs/Genocs.Messaging-Agent-Documentation.md](docs/Genocs.Messaging-Agent-Documentation.md)
