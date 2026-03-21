# Genocs.Messaging.Outbox Agent Reference

## Purpose

This document is optimized for AI-assisted development sessions.
It prioritizes fast retrieval of:

- What Genocs.Messaging.Outbox is responsible for
- Which APIs to call for specific goals
- Where source of truth lives
- What constraints and runtime behaviors matter

## Quick Facts

| Key | Value |
|---|---|
| Package | Genocs.Messaging.Outbox |
| Project file | [src/Genocs.Messaging.Outbox/Genocs.Messaging.Outbox.csproj](src/Genocs.Messaging.Outbox/Genocs.Messaging.Outbox.csproj) |
| Target frameworks | net10.0, net9.0, net8.0 |
| Primary role | Outbox pattern abstraction and hosted processor for reliable publish and idempotent message handling |
| Core themes | AddMessageOutbox registration, IMessageOutbox abstraction, in-memory outbox, periodic flush processor, inbox/outbox records |

## Use This Package When

- Buffering outbound bus messages before publish.
- Applying idempotent processing for inbound messages via message IDs.
- Running periodic outbox flush logic as a hosted service.
- Selecting outbox implementation via configurator extensions.
- Sharing a transport-agnostic outbox abstraction across messaging backends.

## Do Not Assume

- AddMessageOutbox only starts processing when outbox options are enabled.
- Default configurator behavior is in-memory outbox when no custom configure action is provided.
- Header assignment in SendAsync expects a Dictionary<string, object> at runtime; non-dictionary IDictionary input can fail with cast errors.

## High-Value Entry Points

### Registration and configuration

- AddMessageOutbox in [src/Genocs.Messaging.Outbox/Extensions.cs](src/Genocs.Messaging.Outbox/Extensions.cs)
- AddInMemory in [src/Genocs.Messaging.Outbox/Extensions.cs](src/Genocs.Messaging.Outbox/Extensions.cs)
- OutboxOptions in [src/Genocs.Messaging.Outbox/Configurations/OutboxOptions.cs](src/Genocs.Messaging.Outbox/Configurations/OutboxOptions.cs)
- MessageOutboxConfigurator in [src/Genocs.Messaging.Outbox/Configurators/MessageOutboxConfigurator.cs](src/Genocs.Messaging.Outbox/Configurators/MessageOutboxConfigurator.cs)

### Outbox abstraction contracts

- IMessageOutbox in [src/Genocs.Messaging.Outbox/IMessageOutbox.cs](src/Genocs.Messaging.Outbox/IMessageOutbox.cs)
- IMessageOutboxAccessor in [src/Genocs.Messaging.Outbox/IMessageOutboxAccessor.cs](src/Genocs.Messaging.Outbox/IMessageOutboxAccessor.cs)
- IMessageOutboxConfigurator in [src/Genocs.Messaging.Outbox/IMessageOutboxConfigurator.cs](src/Genocs.Messaging.Outbox/IMessageOutboxConfigurator.cs)

### Hosted processing flow

- OutboxProcessor in [src/Genocs.Messaging.Outbox/Processors/OutboxProcessor.cs](src/Genocs.Messaging.Outbox/Processors/OutboxProcessor.cs)

### In-memory implementation

- InMemoryMessageOutbox in [src/Genocs.Messaging.Outbox/Outbox/InMemoryMessageOutbox.cs](src/Genocs.Messaging.Outbox/Outbox/InMemoryMessageOutbox.cs)

### Message persistence models

- OutboxMessage in [src/Genocs.Messaging.Outbox/Messages/OutboxMessage.cs](src/Genocs.Messaging.Outbox/Messages/OutboxMessage.cs)
- InboxMessage in [src/Genocs.Messaging.Outbox/Messages/InboxMessage.cs](src/Genocs.Messaging.Outbox/Messages/InboxMessage.cs)

### Integration usage reference

- Demo registration in [src/demo/WebApi/Program.cs](src/demo/WebApi/Program.cs)

## Decision Matrix For Agents

| Goal | Preferred API | Notes |
|---|---|---|
| Enable outbox feature | AddMessageOutbox | Reads outbox options and wires processor/services |
| Use default implementation quickly | AddMessageOutbox with no configure delegate | Automatically calls AddInMemory |
| Select in-memory outbox explicitly | AddInMemory | Registers InMemoryMessageOutbox as IMessageOutbox |
| Save outbound message with metadata | IMessageOutbox.SendAsync | Stores message payload, context, IDs, headers, timestamps |
| Process incoming message idempotently | IMessageOutbox.HandleAsync | Uses inbox marker to skip duplicates |
| Pull pending unsent records | IMessageOutboxAccessor.GetUnsentAsync | Used by hosted processor |
| Mark one message processed | IMessageOutboxAccessor.ProcessAsync(OutboxMessage) | Used in sequential mode |
| Mark many messages processed | IMessageOutboxAccessor.ProcessAsync(IEnumerable<OutboxMessage>) | Used in parallel mode |

## Minimal Integration Recipe

### Install

```bash
dotnet add package Genocs.Messaging.Outbox
```

### Setup in Program.cs

```csharp
using Genocs.Core.Builders;
using Genocs.Messaging.Outbox;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder gnxBuilder = builder.AddGenocs();

gnxBuilder.AddMessageOutbox();

gnxBuilder.Build();

var app = builder.Build();

app.Run();
```

## Behavior Notes That Affect Agent Decisions

- AddMessageOutbox uses registry key messageBrokers.outbox and ignores duplicate registration.
- OutboxProcessor validates interval milliseconds and throws when enabled with non-positive interval.
- OutboxProcessor supports sequential and parallel processing modes via outbox type option.
- In sequential mode, each message is marked processed immediately after publish.
- In parallel mode, all published messages are marked processed in one batch.
- InMemoryMessageOutbox removes expired processed records only when expiry is greater than zero.

## Source-Accurate Capability Map

### Registration and lifecycle

- Reads options from configurable section with default key outbox.
- Registers options singleton and implementation services.
- Auto-adds hosted outbox processor when enabled.
- Supports extension-based implementation choice.

Files:

- [src/Genocs.Messaging.Outbox/Extensions.cs](src/Genocs.Messaging.Outbox/Extensions.cs)
- [src/Genocs.Messaging.Outbox/Configurators/MessageOutboxConfigurator.cs](src/Genocs.Messaging.Outbox/Configurators/MessageOutboxConfigurator.cs)
- [src/Genocs.Messaging.Outbox/Configurations/OutboxOptions.cs](src/Genocs.Messaging.Outbox/Configurations/OutboxOptions.cs)

### Abstraction layer

- Defines enabled state and methods for inbox handling and outbound buffering.
- Defines accessor methods for unsent retrieval and processed marking.
- Exposes configurator bridge to builder and options.

Files:

- [src/Genocs.Messaging.Outbox/IMessageOutbox.cs](src/Genocs.Messaging.Outbox/IMessageOutbox.cs)
- [src/Genocs.Messaging.Outbox/IMessageOutboxAccessor.cs](src/Genocs.Messaging.Outbox/IMessageOutboxAccessor.cs)
- [src/Genocs.Messaging.Outbox/IMessageOutboxConfigurator.cs](src/Genocs.Messaging.Outbox/IMessageOutboxConfigurator.cs)

### Hosted dispatch processing

- Periodically reads unsent records from accessor.
- Publishes records through IBusPublisher.
- Applies sequential or batch processed marking strategy.
- Uses DI scope per processing iteration.

Files:

- [src/Genocs.Messaging.Outbox/Processors/OutboxProcessor.cs](src/Genocs.Messaging.Outbox/Processors/OutboxProcessor.cs)

### In-memory storage behavior

- Uses concurrent dictionaries for inbox and outbox state.
- Implements duplicate detection for inbound IDs.
- Stores outbound payload and metadata in OutboxMessage.
- Removes processed records after expiry window.

Files:

- [src/Genocs.Messaging.Outbox/Outbox/InMemoryMessageOutbox.cs](src/Genocs.Messaging.Outbox/Outbox/InMemoryMessageOutbox.cs)

### Message entity models

- Outbox entity stores serialized/raw payload metadata and lifecycle timestamps.
- Inbox entity stores processed marker and timestamp.
- Both entities implement IEntity<string>.

Files:

- [src/Genocs.Messaging.Outbox/Messages/OutboxMessage.cs](src/Genocs.Messaging.Outbox/Messages/OutboxMessage.cs)
- [src/Genocs.Messaging.Outbox/Messages/InboxMessage.cs](src/Genocs.Messaging.Outbox/Messages/InboxMessage.cs)

### Practical integration reference

- Demonstrates AddMessageOutbox with Mongo implementation selection.

Files:

- [src/demo/WebApi/Program.cs](src/demo/WebApi/Program.cs)

## Dependencies

From [src/Genocs.Messaging.Outbox/Genocs.Messaging.Outbox.csproj](src/Genocs.Messaging.Outbox/Genocs.Messaging.Outbox.csproj):

- Genocs.Messaging

## Related Docs

- NuGet package readme: [src/Genocs.Messaging.Outbox/README_NUGET.md](src/Genocs.Messaging.Outbox/README_NUGET.md)
- Repository guide: [README.md](README.md)
- Package documentation: [docs/Genocs.Messaging.Outbox-Agent-Documentation.md](docs/Genocs.Messaging.Outbox-Agent-Documentation.md)
