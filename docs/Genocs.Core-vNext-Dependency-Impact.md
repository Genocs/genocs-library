# Genocs.Core vNext Dependency Impact

## Purpose

This document tracks how Genocs.Core vNext changes impact other projects in this solution.

Primary objective:
- make Core cleaner and easier to evolve
- allow intentional breaking changes where they improve architecture
- track all downstream impact in one place

## Golden Rule

Backward compatibility is not required for this vNext stream.

Allowed and encouraged when useful:
- remove legacy APIs instead of keeping compatibility shims
- rename or split contracts for clearer architecture
- move features from Core to companion packages when ownership is clearer

Required for every intentional break:
- document the change and rationale in this file
- list impacted projects and migration owner
- add/update tests proving the new expected behavior

Related definitions:
- [docs/Genocs.Core-vNext-Owner-Definitions.md](docs/Genocs.Core-vNext-Owner-Definitions.md)
- [docs/Genocs.Core-vNext-Migration-State-Definitions.md](docs/Genocs.Core-vNext-Migration-State-Definitions.md)

## Core Dependency Baseline

Source of truth for this section:
- csproj ProjectReference links in src
- last reviewed: 2026-04-05

### Core Outbound Dependency

Genocs.Core currently depends on:
- Genocs.Common (Debug via project reference, Release via package reference)

### Direct Inbound Dependents (ProjectReference to Genocs.Core)

Library packages:
- Genocs.Http
- Genocs.Logging
- Genocs.Messaging
- Genocs.Messaging.AzureServiceBus
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

Test project:
- Genocs.Core.UnitTests

### High-Impact Transitive Dependents

Projects that do not directly reference Genocs.Core but are expected to be affected through direct dependents:
- Genocs.WebApi.OpenApi (via Genocs.WebApi)
- Genocs.WebApi.CQRS (via Genocs.WebApi)
- Genocs.ServiceDiscovery.Consul (via Genocs.Http)
- Genocs.LoadBalancing.Fabio (via Genocs.ServiceDiscovery.Consul)
- Genocs.Messaging.Outbox (via Genocs.Messaging)
- Genocs.Messaging.Outbox.MongoDB (via Genocs.Messaging.Outbox and Genocs.Persistence.MongoDB)
- Genocs.Messaging.RabbitMQ (via Genocs.Messaging)
- Genocs.Tracing.Jaeger.RabbitMQ (via Genocs.Messaging.RabbitMQ)

Application and demo hosts expected to be impacted transitively:
- src/demo/apigateway/WebApi/Host.csproj
- src/demo/identities/Application/Application.csproj
- src/demo/identities/WebApi/Host.csproj
- src/demo/products/WebApi/Host.csproj
- src/demo/orders/WebApi/Host.csproj
- src/demo/notifications/WebApi/Host.csproj
- src/demo/WebApi/Host.csproj
- src/demo/Masstransit.WebApi/Host.csproj
- src/demo/Masstransit.Worker/Host.csproj

## vNext Boundary Decisions (Working Set)

### Keep in Core

- foundational abstractions required by most packages
- startup bootstrap primitives and deterministic initializer flow
- CQRS dispatcher abstractions and default in-memory dispatch implementations
- domain entity base types and cross-cutting conventions

### Candidate to Extract or Remove from Core

- convenience APIs that duplicate behavior available in higher-level packages
- infrastructure-specific behavior that leaks persistence/messaging concerns into Core
- overlapping helper APIs that have clearer ownership in WebApi, Messaging, or Persistence packages

### Non-Goals for vNext

- preserving obsolete contracts solely for compatibility
- keeping sync wrappers that hide async-first behavior
- carrying dead or commented runtime code paths

## Breaking Change Ledger

Use this as the source of truth for package migration work.

| Change ID | Core Area | Change Type | Intentional Break | Directly Impacted Projects | Owner | Status | Notes |
|---|---|---|---|---|---|---|---|
| CORE-011 | Repository contracts | Interface split | Yes | Genocs.Persistence.MongoDB, Genocs.Core, downstream repositories | Core + Persistence | Completed | IQueryable surface moved to opt-in contract |
| CORE-vNext-001 | Startup API surface | Remove sync-first startup entry points where redundant | Yes | Genocs.WebApi, Genocs.WebApi.Security, host projects | Core + WebApi | Planned | Prefer explicit async initialization only |
| CORE-vNext-002 | CQRS registration APIs | Consolidate registration surface and remove duplicates | Yes | Genocs.WebApi.CQRS, Genocs.Messaging, host projects | Core + WebApi.CQRS | Planned | Keep one canonical registration pattern |
| CORE-vNext-003 | Domain base abstractions | Remove or relocate infrastructure-leaking members | Yes | Genocs.Persistence.EFCore, Genocs.Persistence.MongoDB, Genocs.Persistence.Redis | Core + Persistence | Planned | Keep domain contracts provider-agnostic |
| CORE-vNext-004 | Core package boundaries | Extract non-core utilities to owning packages | Yes | Genocs.Logging, Genocs.Telemetry, Genocs.Security, Genocs.Http | Core + Package Owners | Planned | Reduce cross-package coupling |

## Per-Project Migration Tracker

Track each direct dependent until green on vNext branch.

Owner and state values in this table use the definition files listed above.

| Project | Category | Primary Owner | Expected Impact | Migration State | Validation Command |
|---|---|---|---|---|---|
| Genocs.Http | Direct dependent | CORE | Medium | QUEUED-W1 | dotnet build src/Genocs.Http/Genocs.Http.csproj -c Debug --nologo |
| Genocs.Logging | Direct dependent | OBS | Medium | QUEUED-W1 | dotnet build src/Genocs.Logging/Genocs.Logging.csproj -c Debug --nologo |
| Genocs.Messaging | Direct dependent | MSG | High | QUEUED-W1 | dotnet build src/Genocs.Messaging/Genocs.Messaging.csproj -c Debug --nologo |
| Genocs.Messaging.AzureServiceBus | Direct dependent | MSG | High | QUEUED-W1 | dotnet build src/Genocs.Messaging.AzureServiceBus/Genocs.Messaging.AzureServiceBus.csproj -c Debug --nologo |
| Genocs.Persistence.EFCore | Direct dependent | PERSIST | High | QUEUED-W1 | dotnet build src/Genocs.Persistence.EFCore/Genocs.Persistence.EFCore.csproj -c Debug --nologo |
| Genocs.Persistence.MongoDB | Direct dependent | PERSIST | High | QUEUED-W1 | dotnet build src/Genocs.Persistence.MongoDB/Genocs.Persistence.MongoDB.csproj -c Debug --nologo |
| Genocs.Persistence.Redis | Direct dependent | PERSIST | High | QUEUED-W1 | dotnet build src/Genocs.Persistence.Redis/Genocs.Persistence.Redis.csproj -c Debug --nologo |
| Genocs.Saga.Integrations.MongoDB | Direct dependent | PERSIST | Medium | QUEUED-W1 | dotnet build src/Genocs.Saga.Integrations.MongoDB/Genocs.Saga.Integrations.MongoDB.csproj -c Debug --nologo |
| Genocs.Saga.Integrations.Redis | Direct dependent | PERSIST | Medium | QUEUED-W1 | dotnet build src/Genocs.Saga.Integrations.Redis/Genocs.Saga.Integrations.Redis.csproj -c Debug --nologo |
| Genocs.Secrets.AzureKeyVault | Direct dependent | SEC | Medium | QUEUED-W1 | dotnet build src/Genocs.Secrets.AzureKeyVault/Genocs.Secrets.AzureKeyVault.csproj -c Debug --nologo |
| Genocs.Secrets.HashicorpKeyVault | Direct dependent | SEC | Medium | QUEUED-W1 | dotnet build src/Genocs.Secrets.HashicorpKeyVault/Genocs.Secrets.HashicorpKeyVault.csproj -c Debug --nologo |
| Genocs.Security | Direct dependent | SEC | Medium | QUEUED-W1 | dotnet build src/Genocs.Security/Genocs.Security.csproj -c Debug --nologo |
| Genocs.Telemetry | Direct dependent | OBS | Medium | QUEUED-W1 | dotnet build src/Genocs.Telemetry/Genocs.Telemetry.csproj -c Debug --nologo |
| Genocs.WebApi | Direct dependent | WEBAPI | High | QUEUED-W1 | dotnet build src/Genocs.WebApi/Genocs.WebApi.csproj -c Debug --nologo |
| Genocs.WebApi.Security | Direct dependent | WEBAPI | High | QUEUED-W1 | dotnet build src/Genocs.WebApi.Security/Genocs.WebApi.Security.csproj -c Debug --nologo |

## Change Documentation Rules

For every Core vNext PR:
- add or update a row in Breaking Change Ledger
- update impacted projects in Per-Project Migration Tracker
- include concrete migration notes in PR description
- update CHANGELOG with explicit breaking-change callouts

No exception policy:
- if a behavior changes and consumers must update code, treat it as a breaking change and document it

## Recommended Execution Order

1. Land boundary decisions and breaking-change ledger updates first.
2. Migrate direct dependents package-by-package in this order:
   - Persistence packages
   - Messaging packages
   - WebApi packages
   - Security, Telemetry, Logging, Http
3. Migrate app/demo hosts after package layer is green.
4. Run full solution test/build validation and update this file to Completed state.
