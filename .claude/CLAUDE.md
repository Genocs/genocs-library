# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Genocs Library is a set of open-source .NET 10 packages for building cloud-agnostic, production-ready microservices. The repo contains the library source packages, a demo application (BookStore), and a full enterprise application with multiple services.

## Build & Test Commands

```bash
# Build entire solution
dotnet build

# Pack all NuGet packages
dotnet pack

# Pack a specific project with nuspec
cd src/Genocs.Core
dotnet pack -p:NuspecFile=./Genocs.Core.nuspec --no-restore -o .

# Run all tests
make test

# Run by category
make test-unit
make test-integration
make test-e2e

# Run a specific test project directly
dotnet test src/tests/Genocs.Core.UnitTests

# Run a single test by filter
dotnet test src/tests/Genocs.Core.UnitTests --filter "FullyQualifiedName~EncryptUnitTests"

# Run the demo WebApi
dotnet run --project ./src/demo/WebApi/Host.csproj -c Debug

# Run the demo worker
dotnet run --project ./src/demo/Worker
```

## EF Core Migrations (BookStore demo)

```bash
# Restore local tools first (pins dotnet-ef to 10.0.3)
dotnet tool restore

# Add a migration
dotnet dotnet-ef migrations add <MigrationName> \
  --project src/demo/WebApi/Host.csproj \
  --startup-project src/demo/WebApi/Host.csproj \
  --context BookStoreDbContext \
  --output-dir BookStore/Migrations

# Apply migrations
dotnet dotnet-ef database update \
  --project src/demo/WebApi/Host.csproj \
  --startup-project src/demo/WebApi/Host.csproj \
  --context BookStoreDbContext
```

## Infrastructure (Docker)

```bash
cd infrastructure/docker

# Core infrastructure: RabbitMQ + MongoDB
docker compose -f ./infrastructure.yml --env-file ./.env --project-name genocs up -d

# Database extras: Redis + PostgreSQL
docker compose -f ./infrastructure-db.yml --env-file ./.env --project-name genocs up -d

# Monitoring: Prometheus, Grafana, InfluxDB, Jaeger, Seq
docker compose -f ./infrastructure-monitoring.yml --env-file ./.env --project-name genocs up -d
```

Default local endpoints: RabbitMQ `localhost:15672`, MongoDB `localhost:27017`, Redis `localhost:6379`, Postgres `localhost:5432`.

## Architecture

### Library Packages (`src/`)

Each folder under `src/` is an independent NuGet package. Key packages:

| Package | Purpose |
|---|---|
| `Genocs.Core` | Foundation: `IGenocsBuilder`, CQRS abstractions, domain primitives, DI registration |
| `Genocs.Common` | Shared types and utilities |
| `Genocs.WebApi` | Minimal API / endpoint builder pattern (`IEndpointsBuilder`) |
| `Genocs.WebApi.OpenApi` | OpenAPI/Swagger integration |
| `Genocs.WebApi.CQRS` | CQRS wiring for WebApi endpoints |
| `Genocs.Auth` / `Genocs.WebApi.Security` | JWT, certificate-based auth |
| `Genocs.Logging` | Serilog setup with ELK/Seq/file sinks |
| `Genocs.Telemetry` | OpenTelemetry integration |
| `Genocs.Messaging` | `IBusPublisher` / `IBusSubscriber` abstractions |
| `Genocs.Messaging.RabbitMQ` | RabbitMQ implementation |
| `Genocs.Messaging.AzureServiceBus` | Azure Service Bus implementation |
| `Genocs.Messaging.Outbox` | Outbox pattern abstraction |
| `Genocs.Persistence.MongoDB` | MongoDB repository pattern |
| `Genocs.Persistence.EFCore` | EF Core repository pattern |
| `Genocs.Persistence.Redis` | Redis cache integration |
| `Genocs.Saga` | Saga/state machine orchestration |
| `Genocs.Discovery.Consul` | Consul service discovery |
| `Genocs.LoadBalancing.Fabio` | Fabio load balancer integration |
| `Genocs.Secrets.*` | Azure Key Vault & HashiCorp Vault |
| `Genocs.Tracing.Jaeger.RabbitMQ` | Distributed tracing via Jaeger |

### Fluent Builder Pattern

All library modules register via `IGenocsBuilder` extension methods. The standard setup in a host `Program.cs`:

```csharp
IGenocsBuilder gnxBuilder = builder
    .AddGenocs()          // core
    .AddTelemetry()
    .AddJwt("simmetric_jwt")
    .AddWebApi()
    .AddOpenApiDocs();

gnxBuilder.Build();       // must be called before builder.Build()
```

Middleware is then added via `app.UseGenocs()` and module-specific `Use*` extension methods.

### Demo Application (`src/demo/`)

- **WebApi** – ASP.NET Core minimal API host with BookStore (EF Core + SQL Server), Saga, JWT, OpenAPI. Connection string key: `ConnectionStrings:BookStore`.
- **Worker** – Background service using the library.
- **HelloWorld.WebApi** – Minimal example.
- **Masstransit.WebApi / Masstransit.Worker** – MassTransit messaging demo.

### Enterprise Application (`src/apps/`)

Full microservices suite with ApiGateway (:5500), Identity (:5510), Product (:5520), Order (:5530), Notification (:5540) services. All share the `genocs-network` Docker network.

### Tests (`src/tests/`)

- Unit tests use **xUnit** + **Moq** / **NSubstitute** + **FluentAssertions** / **Shouldly**.
- Integration tests use **Testcontainers** for real infrastructure containers.
- E2E tests use **Reqnroll** (BDD/Gherkin).

Use `[assembly: InternalsVisibleTo("...")]` or the `<AssemblyAttribute>` csproj entry to expose internal members to test projects.

## Coding Standards

- **C# conventions**: nullable enabled, implicit usings, `LangVersion=latest`, StyleCop + Roslynator analyzers enforced globally via `Directory.Build.props`.
- **Comments**: explain *why*, not *what*. Business context and architectural decisions only.
- **DI**: Microsoft.Extensions.DependencyInjection throughout; no third-party containers.
- **Validation**: FluentValidation.
- **Default ID type**: `System.Guid` is aliased globally as `DefaultIdType` (set in `Directory.Build.props`).
- **Version**: set globally in `Directory.Build.props` (`<Version>`). Do not set per-project.

## Configuration

Services are configured via `appsettings.json` sections: `app`, `consul`, `fabio`, `httpClient`, `logger`, `jwt`, `prometheus`, `mongodb`, `outbox`, `rabbitmq`, `redis`, `openapi`, `security`, `azureKeyVault`, `vault`. See `README.md` for the full reference schema.
