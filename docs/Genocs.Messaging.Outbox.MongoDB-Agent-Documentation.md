# Genocs.Messaging.Outbox.MongoDB Agent Reference

## Purpose

This document is optimized for AI-assisted development sessions.
It prioritizes fast retrieval of:

- What Genocs.Messaging.Outbox.MongoDB is responsible for
- Which APIs to call for specific goals
- Where source of truth lives
- What constraints and runtime behaviors matter

## Quick Facts

| Key | Value |
|---|---|
| Package | Genocs.Messaging.Outbox.MongoDB |
| Project file | [src/Genocs.Messaging.Outbox.MongoDB/Genocs.Messaging.Outbox.MongoDB.csproj](src/Genocs.Messaging.Outbox.MongoDB/Genocs.Messaging.Outbox.MongoDB.csproj) |
| Target frameworks | net10.0, net9.0, net8.0 |
| Primary role | MongoDB-backed implementation of outbox/inbox storage and initialization for Genocs.Messaging.Outbox |
| Core themes | AddMongo configurator extension, MongoMessageOutbox implementation, Mongo TTL indexes, message serialization/deserialization, optional transaction support |

## Use This Package When

- Persisting outbox and inbox records in MongoDB.
- Replacing in-memory outbox storage with durable collections.
- Enabling TTL cleanup for processed inbox/outbox records.
- Using Mongo transaction boundaries during inbox deduplication and handler execution.
- Wiring outbox repositories through Genocs.Persistence.MongoDB.

## Do Not Assume

- This package does not register outbox by itself; it extends IMessageOutboxConfigurator and must be used from AddMessageOutbox.
- Transactions are disabled when outbox disableTransactions option is true.
- Message and messageContext runtime types must be resolvable at deserialization time from saved assembly-qualified type names.

## High-Value Entry Points

### Configurator integration

- AddMongo in [src/Genocs.Messaging.Outbox.MongoDB/Extensions.cs](src/Genocs.Messaging.Outbox.MongoDB/Extensions.cs)

### Outbox implementation

- MongoMessageOutbox in [src/Genocs.Messaging.Outbox.MongoDB/Internals/MongoMessageOutbox.cs](src/Genocs.Messaging.Outbox.MongoDB/Internals/MongoMessageOutbox.cs)

### Initialization and indexes

- MongoOutboxInitializer in [src/Genocs.Messaging.Outbox.MongoDB/Internals/MongoOutboxInitializer.cs](src/Genocs.Messaging.Outbox.MongoDB/Internals/MongoOutboxInitializer.cs)

### Core models and options consumed

- OutboxOptions in [src/Genocs.Messaging.Outbox/Configurations/OutboxOptions.cs](src/Genocs.Messaging.Outbox/Configurations/OutboxOptions.cs)
- OutboxMessage in [src/Genocs.Messaging.Outbox/Messages/OutboxMessage.cs](src/Genocs.Messaging.Outbox/Messages/OutboxMessage.cs)
- InboxMessage in [src/Genocs.Messaging.Outbox/Messages/InboxMessage.cs](src/Genocs.Messaging.Outbox/Messages/InboxMessage.cs)

### Related outbox abstractions

- IMessageOutbox in [src/Genocs.Messaging.Outbox/IMessageOutbox.cs](src/Genocs.Messaging.Outbox/IMessageOutbox.cs)
- IMessageOutboxAccessor in [src/Genocs.Messaging.Outbox/IMessageOutboxAccessor.cs](src/Genocs.Messaging.Outbox/IMessageOutboxAccessor.cs)
- IMessageOutboxConfigurator in [src/Genocs.Messaging.Outbox/IMessageOutboxConfigurator.cs](src/Genocs.Messaging.Outbox/IMessageOutboxConfigurator.cs)

### Integration usage reference

- Demo registration in [src/demo/WebApi/Program.cs](src/demo/WebApi/Program.cs)

## Decision Matrix For Agents

| Goal | Preferred API | Notes |
|---|---|---|
| Switch outbox storage to MongoDB | AddMessageOutbox(o => o.AddMongo()) | Runs from base outbox package configurator |
| Use custom collection names | OutboxOptions.InboxCollection and OutboxOptions.OutboxCollection | Defaults are inbox and outbox |
| Enable TTL cleanup | OutboxOptions.Expiry > 0 | Initializer creates ProcessedAt TTL indexes |
| Enable transactional inbox handling | Keep DisableTransactions false | Wraps handler plus inbox add in session transaction |
| Disable transactional handling | Set DisableTransactions true | Skips session and transaction calls |
| Save outbound message durably | IMessageOutbox.SendAsync on MongoMessageOutbox | Stores serialized payload and type metadata |
| Rehydrate pending unsent messages | IMessageOutboxAccessor.GetUnsentAsync | Deserializes message and messageContext |
| Mark records as processed | IMessageOutboxAccessor.ProcessAsync | Updates ProcessedAt in Mongo collection |

## Minimal Integration Recipe

### Install

```bash
dotnet add package Genocs.Messaging.Outbox.MongoDB
```

### Setup in Program.cs

```csharp
using Genocs.Core.Builders;
using Genocs.Messaging.Outbox;
using Genocs.Messaging.Outbox.MongoDB;
using Genocs.Persistence.MongoDB.Extensions;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder gnxBuilder = builder.AddGenocs();

gnxBuilder
    .AddMongo()
    .AddMessageOutbox(o => o.AddMongo());

gnxBuilder.Build();

var app = builder.Build();

app.Run();
```

## Behavior Notes That Affect Agent Decisions

- AddMongo registers Mongo repositories for InboxMessage and OutboxMessage.
- AddMongo adds a BsonClassMap for OutboxMessage and unmaps Message and MessageContext object members.
- HandleAsync checks duplicate inbox message ID before running handler.
- Transaction commit/abort happens only when transactions are enabled.
- SendAsync serializes message and message context into JSON strings and stores runtime type names.
- GetUnsentAsync deserializes persisted JSON back into runtime objects based on saved type metadata.

## Source-Accurate Capability Map

### Mongo registration layer

- Resolves collection names from options with inbox/outbox defaults.
- Registers Mongo repositories for inbox and outbox entities.
- Registers Mongo outbox services for both IMessageOutbox and accessor interface.
- Registers initializer for TTL index provisioning.

Files:

- [src/Genocs.Messaging.Outbox.MongoDB/Extensions.cs](src/Genocs.Messaging.Outbox.MongoDB/Extensions.cs)

### Durable outbox and inbox processing

- Implements idempotent inbound handling via inbox repository existence check.
- Stores outgoing message metadata and serialized payload in outbox repository.
- Supports optional transaction around handler plus inbox insert.
- Logs processing and storage warnings when disabled.

Files:

- [src/Genocs.Messaging.Outbox.MongoDB/Internals/MongoMessageOutbox.cs](src/Genocs.Messaging.Outbox.MongoDB/Internals/MongoMessageOutbox.cs)

### Pending message rehydration and completion

- Retrieves unsent messages by ProcessedAt equals null.
- Rehydrates Message and MessageContext from serialized JSON and type names.
- Marks single or multiple records processed via update operations.

Files:

- [src/Genocs.Messaging.Outbox.MongoDB/Internals/MongoMessageOutbox.cs](src/Genocs.Messaging.Outbox.MongoDB/Internals/MongoMessageOutbox.cs)

### TTL index initialization

- Creates TTL index for inbox ProcessedAt when expiry is configured.
- Creates TTL index for outbox ProcessedAt when expiry is configured.
- Skips index creation when outbox is disabled or expiry is non-positive.

Files:

- [src/Genocs.Messaging.Outbox.MongoDB/Internals/MongoOutboxInitializer.cs](src/Genocs.Messaging.Outbox.MongoDB/Internals/MongoOutboxInitializer.cs)

### Shared model and option contracts

- Uses OutboxOptions for enabled flag, expiry, transaction toggle, and collection names.
- Reuses shared inbox/outbox message entities from base outbox package.

Files:

- [src/Genocs.Messaging.Outbox/Configurations/OutboxOptions.cs](src/Genocs.Messaging.Outbox/Configurations/OutboxOptions.cs)
- [src/Genocs.Messaging.Outbox/Messages/InboxMessage.cs](src/Genocs.Messaging.Outbox/Messages/InboxMessage.cs)
- [src/Genocs.Messaging.Outbox/Messages/OutboxMessage.cs](src/Genocs.Messaging.Outbox/Messages/OutboxMessage.cs)

### Practical integration reference

- Demonstrates AddMessageOutbox with AddMongo in demo host setup.

Files:

- [src/demo/WebApi/Program.cs](src/demo/WebApi/Program.cs)

## Dependencies

From [src/Genocs.Messaging.Outbox.MongoDB/Genocs.Messaging.Outbox.MongoDB.csproj](src/Genocs.Messaging.Outbox.MongoDB/Genocs.Messaging.Outbox.MongoDB.csproj):

- Genocs.Messaging.Outbox
- Genocs.Persistence.MongoDB

## Related Docs

- NuGet package readme: [src/Genocs.Messaging.Outbox.MongoDB/README_NUGET.md](src/Genocs.Messaging.Outbox.MongoDB/README_NUGET.md)
- Repository guide: [README.md](README.md)
- Package documentation: [docs/Genocs.Messaging-Agent-Documentation.md](docs/Genocs.Messaging-Agent-Documentation.md)
