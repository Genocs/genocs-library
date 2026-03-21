# Genocs.Messaging.RabbitMQ Agent Reference

## Purpose

This document is optimized for AI-assisted development sessions.
It prioritizes fast retrieval of:

- What Genocs.Messaging.RabbitMQ is responsible for
- Which APIs to call for specific goals
- Where source of truth lives
- What constraints and runtime behaviors matter

## Quick Facts

| Key | Value |
|---|---|
| Package | Genocs.Messaging.RabbitMQ |
| Project file | [src/Genocs.Messaging.RabbitMQ/Genocs.Messaging.RabbitMQ.csproj](src/Genocs.Messaging.RabbitMQ/Genocs.Messaging.RabbitMQ.csproj) |
| Target frameworks | net10.0, net9.0, net8.0 |
| Primary role | RabbitMQ transport implementation for Genocs messaging abstractions with conventions, retries, DLQ flow, plugins, and tracing headers |
| Core themes | AddRabbitMQAsync registration, IBusPublisher and IBusSubscriber implementations, conventions builder/provider, background consumer service, plugin chain, serializer abstraction |

## Use This Package When

- Binding Genocs.Messaging abstractions to RabbitMQ transport.
- Publishing messages with conventions-driven exchange, routing key, and queue naming.
- Running background consumer handling with retry and optional dead-letter routing.
- Propagating correlation context and tracing headers over RabbitMQ.
- Extending message handling with plugin middleware and custom exception mappers.

## Do Not Assume

- AddRabbitMQAsync is asynchronous and must be awaited.
- Host names are required; registration throws when rabbitmq host list is empty.
- UseRabbitMQ returns a subscriber facade and does not auto-subscribe handlers unless Subscribe calls are made.
- Producer channels are created per managed thread and bounded by max producer channels.
- Retries in background consumer do not imply automatic success; failure mapping and dead-letter settings alter final ack/nack behavior.

## High-Value Entry Points

### Registration and host integration

- AddRabbitMQAsync in [src/Genocs.Messaging.RabbitMQ/Extensions.cs](src/Genocs.Messaging.RabbitMQ/Extensions.cs)
- UseRabbitMQ in [src/Genocs.Messaging.RabbitMQ/Extensions.cs](src/Genocs.Messaging.RabbitMQ/Extensions.cs)
- RabbitMQOptions in [src/Genocs.Messaging.RabbitMQ/RabbitMQOptions.cs](src/Genocs.Messaging.RabbitMQ/RabbitMQOptions.cs)

### Publish and subscribe abstractions

- RabbitMQPublisher in [src/Genocs.Messaging.RabbitMQ/Publishers/RabbitMqPublisher.cs](src/Genocs.Messaging.RabbitMQ/Publishers/RabbitMqPublisher.cs)
- RabbitMQSubscriber in [src/Genocs.Messaging.RabbitMQ/Subscribers/RabbitMqSubscriber.cs](src/Genocs.Messaging.RabbitMQ/Subscribers/RabbitMqSubscriber.cs)
- MessageSubscribersChannel in [src/Genocs.Messaging.RabbitMQ/MessageSubscribersChannel.cs](src/Genocs.Messaging.RabbitMQ/MessageSubscribersChannel.cs)
- RabbitMqBackgroundService in [src/Genocs.Messaging.RabbitMQ/Internals/RabbitMqBackgroundService.cs](src/Genocs.Messaging.RabbitMQ/Internals/RabbitMqBackgroundService.cs)

### Client, serialization, and context

- IRabbitMQClient in [src/Genocs.Messaging.RabbitMQ/IRabbitMqClient.cs](src/Genocs.Messaging.RabbitMQ/IRabbitMqClient.cs)
- RabbitMQClient in [src/Genocs.Messaging.RabbitMQ/Clients/RabbitMqClient.cs](src/Genocs.Messaging.RabbitMQ/Clients/RabbitMqClient.cs)
- IRabbitMQSerializer in [src/Genocs.Messaging.RabbitMQ/IRabbitMqSerializer.cs](src/Genocs.Messaging.RabbitMQ/IRabbitMqSerializer.cs)
- SystemTextJsonJsonRabbitMQSerializer in [src/Genocs.Messaging.RabbitMQ/Serializers/SystemTextJsonJsonRabbitMqSerializer.cs](src/Genocs.Messaging.RabbitMQ/Serializers/SystemTextJsonJsonRabbitMqSerializer.cs)
- NewtonsoftJsonRabbitMQSerializer in [src/Genocs.Messaging.RabbitMQ/Serializers/NewtonsoftJsonRabbitMqSerializer.cs](src/Genocs.Messaging.RabbitMQ/Serializers/NewtonsoftJsonRabbitMqSerializer.cs)

### Conventions and routing metadata

- IConventionsBuilder in [src/Genocs.Messaging.RabbitMQ/IConventionsBuilder.cs](src/Genocs.Messaging.RabbitMQ/IConventionsBuilder.cs)
- ConventionsBuilder in [src/Genocs.Messaging.RabbitMQ/Conventions/ConventionsBuilder.cs](src/Genocs.Messaging.RabbitMQ/Conventions/ConventionsBuilder.cs)
- IConventionsProvider in [src/Genocs.Messaging.RabbitMQ/IConventionsProvider.cs](src/Genocs.Messaging.RabbitMQ/IConventionsProvider.cs)
- ConventionsProvider in [src/Genocs.Messaging.RabbitMQ/Conventions/ConventionsProvider.cs](src/Genocs.Messaging.RabbitMQ/Conventions/ConventionsProvider.cs)
- ConventionsRegistry in [src/Genocs.Messaging.RabbitMQ/ConventionsRegistry.cs](src/Genocs.Messaging.RabbitMQ/ConventionsRegistry.cs)

### Plugin and failure extension points

- IRabbitMqPluginsRegistry in [src/Genocs.Messaging.RabbitMQ/IRabbitMqPluginsRegistry.cs](src/Genocs.Messaging.RabbitMQ/IRabbitMqPluginsRegistry.cs)
- RabbitMQPlugin in [src/Genocs.Messaging.RabbitMQ/RabbitMqPlugin.cs](src/Genocs.Messaging.RabbitMQ/RabbitMqPlugin.cs)
- RabbitMqPluginsRegistry in [src/Genocs.Messaging.RabbitMQ/Plugins/RabbitMqPluginsRegistry.cs](src/Genocs.Messaging.RabbitMQ/Plugins/RabbitMqPluginsRegistry.cs)
- RabbitMqPluginsExecutor in [src/Genocs.Messaging.RabbitMQ/Plugins/RabbitMqPluginExecutor.cs](src/Genocs.Messaging.RabbitMQ/Plugins/RabbitMqPluginExecutor.cs)
- AddExceptionToMessageMapper and AddExceptionToFailedMessageMapper in [src/Genocs.Messaging.RabbitMQ/Extensions.cs](src/Genocs.Messaging.RabbitMQ/Extensions.cs)

### Exchange initialization and processing constraints

- RabbitMqExchangeInitializer in [src/Genocs.Messaging.RabbitMQ/Initializers/RabbitMqExchangeInitializer.cs](src/Genocs.Messaging.RabbitMQ/Initializers/RabbitMqExchangeInitializer.cs)
- FailedMessage in [src/Genocs.Messaging.RabbitMQ/FailedMessage.cs](src/Genocs.Messaging.RabbitMQ/FailedMessage.cs)
- RabbitMqMessageProcessingTimeoutException in [src/Genocs.Messaging.RabbitMQ/RabbitMqMessageProcessingTimeoutException.cs](src/Genocs.Messaging.RabbitMQ/RabbitMqMessageProcessingTimeoutException.cs)

## Decision Matrix For Agents

| Goal | Preferred API | Notes |
|---|---|---|
| Register RabbitMQ transport | AddRabbitMQAsync | Wires connections, publisher, subscriber, background service, conventions |
| Add typed subscriptions in pipeline | UseRabbitMQ then Subscribe<T> | Pushes subscriber actions to internal channel consumed by hosted service |
| Publish broker message | IBusPublisher.PublishAsync | Uses RabbitMQPublisher then RabbitMQClient.SendAsync |
| Customize message routing names | IConventionsBuilder and options | Supports MessageAttribute values plus casing and queue template |
| Add pre-handler middleware | plugins argument on AddRabbitMQAsync with IRabbitMqPluginsRegistry.Add<TPlugin> | Builds linked plugin chain around core handler |
| Map exceptions to failure events | AddExceptionToFailedMessageMapper or AddExceptionToMessageMapper | Controls retry, failed publish, and dead-letter path |
| Switch serializer implementation | pass serializer argument to AddRabbitMQAsync or register custom IRabbitMQSerializer | Defaults to System.Text.Json serializer |
| Tune retry and dead-letter behavior | RabbitMQOptions.Retries, RetryInterval, DeadLetter, RequeueFailedMessages | Background service decides ack/nack and DLX behavior |

## Minimal Integration Recipe

### Install

```bash
dotnet add package Genocs.Messaging.RabbitMQ
```

### Setup in Program.cs

```csharp
using Genocs.Core.Builders;
using Genocs.Messaging.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder gnxBuilder = builder.AddGenocs();

await gnxBuilder.AddRabbitMQAsync();

gnxBuilder.Build();

var app = builder.Build();

app.UseRabbitMQ();

app.Run();
```

## Behavior Notes That Affect Agent Decisions

- AddRabbitMQAsync uses registry key messageBrokers.rabbitmq and skips duplicate registrations.
- AddRabbitMQAsync creates two RabbitMQ connections, one consumer and one producer.
- Producer publish path injects traceparent and tracestate headers when activity data is available.
- Consumer path supports retry policy with wait-and-retry based on configured retries and interval.
- When message processing timeout is configured, timed-out handlers trigger RabbitMqMessageProcessingTimeoutException.
- FailedMessage mapping controls whether to retry, acknowledge, or move message to dead-letter flow.

## Source-Accurate Capability Map

### Transport registration and DI wiring

- Registers core messaging services, conventions services, and context accessors.
- Registers background consumer hosted service and exchange initializer.
- Supports plugin registry and plugin executor registration.
- Supports SSL and optional custom connection factory configuration.

Files:

- [src/Genocs.Messaging.RabbitMQ/Extensions.cs](src/Genocs.Messaging.RabbitMQ/Extensions.cs)
- [src/Genocs.Messaging.RabbitMQ/RabbitMQOptions.cs](src/Genocs.Messaging.RabbitMQ/RabbitMQOptions.cs)
- [src/Genocs.Messaging.RabbitMQ/Initializers/RabbitMqExchangeInitializer.cs](src/Genocs.Messaging.RabbitMQ/Initializers/RabbitMqExchangeInitializer.cs)

### Publish pipeline

- Resolves routing conventions by message type.
- Serializes payload and builds RabbitMQ basic properties.
- Adds correlation, message ID, timestamp, optional context, and tracing headers.
- Publishes using producer channel pool keyed by managed thread id.

Files:

- [src/Genocs.Messaging.RabbitMQ/Publishers/RabbitMqPublisher.cs](src/Genocs.Messaging.RabbitMQ/Publishers/RabbitMqPublisher.cs)
- [src/Genocs.Messaging.RabbitMQ/Clients/RabbitMqClient.cs](src/Genocs.Messaging.RabbitMQ/Clients/RabbitMqClient.cs)
- [src/Genocs.Messaging.RabbitMQ/Contexts/ContextProvider.cs](src/Genocs.Messaging.RabbitMQ/Contexts/ContextProvider.cs)

### Subscribe and consume runtime

- Queues subscribe actions through MessageSubscribersChannel.
- Declares and binds queues with optional dead-letter setup.
- Processes incoming messages with scoped services and correlation/message properties accessors.
- Acknowledges, retries, nacks, and dead-letter transitions based on processing outcome.

Files:

- [src/Genocs.Messaging.RabbitMQ/Subscribers/RabbitMqSubscriber.cs](src/Genocs.Messaging.RabbitMQ/Subscribers/RabbitMqSubscriber.cs)
- [src/Genocs.Messaging.RabbitMQ/Internals/RabbitMqBackgroundService.cs](src/Genocs.Messaging.RabbitMQ/Internals/RabbitMqBackgroundService.cs)
- [src/Genocs.Messaging.RabbitMQ/MessageSubscribersChannel.cs](src/Genocs.Messaging.RabbitMQ/MessageSubscribersChannel.cs)

### Conventions and routing computation

- Computes routing key, exchange, and queue from options plus MessageAttribute metadata.
- Supports snake_case naming mode and queue template token replacement.
- Caches resolved conventions and allows explicit overrides in registry.

Files:

- [src/Genocs.Messaging.RabbitMQ/Conventions/ConventionsBuilder.cs](src/Genocs.Messaging.RabbitMQ/Conventions/ConventionsBuilder.cs)
- [src/Genocs.Messaging.RabbitMQ/Conventions/ConventionsProvider.cs](src/Genocs.Messaging.RabbitMQ/Conventions/ConventionsProvider.cs)
- [src/Genocs.Messaging.RabbitMQ/ConventionsRegistry.cs](src/Genocs.Messaging.RabbitMQ/ConventionsRegistry.cs)

### Plugin chain and failure mapping

- Supports chain-of-responsibility plugin execution around handler invocation.
- Allows custom exception-to-message mapping for legacy behavior.
- Supports richer failure mapping with retry and dead-letter intent flags.

Files:

- [src/Genocs.Messaging.RabbitMQ/RabbitMqPlugin.cs](src/Genocs.Messaging.RabbitMQ/RabbitMqPlugin.cs)
- [src/Genocs.Messaging.RabbitMQ/Plugins/RabbitMqPluginExecutor.cs](src/Genocs.Messaging.RabbitMQ/Plugins/RabbitMqPluginExecutor.cs)
- [src/Genocs.Messaging.RabbitMQ/FailedMessage.cs](src/Genocs.Messaging.RabbitMQ/FailedMessage.cs)

### Serializer and options surface

- Defines serializer contract for generic and typed deserialization.
- Provides System.Text.Json default serializer.
- Provides Newtonsoft.Json serializer alternative.
- Exposes extensive RabbitMQ option model for connection, queue, exchange, DLQ, QoS, context, and logging.

Files:

- [src/Genocs.Messaging.RabbitMQ/IRabbitMqSerializer.cs](src/Genocs.Messaging.RabbitMQ/IRabbitMqSerializer.cs)
- [src/Genocs.Messaging.RabbitMQ/Serializers/SystemTextJsonJsonRabbitMqSerializer.cs](src/Genocs.Messaging.RabbitMQ/Serializers/SystemTextJsonJsonRabbitMqSerializer.cs)
- [src/Genocs.Messaging.RabbitMQ/Serializers/NewtonsoftJsonRabbitMqSerializer.cs](src/Genocs.Messaging.RabbitMQ/Serializers/NewtonsoftJsonRabbitMqSerializer.cs)
- [src/Genocs.Messaging.RabbitMQ/RabbitMQOptions.cs](src/Genocs.Messaging.RabbitMQ/RabbitMQOptions.cs)

## Dependencies

From [src/Genocs.Messaging.RabbitMQ/Genocs.Messaging.RabbitMQ.csproj](src/Genocs.Messaging.RabbitMQ/Genocs.Messaging.RabbitMQ.csproj):

- Genocs.Messaging
- RabbitMQ.Client
- Polly
- Newtonsoft.Json

## Related Docs

- NuGet package readme: [src/Genocs.Messaging.RabbitMQ/README_NUGET.md](src/Genocs.Messaging.RabbitMQ/README_NUGET.md)
- Repository guide: [README.md](README.md)
- Package documentation: [docs/Genocs.Messaging-Agent-Documentation.md](docs/Genocs.Messaging-Agent-Documentation.md)
