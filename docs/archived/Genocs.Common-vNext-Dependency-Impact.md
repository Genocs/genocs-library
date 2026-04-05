# Genocs.Common vNext Dependency Impact

## Purpose

This document tracks how Genocs.Common vNext changes affect packages and projects in this solution.

Primary objective:
- evolve common contracts aggressively where architecture benefits are clear
- track and coordinate downstream migration across packages and apps
- keep breaking-change documentation explicit and reviewable

## Golden Rule

Backward compatibility is not required for this vNext stream.

Allowed and encouraged when useful:
- remove obsolete or ambiguous contracts
- split interfaces where boundaries are unclear
- rename contracts for consistency and correctness

Required for every intentional break:
- document the change and rationale in this file
- list impacted projects and migration owner
- include validation evidence (build/tests) for migrated projects

Related definitions:
- [docs/Genocs.Common-vNext-Owner-Definitions.md](docs/Genocs.Common-vNext-Owner-Definitions.md)
- [docs/Genocs.Common-vNext-Migration-State-Definitions.md](docs/Genocs.Common-vNext-Migration-State-Definitions.md)

## Dependency Baseline

Source of truth for this section:
- csproj project references under src
- last reviewed: 2026-04-05

### Common Outbound Dependency

Genocs.Common currently has no outbound ProjectReference dependency on other in-solution projects.

### Direct Inbound Dependents (ProjectReference to Genocs.Common)

Package and project dependents:
- Genocs.Core
- demo/Contracts

Test project:
- tests/Genocs.Common.UnitTests

### High-Impact Transitive Dependents

Because Genocs.Core depends directly on Genocs.Common, all direct Genocs.Core dependents are transitive dependents of Genocs.Common and expected to be impacted by major contract changes.

High-impact transitive package dependents (via Genocs.Core):
- Genocs.Http
- Genocs.Logging
- Genocs.Messaging
- Genocs.Messaging.AzureServiceBus
- Genocs.Metrics
- Genocs.Persistence.EFCore
- Genocs.Persistence.MongoDB
- Genocs.Persistence.Redis
- Genocs.Saga.Integrations.MongoDB
- Genocs.Saga.Integrations.Redis
- Genocs.Secrets.AzureKeyVault
- Genocs.Secrets.HashicorpKeyVault
- Genocs.Security
- Genocs.Telemetry
- Genocs.WebApi
- Genocs.WebApi.Security

Additional transitive dependents through package layering:
- Genocs.WebApi.OpenApi (via Genocs.WebApi)
- Genocs.WebApi.CQRS (via Genocs.WebApi)
- Genocs.ServiceDiscovery.Consul (via Genocs.Http)
- Genocs.LoadBalancing.Fabio (via Genocs.ServiceDiscovery.Consul)
- Genocs.Messaging.Outbox (via Genocs.Messaging)
- Genocs.Messaging.Outbox.MongoDB (via Genocs.Messaging.Outbox and Genocs.Persistence.MongoDB)
- Genocs.Messaging.RabbitMQ (via Genocs.Messaging)
- Genocs.Tracing.Jaeger.RabbitMQ (via Genocs.Messaging.RabbitMQ)

Host-level transitive impact expected on:
- src/apps/apigateway/WebApi/Host.csproj
- src/apps/identities/Application/Application.csproj
- src/apps/identities/WebApi/Host.csproj
- src/apps/products/WebApi/Host.csproj
- src/apps/orders/WebApi/Host.csproj
- src/apps/notifications/WebApi/Host.csproj
- src/demo/WebApi/Host.csproj
- src/demo/Masstransit.WebApi/Host.csproj
- src/demo/Masstransit.Worker/Host.csproj

## vNext Boundary Decisions (Working Set)

### Keep in Genocs.Common

- core domain and CQRS contracts intended for broad package reuse
- dependency marker abstractions used uniformly across the stack
- transport-agnostic primitives and value contracts

### Candidate to Remove or Reshape

- contracts with unclear ownership that leak infrastructure concerns
- overlapping contracts where one canonical contract should remain
- obsolete marker interfaces and aliases retained only for compatibility

### Non-Goals

- preserving compatibility-only shims for removed contracts
- carrying duplicate interfaces with near-identical semantics
- deferring necessary cleanup because of downstream churn

## Breaking Change Ledger

Use this ledger as the source of truth for Genocs.Common vNext migration planning.

| Change ID | Common Area | Change Type | Intentional Break | Directly Impacted Projects | Owner | Status | Notes |
|---|---|---|---|---|---|---|---|
| COMMON-011 | Repository contracts | Interface split | Yes | Genocs.Core, Genocs.Persistence.MongoDB, downstream repositories | COMMON + PERSIST | Completed | IQueryable moved to opt-in contract |
| COMMON-027 | Outbox contracts | New contract foundation | No | Genocs.Messaging.Outbox, Genocs.Messaging.Outbox.MongoDB, Genocs.Core | COMMON + MSG | Completed | Added `ITransactionalEvent`, `IOutboxMessage`, and `IOutboxDispatcher` |
| COMMON-028 | Specification contracts | New contract foundation | No | Genocs.Core, Genocs.Persistence.EFCore, Genocs.Persistence.MongoDB, Genocs.Persistence.Redis | COMMON + PERSIST | Completed | Added `ISpecification<TEntity>`, `IProjectionSpecification<TEntity, TResult>`, and `ISpecificationRepository<TEntity, TKey>` |
| COMMON-029 | Cursor paging contracts | New contract foundation | No | Genocs.Core, Genocs.WebApi, Genocs.WebApi.CQRS | COMMON + WEBAPI | Completed | Added `ICursorQuery`, `CursorQueryBase`, and `CursorPagedResult<T>` |
| COMMON-030 | Validation result contracts | New contract foundation | No | Genocs.Core, Genocs.WebApi.CQRS, Genocs.WebApi, host pipelines | COMMON + WEBAPI | Completed | Added `ValidationError`, `ValidationResult`, and minimal `IValidator<T>` contract |
| COMMON-031 | Optimistic concurrency contracts | New contract foundation | No | Genocs.Core, Genocs.Persistence.EFCore, Genocs.Persistence.MongoDB, Genocs.Persistence.Redis | COMMON + PERSIST | Completed | Added minimal `IVersioned` contract with `long Version` |
| COMMON-032 | Soft-delete query filter contracts | New contract foundation | No | Genocs.Core, Genocs.Persistence.EFCore, Genocs.Persistence.MongoDB, Genocs.WebApi.CQRS | COMMON + PERSIST + WEBAPI | Completed | Added `ISoftDeleteFilter` and `SoftDeleteFilterBase` |
| COMMON-vNext-001 | Dependency markers | Remove obsolete marker aliases | Yes | Genocs.Core, Genocs.WebApi, package adapters | COMMON + CORE | Planned | Keep only canonical marker abstractions |
| COMMON-vNext-002 | CQRS contracts | Consolidate overlapping abstractions | Yes | Genocs.Core, Genocs.WebApi.CQRS, Genocs.Messaging | COMMON + CORE + MSG | Planned | Reduce duplicate registration/dispatch shapes |
| COMMON-vNext-003 | Repository interfaces | Remove provider-leaking members from baseline contracts | Yes | Genocs.Core, Genocs.Persistence.EFCore, Genocs.Persistence.MongoDB, Genocs.Persistence.Redis | COMMON + PERSIST | Planned | Keep baseline contracts provider-agnostic |
| COMMON-vNext-004 | Error and response model | Normalize error contracts and payload shape | Yes | Genocs.WebApi, Genocs.WebApi.Security, host applications | COMMON + WEBAPI | Planned | Improve consistency and diagnostics |

## Per-Project Migration Tracker (Direct Dependents)

Owner and state values in this table use the definition files listed above.

| Project | Category | Primary Owner | Expected Impact | Migration State | Validation Command |
|---|---|---|---|---|---|
| Genocs.Core | Direct dependent | CORE | High | QUEUED-W1 | dotnet build src/Genocs.Core/Genocs.Core.csproj -c Debug --nologo |
| demo/Contracts | Direct dependent | APPS | Medium | QUEUED-W1 | dotnet build src/demo/Contracts/Contracts.csproj -c Debug --nologo |
| tests/Genocs.Common.UnitTests | Test dependent | COMMON | Medium | QUEUED-W1 | dotnet test src/tests/Genocs.Common.UnitTests/Genocs.Common.UnitTests.csproj -c Debug --nologo |

## Per-Project Migration Tracker (Transitive Package Dependents)

| Project | Category | Primary Owner | Expected Impact | Migration State | Validation Command |
|---|---|---|---|---|---|
| Genocs.Http | Transitive package dependent | CORE | Medium | QUEUED-W2 | dotnet build src/Genocs.Http/Genocs.Http.csproj -c Debug --nologo |
| Genocs.Logging | Transitive package dependent | COMMON | Medium | QUEUED-W2 | dotnet build src/Genocs.Logging/Genocs.Logging.csproj -c Debug --nologo |
| Genocs.Messaging | Transitive package dependent | MSG | High | QUEUED-W2 | dotnet build src/Genocs.Messaging/Genocs.Messaging.csproj -c Debug --nologo |
| Genocs.Messaging.AzureServiceBus | Transitive package dependent | MSG | High | QUEUED-W2 | dotnet build src/Genocs.Messaging.AzureServiceBus/Genocs.Messaging.AzureServiceBus.csproj -c Debug --nologo |
| Genocs.Metrics | Transitive package dependent | COMMON | Medium | QUEUED-W2 | dotnet build src/Genocs.Metrics/Genocs.Metrics.csproj -c Debug --nologo |
| Genocs.Persistence.EFCore | Transitive package dependent | PERSIST | High | QUEUED-W2 | dotnet build src/Genocs.Persistence.EFCore/Genocs.Persistence.EFCore.csproj -c Debug --nologo |
| Genocs.Persistence.MongoDB | Transitive package dependent | PERSIST | High | QUEUED-W2 | dotnet build src/Genocs.Persistence.MongoDB/Genocs.Persistence.MongoDB.csproj -c Debug --nologo |
| Genocs.Persistence.Redis | Transitive package dependent | PERSIST | High | QUEUED-W2 | dotnet build src/Genocs.Persistence.Redis/Genocs.Persistence.Redis.csproj -c Debug --nologo |
| Genocs.Saga.Integrations.MongoDB | Transitive package dependent | PERSIST | Medium | QUEUED-W2 | dotnet build src/Genocs.Saga.Integrations.MongoDB/Genocs.Saga.Integrations.MongoDB.csproj -c Debug --nologo |
| Genocs.Saga.Integrations.Redis | Transitive package dependent | PERSIST | Medium | QUEUED-W2 | dotnet build src/Genocs.Saga.Integrations.Redis/Genocs.Saga.Integrations.Redis.csproj -c Debug --nologo |
| Genocs.Secrets.AzureKeyVault | Transitive package dependent | WEBAPI | Medium | QUEUED-W2 | dotnet build src/Genocs.Secrets.AzureKeyVault/Genocs.Secrets.AzureKeyVault.csproj -c Debug --nologo |
| Genocs.Secrets.HashicorpKeyVault | Transitive package dependent | WEBAPI | Medium | QUEUED-W2 | dotnet build src/Genocs.Secrets.HashicorpKeyVault/Genocs.Secrets.HashicorpKeyVault.csproj -c Debug --nologo |
| Genocs.Security | Transitive package dependent | WEBAPI | Medium | QUEUED-W2 | dotnet build src/Genocs.Security/Genocs.Security.csproj -c Debug --nologo |
| Genocs.Telemetry | Transitive package dependent | COMMON | Medium | QUEUED-W2 | dotnet build src/Genocs.Telemetry/Genocs.Telemetry.csproj -c Debug --nologo |
| Genocs.WebApi | Transitive package dependent | WEBAPI | High | QUEUED-W2 | dotnet build src/Genocs.WebApi/Genocs.WebApi.csproj -c Debug --nologo |
| Genocs.WebApi.Security | Transitive package dependent | WEBAPI | High | QUEUED-W2 | dotnet build src/Genocs.WebApi.Security/Genocs.WebApi.Security.csproj -c Debug --nologo |

## Documentation Rules for Breaking Changes

For every Genocs.Common vNext PR:
- add or update a row in Breaking Change Ledger
- update impacted projects in direct/transitive migration trackers
- include migration notes and rationale in the PR description
- update CHANGELOG with explicit breaking-change callouts

No exception policy:
- if consumer code must change, treat as a breaking change and document it

## Recommended Execution Order

1. Finalize contract boundary decisions and update the Breaking Change Ledger.
2. Migrate direct dependents first (Genocs.Core, demo/Contracts, Common tests).
3. Migrate transitive package dependents in waves after direct dependents are green.
4. Migrate app/demo hosts after package layer validation.
5. Run solution-level validation and move rows to VERIFIED/CLOSED.
