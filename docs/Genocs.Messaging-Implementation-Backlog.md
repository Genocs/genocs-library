# Genocs.Messaging Implementation Backlog

## Purpose

This backlog translates the architectural and quality concerns observed in Genocs.Messaging and related transport/outbox packages into issue-sized implementation work.

The backlog is ordered by delivery risk and runtime impact, not by namespace.

## Current Status

Observed baseline (April 2026):

- No dedicated messaging implementation backlog file existed before this analysis.
- Core abstractions exist in [src/Genocs.Messaging](src/Genocs.Messaging), but key CQRS dispatcher behavior remains partially implemented.
- RabbitMQ runtime is feature-rich, but contains settlement and DI-scope handling risks in subscriber execution paths.
- Outbox packages compile but emit recurring nullable warnings and contain entity contract gaps.
- Azure Service Bus package compiles but emits warning noise and still relies on legacy handler contracts.
- Test coverage is present for tracing only: [src/tests/Genocs.Messaging.Tracing.UnitTests](src/tests/Genocs.Messaging.Tracing.UnitTests) and [src/tests/Genocs.Messaging.Tracing.IntegrationTests](src/tests/Genocs.Messaging.Tracing.IntegrationTests).

Latest validation runs:

- dotnet build src/Genocs.Messaging/Genocs.Messaging.csproj -c Debug --nologo
- dotnet build src/Genocs.Messaging.RabbitMQ/Genocs.Messaging.RabbitMQ.csproj -c Debug --nologo
- dotnet build src/Genocs.Messaging.Outbox/Genocs.Messaging.Outbox.csproj -c Debug --nologo
- dotnet build src/Genocs.Messaging.Outbox.MongoDB/Genocs.Messaging.Outbox.MongoDB.csproj -c Debug --nologo
- dotnet build src/Genocs.Messaging.AzureServiceBus/Genocs.Messaging.AzureServiceBus.csproj -c Debug --nologo

Builds succeeded, but with warning-heavy baseline (notably Outbox and Azure Service Bus).

## Planning Assumptions

- Address correctness and runtime safety before expanding API surface.
- Reduce warning volume to improve signal-to-noise for future regressions.
- Prefer transport-agnostic contracts for host-facing APIs; keep provider specifics behind package boundaries.
- Pair behavior changes with focused unit/integration tests in package-specific test projects.
- Keep migration guidance synchronized across package README and agent documentation.

## Milestones

| Milestone | Goal | Tasks |
|---|---|---|
| M1 | Runtime correctness and safety baseline | MESSAGING-001 to MESSAGING-005 |
| M2 | Outbox contract and nullability hardening | MESSAGING-006 to MESSAGING-008 |
| M3 | Azure Service Bus modernization and contract alignment | MESSAGING-009 to MESSAGING-011 |
| M4 | Testability, docs, and quality gates | MESSAGING-012 to MESSAGING-014 |

## Execution Order

1. Complete M1 before additional transport feature work.
2. Complete M2 before adopting stricter warning policies.
3. Complete M3 before recommending Azure Service Bus package for new services.
4. Complete M4 to lock in long-term maintainability and adoption readiness.

---

## M1: Runtime Correctness and Safety Baseline

### MESSAGING-001 Implement missing generic command-dispatcher path

**Status**: Completed (April 2026)

**Priority**: P0

**Problem**

`ICommandDispatcher.SendAsync<TCommand, TResult>` currently throws `NotImplementedException`, causing runtime failures for command flows that require result payloads.

**Likely touch points**

- [src/Genocs.Messaging/CQRS/Dispatchers/ServiceBusMessageDispatcher.cs](src/Genocs.Messaging/CQRS/Dispatchers/ServiceBusMessageDispatcher.cs)

**Acceptance criteria**

- No `NotImplementedException` remains in command-dispatcher execution paths.
- Generic dispatch behavior is explicitly defined (supported or intentionally rejected with clear contract).
- Unit tests cover both fire-and-forget and result-return command paths.

**Dependencies**

- none

**Implementation notes**

- Replaced `NotImplementedException` in `ServiceBusMessageDispatcher` with an explicit `NotSupportedException` that documents request/response commands are not supported over service-bus dispatch.
- Added unit tests in [src/tests/Genocs.Messaging.UnitTests/CQRS/ServiceBusMessageDispatcherTests.cs](src/tests/Genocs.Messaging.UnitTests/CQRS/ServiceBusMessageDispatcherTests.cs) to cover:
	- fire-and-forget command publishing with correlation context propagation,
	- result-return command rejection with a clear contract exception.

### MESSAGING-002 Propagate cancellation tokens through CQRS-to-bus bridge

**Status**: Completed (April 2026)

**Priority**: P0

**Problem**

Cancellation tokens accepted by dispatcher methods are currently not propagated to underlying publish operations.

**Likely touch points**

- [src/Genocs.Messaging/CQRS/Dispatchers/ServiceBusMessageDispatcher.cs](src/Genocs.Messaging/CQRS/Dispatchers/ServiceBusMessageDispatcher.cs)
- [src/Genocs.Messaging/CQRS/Extensions.cs](src/Genocs.Messaging/CQRS/Extensions.cs)
- [src/Genocs.Messaging.RabbitMQ/Publishers/RabbitMQPublisher.cs](src/Genocs.Messaging.RabbitMQ/Publishers/RabbitMQPublisher.cs)

**Acceptance criteria**

- Cancellation tokens are forwarded from dispatcher to bus publisher consistently.
- Cancellation semantics are documented for command and event paths.

**Dependencies**

- none

**Implementation notes**

- Added cancellation-token forwarding from `ServiceBusMessageDispatcher` to `IBusPublisher` bridge methods for both command and event paths.
- Updated messaging CQRS extension methods to accept nullable message context and forward `CancellationToken` to `IBusPublisher.PublishAsync(...)`.
- Updated RabbitMQ publisher to honor pre-dispatch cancellation via `cancellationToken.ThrowIfCancellationRequested()` before handing off to the RabbitMQ client.
- Extended unit coverage in [src/tests/Genocs.Messaging.UnitTests/CQRS/ServiceBusMessageDispatcherTests.cs](src/tests/Genocs.Messaging.UnitTests/CQRS/ServiceBusMessageDispatcherTests.cs) to verify cancellation token forwarding on command and event dispatch.

### MESSAGING-003 Eliminate service-provider build during RabbitMQ registration

**Status**: Completed (April 2026)

**Priority**: P1

**Problem**

RabbitMQ registration builds a temporary `ServiceProvider` during service registration to resolve a logger. This can create duplicate singleton instances and non-deterministic startup behavior.

**Likely touch points**

- [src/Genocs.Messaging.RabbitMQ/Extensions.cs](src/Genocs.Messaging.RabbitMQ/Extensions.cs)

**Acceptance criteria**

- Registration path no longer calls `BuildServiceProvider()`.
- Logging still works with standard DI lifecycle semantics.

**Dependencies**

- none

**Implementation notes**

- Removed temporary `BuildServiceProvider()` usage from RabbitMQ registration in [src/Genocs.Messaging.RabbitMQ/Extensions.cs](src/Genocs.Messaging.RabbitMQ/Extensions.cs).
- Kept registration-time diagnostics non-invasive by avoiding premature service resolution; runtime components continue resolving typed loggers from the final application container.

### MESSAGING-004 Fix subscriber handler DI scope usage

**Status**: Completed (April 2026)

**Priority**: P0

**Problem**

RabbitMQ consumer flow creates a scope but invokes handlers with the root service provider, risking incorrect scoped dependency resolution and scope-leak behavior.

**Likely touch points**

- [src/Genocs.Messaging.RabbitMQ/Internals/RabbitMqBackgroundService.cs](src/Genocs.Messaging.RabbitMQ/Internals/RabbitMqBackgroundService.cs)

**Acceptance criteria**

- Handler execution uses the intended scope service provider.
- Scoped dependencies resolve correctly in message handlers.
- Integration test verifies scoped service lifetimes per consumed message.

**Dependencies**

- `MESSAGING-003`

**Implementation notes**

- Updated [src/Genocs.Messaging.RabbitMQ/Internals/RabbitMqBackgroundService.cs](src/Genocs.Messaging.RabbitMQ/Internals/RabbitMqBackgroundService.cs) to pass the per-message scope service provider into handler execution.
- Ensured `TryHandleAsync(...)` invokes handlers with the scoped provider (including timeout path), eliminating root-provider usage during consumer handling.
- Added focused coverage in [src/tests/Genocs.Messaging.RabbitMQ.UnitTests/Internals/RabbitMqBackgroundServiceScopeTests.cs](src/tests/Genocs.Messaging.RabbitMQ.UnitTests/Internals/RabbitMqBackgroundServiceScopeTests.cs), validating distinct scoped dependency instances across consumed messages.

### MESSAGING-005 Make message settlement deterministic (await ack/nack)

**Status**: Completed (April 2026)

**Priority**: P0

**Problem**

Multiple `BasicAckAsync` and `BasicNackAsync` calls are issued without awaiting completion, creating a risk of out-of-order settlement or hidden failures under load.

**Likely touch points**

- [src/Genocs.Messaging.RabbitMQ/Internals/RabbitMqBackgroundService.cs](src/Genocs.Messaging.RabbitMQ/Internals/RabbitMqBackgroundService.cs)

**Acceptance criteria**

- Ack/nack operations are awaited (or intentionally batched with explicit rationale).
- Message settlement behavior is deterministic and covered by integration tests.

**Dependencies**

- `MESSAGING-004`

**Implementation notes**

- Updated settlement paths in [src/Genocs.Messaging.RabbitMQ/Internals/RabbitMqBackgroundService.cs](src/Genocs.Messaging.RabbitMQ/Internals/RabbitMqBackgroundService.cs) to await all `BasicAckAsync(...)` and `BasicNackAsync(...)` calls, removing fire-and-forget settlement behavior.
- Added settlement-focused assertions in [src/tests/Genocs.Messaging.RabbitMQ.UnitTests/Internals/RabbitMqBackgroundServiceScopeTests.cs](src/tests/Genocs.Messaging.RabbitMQ.UnitTests/Internals/RabbitMqBackgroundServiceScopeTests.cs):
	- successful handler flow verifies ack invocation,
	- failing handler flow verifies nack invocation and no ack.

---

## M2: Outbox Contract and Nullability Hardening

### MESSAGING-006 Complete entity lifecycle contract in outbox message models

**Status**: Completed (April 2026)

**Priority**: P0

**Problem**

`InboxMessage.IsTransient()` and `OutboxMessage.IsTransient()` throw `NotImplementedException`, which can fail repository logic paths that rely on entity lifecycle semantics.

**Likely touch points**

- [src/Genocs.Messaging.Outbox/Messages/InboxMessage.cs](src/Genocs.Messaging.Outbox/Messages/InboxMessage.cs)
- [src/Genocs.Messaging.Outbox/Messages/OutboxMessage.cs](src/Genocs.Messaging.Outbox/Messages/OutboxMessage.cs)

**Acceptance criteria**

- Entity lifecycle methods are implemented and documented.
- Repository operations no longer risk runtime exceptions due to unimplemented lifecycle contracts.

**Dependencies**

- none

**Implementation notes**

- Implemented `IsTransient()` in [src/Genocs.Messaging.Outbox/Messages/InboxMessage.cs](src/Genocs.Messaging.Outbox/Messages/InboxMessage.cs) and [src/Genocs.Messaging.Outbox/Messages/OutboxMessage.cs](src/Genocs.Messaging.Outbox/Messages/OutboxMessage.cs) using string-key lifecycle semantics (`null`, empty, or whitespace ID is transient).
- Added lifecycle coverage in [src/tests/Genocs.Messaging.UnitTests/Outbox/MessageLifecycleTests.cs](src/tests/Genocs.Messaging.UnitTests/Outbox/MessageLifecycleTests.cs) and referenced the outbox package in [src/tests/Genocs.Messaging.UnitTests/Genocs.Messaging.UnitTests.csproj](src/tests/Genocs.Messaging.UnitTests/Genocs.Messaging.UnitTests.csproj).

### MESSAGING-007 Normalize nullable API contracts in outbox abstractions

**Status**: Completed (April 2026)

**Priority**: P1

**Problem**

Outbox interfaces and models generate repeated nullable warnings (CS8625, CS8618, CS8634), reducing confidence in null-safety and masking real regressions.

**Likely touch points**

- [src/Genocs.Messaging.Outbox/IMessageOutbox.cs](src/Genocs.Messaging.Outbox/IMessageOutbox.cs)
- [src/Genocs.Messaging.Outbox/Configurations/OutboxOptions.cs](src/Genocs.Messaging.Outbox/Configurations/OutboxOptions.cs)
- [src/Genocs.Messaging.Outbox/Outbox/InMemoryMessageOutbox.cs](src/Genocs.Messaging.Outbox/Outbox/InMemoryMessageOutbox.cs)
- [src/Genocs.Messaging.Outbox.MongoDB/Internals/MongoMessageOutbox.cs](src/Genocs.Messaging.Outbox.MongoDB/Internals/MongoMessageOutbox.cs)

**Acceptance criteria**

- Outbox projects build with no nullable warnings in touched files.
- Public contracts clearly distinguish optional versus required values.

**Dependencies**

- `MESSAGING-006`

**Implementation notes**

- Normalized nullable contracts in [src/Genocs.Messaging.Outbox/IMessageOutbox.cs](src/Genocs.Messaging.Outbox/IMessageOutbox.cs), [src/Genocs.Messaging.Outbox/Outbox/InMemoryMessageOutbox.cs](src/Genocs.Messaging.Outbox/Outbox/InMemoryMessageOutbox.cs), and [src/Genocs.Messaging.Outbox.MongoDB/Internals/MongoMessageOutbox.cs](src/Genocs.Messaging.Outbox.MongoDB/Internals/MongoMessageOutbox.cs) so optional message metadata and headers align across interface and implementations.
- Added safe defaults in [src/Genocs.Messaging.Outbox/Configurations/OutboxOptions.cs](src/Genocs.Messaging.Outbox/Configurations/OutboxOptions.cs) and tightened lifecycle model nullability in [src/Genocs.Messaging.Outbox/Messages/InboxMessage.cs](src/Genocs.Messaging.Outbox/Messages/InboxMessage.cs) and [src/Genocs.Messaging.Outbox/Messages/OutboxMessage.cs](src/Genocs.Messaging.Outbox/Messages/OutboxMessage.cs).
- Hardened outbox processing path in [src/Genocs.Messaging.Outbox/Processors/OutboxProcessor.cs](src/Genocs.Messaging.Outbox/Processors/OutboxProcessor.cs) for null payload/header handling so publisher calls satisfy non-null generic constraints without warning noise.
- Validation: `dotnet build src/Genocs.Messaging.Outbox/Genocs.Messaging.Outbox.csproj -c Debug --nologo`, `dotnet build src/Genocs.Messaging.Outbox.MongoDB/Genocs.Messaging.Outbox.MongoDB.csproj -c Debug --nologo`, and `dotnet test src/tests/Genocs.Messaging.UnitTests/Genocs.Messaging.UnitTests.csproj -c Debug --nologo`.

### MESSAGING-008 Define safe default behavior for outbox provider registration

**Status**: Completed (April 2026)

**Priority**: P1

**Problem**

Defaulting to in-memory outbox when no configurator is provided can lead to accidental non-durable behavior in production environments.

**Likely touch points**

- [src/Genocs.Messaging.Outbox/Extensions.cs](src/Genocs.Messaging.Outbox/Extensions.cs)
- [src/Genocs.Messaging.Outbox/README_NUGET.md](src/Genocs.Messaging.Outbox/README_NUGET.md)
- [docs/Genocs.Messaging.Outbox-Agent-Documentation.md](docs/Genocs.Messaging.Outbox-Agent-Documentation.md)

**Acceptance criteria**

- Startup behavior is explicit when durable provider is not configured.
- Documentation clearly states production versus development defaults.

**Dependencies**

- `MESSAGING-007`

**Implementation notes**

- Updated [src/Genocs.Messaging.Outbox/Extensions.cs](src/Genocs.Messaging.Outbox/Extensions.cs) so `AddMessageOutbox(...)` now fails fast when no configurator callback is supplied, preventing implicit in-memory fallback in production hosts.
- Added targeted registration tests in [src/tests/Genocs.Messaging.UnitTests/Outbox/OutboxRegistrationExtensionsTests.cs](src/tests/Genocs.Messaging.UnitTests/Outbox/OutboxRegistrationExtensionsTests.cs) to lock explicit provider behavior and explicit in-memory development registration.
- Updated [src/Genocs.Messaging.Outbox/README_NUGET.md](src/Genocs.Messaging.Outbox/README_NUGET.md) and [docs/Genocs.Messaging.Outbox-Agent-Documentation.md](docs/Genocs.Messaging.Outbox-Agent-Documentation.md) to clearly distinguish development-only in-memory usage from durable production provider guidance.
- Validation: `dotnet build src/Genocs.Messaging.Outbox/Genocs.Messaging.Outbox.csproj -c Debug --nologo` and `dotnet test src/tests/Genocs.Messaging.UnitTests/Genocs.Messaging.UnitTests.csproj -c Debug --nologo`.

---

## M3: Azure Service Bus Modernization and Contract Alignment

### MESSAGING-009 Add DI/host registration extensions for Azure Service Bus package

**Status**: Completed (April 2026)

**Priority**: P1

**Problem**

The Azure Service Bus package currently lacks a clear builder/host registration extension path aligned with other Genocs transport packages, increasing integration friction.

**Likely touch points**

- [src/Genocs.Messaging.AzureServiceBus](src/Genocs.Messaging.AzureServiceBus)
- [src/Genocs.Messaging.AzureServiceBus/README_NUGET.md](src/Genocs.Messaging.AzureServiceBus/README_NUGET.md)

**Acceptance criteria**

- A standard extension-based registration path is available and documented.
- Host startup does not require consumers to manually wire internals.

**Dependencies**

- none

**Implementation notes**

- Added [src/Genocs.Messaging.AzureServiceBus/Extensions.cs](src/Genocs.Messaging.AzureServiceBus/Extensions.cs) with `AddAzureServiceBus(...)` so hosts can register Azure Service Bus queue/topic capabilities via standard Genocs builder extension flow.
- The new extension binds queue/topic options from configuration, validates required settings for enabled transports, and registers [src/Genocs.Messaging.AzureServiceBus/Queues/Interfaces/IAzureServiceBusQueue.cs](src/Genocs.Messaging.AzureServiceBus/Queues/Interfaces/IAzureServiceBusQueue.cs) and [src/Genocs.Messaging.AzureServiceBus/Topics/Interfaces/IAzureServiceBusTopic.cs](src/Genocs.Messaging.AzureServiceBus/Topics/Interfaces/IAzureServiceBusTopic.cs) without manual internal wiring.
- Added focused registration tests in [src/tests/Genocs.Messaging.UnitTests/AzureServiceBus/AzureServiceBusRegistrationExtensionsTests.cs](src/tests/Genocs.Messaging.UnitTests/AzureServiceBus/AzureServiceBusRegistrationExtensionsTests.cs) and added Azure Service Bus project reference in [src/tests/Genocs.Messaging.UnitTests/Genocs.Messaging.UnitTests.csproj](src/tests/Genocs.Messaging.UnitTests/Genocs.Messaging.UnitTests.csproj).
- Updated package documentation in [src/Genocs.Messaging.AzureServiceBus/README_NUGET.md](src/Genocs.Messaging.AzureServiceBus/README_NUGET.md) and migration guidance in [src/Genocs.Messaging.AzureServiceBus/MIGRATION_GUIDE.md](src/Genocs.Messaging.AzureServiceBus/MIGRATION_GUIDE.md) to use extension-based host registration.
- Validation: `dotnet build src/Genocs.Messaging.AzureServiceBus/Genocs.Messaging.AzureServiceBus.csproj -c Debug --nologo` and `dotnet test src/tests/Genocs.Messaging.UnitTests/Genocs.Messaging.UnitTests.csproj -c Debug --nologo`.

### MESSAGING-010 Remove sync-over-async startup in Azure processors

**Status**: Completed (April 2026)

**Priority**: P1

**Problem**

Queue/topic processors call asynchronous startup with `.GetAwaiter().GetResult()`, increasing deadlock and startup-hang risk in constrained host contexts.

**Likely touch points**

- [src/Genocs.Messaging.AzureServiceBus/Queues/AzureServiceBusQueue.cs](src/Genocs.Messaging.AzureServiceBus/Queues/AzureServiceBusQueue.cs)
- [src/Genocs.Messaging.AzureServiceBus/Topics/AzureServiceBusTopic.cs](src/Genocs.Messaging.AzureServiceBus/Topics/AzureServiceBusTopic.cs)

**Acceptance criteria**

- Processor lifecycle is fully asynchronous and host-managed.
- No sync-over-async startup calls remain.

**Dependencies**

- `MESSAGING-009`

**Implementation notes**

- Removed constructor-time sync-over-async startup from [src/Genocs.Messaging.AzureServiceBus/Queues/AzureServiceBusQueue.cs](src/Genocs.Messaging.AzureServiceBus/Queues/AzureServiceBusQueue.cs) and [src/Genocs.Messaging.AzureServiceBus/Topics/AzureServiceBusTopic.cs](src/Genocs.Messaging.AzureServiceBus/Topics/AzureServiceBusTopic.cs) by moving processor start/stop to `IHostedService.StartAsync(...)` and `IHostedService.StopAsync(...)`.
- Updated [src/Genocs.Messaging.AzureServiceBus/Extensions.cs](src/Genocs.Messaging.AzureServiceBus/Extensions.cs) to register queue/topic transport services as host-managed `IHostedService` instances using shared singleton registrations.
- Extended [src/tests/Genocs.Messaging.UnitTests/AzureServiceBus/AzureServiceBusRegistrationExtensionsTests.cs](src/tests/Genocs.Messaging.UnitTests/AzureServiceBus/AzureServiceBusRegistrationExtensionsTests.cs) to verify hosted-service registrations are present only when Azure queue/topic transport is enabled.
- Updated [src/Genocs.Messaging.AzureServiceBus/README_NUGET.md](src/Genocs.Messaging.AzureServiceBus/README_NUGET.md) and [src/Genocs.Messaging.AzureServiceBus/MIGRATION_GUIDE.md](src/Genocs.Messaging.AzureServiceBus/MIGRATION_GUIDE.md) to document async host-managed processor lifecycle.
- Validation: `dotnet build src/Genocs.Messaging.AzureServiceBus/Genocs.Messaging.AzureServiceBus.csproj -c Debug --nologo`, `dotnet test src/tests/Genocs.Messaging.UnitTests/Genocs.Messaging.UnitTests.csproj -c Debug --nologo`, and grep check confirming no `GetAwaiter().GetResult()` remains in Azure Service Bus package sources.

### MESSAGING-011 Plan migration from legacy handler contracts

**Status**: Completed (April 2026)

**Priority**: P2

**Problem**

Azure Service Bus consumers currently depend on `ICommandHandlerLegacy<T>` and `IEventHandlerLegacy<T>` while core CQRS packages expose modern handler interfaces, increasing ecosystem inconsistency.

**Likely touch points**

- [src/Genocs.Messaging.AzureServiceBus/Queues/Interfaces/IAzureServiceBusQueue.cs](src/Genocs.Messaging.AzureServiceBus/Queues/Interfaces/IAzureServiceBusQueue.cs)
- [src/Genocs.Messaging.AzureServiceBus/Topics/Interfaces/IAzureServiceBusTopic.cs](src/Genocs.Messaging.AzureServiceBus/Topics/Interfaces/IAzureServiceBusTopic.cs)
- [src/Genocs.Messaging.AzureServiceBus/MIGRATION_GUIDE.md](src/Genocs.Messaging.AzureServiceBus/MIGRATION_GUIDE.md)

**Acceptance criteria**

- Contract direction (legacy support, adapter layer, or deprecation timeline) is explicitly documented.
- Migration guidance is available for existing consumers.

**Dependencies**

- `MESSAGING-009`

**Implementation notes**

- Added explicit dual-contract migration direction in Azure Service Bus interfaces by introducing modern registration APIs in [src/Genocs.Messaging.AzureServiceBus/Queues/Interfaces/IAzureServiceBusQueue.cs](src/Genocs.Messaging.AzureServiceBus/Queues/Interfaces/IAzureServiceBusQueue.cs) (`ConsumeModern<T, TH>()`) and [src/Genocs.Messaging.AzureServiceBus/Topics/Interfaces/IAzureServiceBusTopic.cs](src/Genocs.Messaging.AzureServiceBus/Topics/Interfaces/IAzureServiceBusTopic.cs) (`SubscribeModern<T, TH>()), while keeping legacy methods with `[Obsolete]` guidance.
- Implemented adapter-style runtime dispatch in [src/Genocs.Messaging.AzureServiceBus/Queues/AzureServiceBusQueue.cs](src/Genocs.Messaging.AzureServiceBus/Queues/AzureServiceBusQueue.cs) and [src/Genocs.Messaging.AzureServiceBus/Topics/AzureServiceBusTopic.cs](src/Genocs.Messaging.AzureServiceBus/Topics/AzureServiceBusTopic.cs) so both modern (`ICommandHandler<T>`, `IEventHandler<T>`) and legacy (`ICommandHandlerLegacy<T>`, `IEventHandlerLegacy<T>`) handlers execute correctly during transition.
- Extended subscription metadata in [src/Genocs.Messaging.AzureServiceBus/Topics/SubscriptionInfo.cs](src/Genocs.Messaging.AzureServiceBus/Topics/SubscriptionInfo.cs) to record contract kind and event type for deterministic modern/legacy invocation.
- Added migration-contract coverage in [src/tests/Genocs.Messaging.UnitTests/AzureServiceBus/AzureServiceBusHandlerContractMigrationTests.cs](src/tests/Genocs.Messaging.UnitTests/AzureServiceBus/AzureServiceBusHandlerContractMigrationTests.cs) to assert modern registration APIs exist and legacy APIs are marked obsolete.
- Updated migration guidance and package docs in [src/Genocs.Messaging.AzureServiceBus/MIGRATION_GUIDE.md](src/Genocs.Messaging.AzureServiceBus/MIGRATION_GUIDE.md) and [src/Genocs.Messaging.AzureServiceBus/README_NUGET.md](src/Genocs.Messaging.AzureServiceBus/README_NUGET.md) with concrete contract direction and migration examples.
- Validation: `dotnet build src/Genocs.Messaging.AzureServiceBus/Genocs.Messaging.AzureServiceBus.csproj -c Debug --nologo` and `dotnet test src/tests/Genocs.Messaging.UnitTests/Genocs.Messaging.UnitTests.csproj -c Debug --nologo`.

---

## M4: Testability, Docs, and Quality Gates

### MESSAGING-012 Add package-specific tests for core messaging abstractions

**Status**: Completed (April 2026)

**Priority**: P0

**Problem**

There is no dedicated unit test project covering `Genocs.Messaging` abstractions and CQRS bridge behavior.

**Likely touch points**

- [src/tests](src/tests)
- [src/Genocs.Messaging](src/Genocs.Messaging)

**Acceptance criteria**

- New unit tests cover dispatcher behavior, cancellation propagation, and correlation/message-properties accessors.

**Dependencies**

- `MESSAGING-001`
- `MESSAGING-002`

**Implementation notes**

- Added core-accessor coverage in [src/tests/Genocs.Messaging.UnitTests/Core/MessagingAccessorsTests.cs](src/tests/Genocs.Messaging.UnitTests/Core/MessagingAccessorsTests.cs) for [src/Genocs.Messaging/CorrelationContextAccessor.cs](src/Genocs.Messaging/CorrelationContextAccessor.cs) and [src/Genocs.Messaging/MessagePropertiesAccessor.cs](src/Genocs.Messaging/MessagePropertiesAccessor.cs), including set/get semantics, null-clearing behavior, and `AsyncLocal` flow across `await` boundaries.
- Existing CQRS bridge coverage in [src/tests/Genocs.Messaging.UnitTests/CQRS/ServiceBusMessageDispatcherTests.cs](src/tests/Genocs.Messaging.UnitTests/CQRS/ServiceBusMessageDispatcherTests.cs) already validates dispatcher behavior and cancellation-token propagation for command and event dispatch paths, satisfying the remaining acceptance criteria for this task.
- Validation: `dotnet test src/tests/Genocs.Messaging.UnitTests/Genocs.Messaging.UnitTests.csproj -c Debug --nologo`.

### MESSAGING-013 Add RabbitMQ reliability integration tests

**Status**: Completed (April 2026)

**Priority**: P0

**Problem**

Critical runtime paths (retry, ack/nack settlement, dead-letter routing, scoped handler resolution) lack dedicated package-level integration coverage.

**Likely touch points**

- [src/tests](src/tests)
- [src/Genocs.Messaging.RabbitMQ](src/Genocs.Messaging.RabbitMQ)

**Acceptance criteria**

- Integration tests validate successful ack, failure retry flow, dead-letter behavior, and per-message scoped service resolution.

**Dependencies**

- `MESSAGING-004`
- `MESSAGING-005`

**Implementation notes**

- Added reliability integration-style coverage in [src/tests/Genocs.Messaging.RabbitMQ.UnitTests/Internals/RabbitMqBackgroundServiceReliabilityIntegrationTests.cs](src/tests/Genocs.Messaging.RabbitMQ.UnitTests/Internals/RabbitMqBackgroundServiceReliabilityIntegrationTests.cs) to validate critical subscriber runtime paths in [src/Genocs.Messaging.RabbitMQ/Internals/RabbitMqBackgroundService.cs](src/Genocs.Messaging.RabbitMQ/Internals/RabbitMqBackgroundService.cs).
- Added assertions for successful settlement (`BasicAckAsync`) and failure settlement (`BasicNackAsync`) with deterministic behavior under handler exceptions.
- Added retry-flow coverage using configured `Retries`/`RetryInterval` to verify failed handler invocation retries before final negative settlement.
- Added dead-letter flow coverage by enabling dead-letter options and mapping failures to `FailedMessage` with `MoveToDeadLetter = true`, verifying dead-letter path settles through nack.
- Added scoped-resolution coverage to assert per-message scope isolation by resolving unique scoped dependencies for consecutive deliveries.
- Validation: `dotnet test src/tests/Genocs.Messaging.RabbitMQ.UnitTests/Genocs.Messaging.RabbitMQ.UnitTests.csproj -c Debug --nologo`.

### MESSAGING-014 Synchronize docs and enforce package warning baselines

**Status**: Completed (April 2026)

**Priority**: P1

**Problem**

Documentation and runtime contracts can drift, while warning-heavy baselines reduce confidence in package health.

**Likely touch points**

- [docs/Genocs.Messaging-Agent-Documentation.md](docs/Genocs.Messaging-Agent-Documentation.md)
- [docs/Genocs.Messaging.RabbitMQ-Agent-Documentation.md](docs/Genocs.Messaging.RabbitMQ-Agent-Documentation.md)
- [docs/Genocs.Messaging.Outbox-Agent-Documentation.md](docs/Genocs.Messaging.Outbox-Agent-Documentation.md)
- [src/Genocs.Messaging/README_NUGET.md](src/Genocs.Messaging/README_NUGET.md)
- [src/Genocs.Messaging.RabbitMQ/README_NUGET.md](src/Genocs.Messaging.RabbitMQ/README_NUGET.md)
- [src/Genocs.Messaging.Outbox/README_NUGET.md](src/Genocs.Messaging.Outbox/README_NUGET.md)
- [src/Genocs.Messaging.AzureServiceBus/README_NUGET.md](src/Genocs.Messaging.AzureServiceBus/README_NUGET.md)

**Acceptance criteria**

- Messaging package docs align with implemented behavior and ownership boundaries.
- Package-level warning policy is defined and enforced for touched projects.

**Dependencies**

- `MESSAGING-007`
- `MESSAGING-011`

**Implementation notes**

- Synchronized messaging package docs and ownership guidance in [docs/Genocs.Messaging-Agent-Documentation.md](docs/Genocs.Messaging-Agent-Documentation.md), [docs/Genocs.Messaging.RabbitMQ-Agent-Documentation.md](docs/Genocs.Messaging.RabbitMQ-Agent-Documentation.md), and [docs/Genocs.Messaging.Outbox-Agent-Documentation.md](docs/Genocs.Messaging.Outbox-Agent-Documentation.md) with current runtime behavior delivered in MESSAGING-007 through MESSAGING-013.
- Updated package READMEs in [src/Genocs.Messaging/README_NUGET.md](src/Genocs.Messaging/README_NUGET.md), [src/Genocs.Messaging.RabbitMQ/README_NUGET.md](src/Genocs.Messaging.RabbitMQ/README_NUGET.md), [src/Genocs.Messaging.Outbox/README_NUGET.md](src/Genocs.Messaging.Outbox/README_NUGET.md), and [src/Genocs.Messaging.AzureServiceBus/README_NUGET.md](src/Genocs.Messaging.AzureServiceBus/README_NUGET.md) to align startup/contract expectations and warning-policy references.
- Added package-level warning baseline enforcement target in [validate-messaging.mk](validate-messaging.mk) and exposed it via [Makefile](Makefile) as `make validate-messaging`.
- The warning policy gate enforces warning-free `net10.0` builds (`-warnaserror`) for messaging packages and runs messaging regression tests to prevent behavior drift.
- Validation: `make validate-messaging`.

## Suggested First Execution Slice

1. `MESSAGING-001`
2. `MESSAGING-004`
3. `MESSAGING-005`
4. `MESSAGING-006`
5. `MESSAGING-012`

This slice addresses the highest runtime-risk items first while enabling immediate regression coverage.
