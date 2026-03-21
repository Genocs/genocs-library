# Genocs.Persistence.MongoDB Agent Reference

## Purpose

This document is optimized for AI-assisted development sessions.
It prioritizes fast retrieval of:

- What Genocs.Persistence.MongoDB is responsible for
- Which APIs to call for specific goals
- Where source of truth lives
- What constraints and runtime behaviors matter

## Quick Facts

| Key | Value |
|---|---|
| Package | Genocs.Persistence.MongoDB |
| Project file | [src/Genocs.Persistence.MongoDB/Genocs.Persistence.MongoDB.csproj](src/Genocs.Persistence.MongoDB/Genocs.Persistence.MongoDB.csproj) |
| Target frameworks | net10.0, net9.0, net8.0 |
| Primary role | MongoDB persistence integration with repository pattern support for Genocs applications |
| Core themes | AddMongo and AddMongoWithRegistration extensions, repository abstractions and implementations, database/session providers, convention registration, paging utilities, optional seeding |

## Use This Package When

- Registering MongoDB connectivity in a Genocs service.
- Using generic repository abstractions backed by MongoDB collections.
- Enabling convention-based BSON serialization defaults.
- Running one-time database seeding during initialization.
- Building paged query flows over Mongo IQueryable sources.

## Do Not Assume

- AddMongo returns early and registers nothing when the configured section does not exist.
- Mongo registration is skipped when MongoOptions validation fails.
- AddMongoWithRegistration only auto-registers IMongoRepository<> and does not scan custom repository types.

## High-Value Entry Points

### Service registration extensions

- AddMongo in [src/Genocs.Persistence.MongoDB/Extensions/MongoExtensions.cs](src/Genocs.Persistence.MongoDB/Extensions/MongoExtensions.cs)
- AddMongoWithRegistration in [src/Genocs.Persistence.MongoDB/Extensions/MongoExtensions.cs](src/Genocs.Persistence.MongoDB/Extensions/MongoExtensions.cs)
- RegisterMongoRepositories in [src/Genocs.Persistence.MongoDB/Extensions/MongoExtensions.cs](src/Genocs.Persistence.MongoDB/Extensions/MongoExtensions.cs)

### Core configuration contracts

- MongoOptions in [src/Genocs.Persistence.MongoDB/Configurations/MongoOptions.cs](src/Genocs.Persistence.MongoDB/Configurations/MongoOptions.cs)
- IMongoOptionsBuilder in [src/Genocs.Persistence.MongoDB/Configurations/IMongoOptionsBuilder.cs](src/Genocs.Persistence.MongoDB/Configurations/IMongoOptionsBuilder.cs)
- MongoOptionsBuilder in [src/Genocs.Persistence.MongoDB/Builders/MongoOptionsBuilder.cs](src/Genocs.Persistence.MongoDB/Builders/MongoOptionsBuilder.cs)
- MongoEncryptionOptions in [src/Genocs.Persistence.MongoDB/Configurations/MongoEncryptionOptions.cs](src/Genocs.Persistence.MongoDB/Configurations/MongoEncryptionOptions.cs)

### Repository abstractions and implementations

- IMongoBaseRepository<TEntity, TKey> in [src/Genocs.Persistence.MongoDB/Domain/Repositories/IMongoBaseRepository.cs](src/Genocs.Persistence.MongoDB/Domain/Repositories/IMongoBaseRepository.cs)
- IMongoRepository<TEntity> in [src/Genocs.Persistence.MongoDB/Domain/Repositories/IMongoRepository.cs](src/Genocs.Persistence.MongoDB/Domain/Repositories/IMongoRepository.cs)
- MongoBaseRepository<TEntity, TKey> in [src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoBaseRepository.cs](src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoBaseRepository.cs)
- MongoRepository<TEntity> in [src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoRepository.cs](src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoRepository.cs)

### Database access and session lifecycle

- IMongoDatabaseProvider in [src/Genocs.Persistence.MongoDB/IMongoDatabaseProvider.cs](src/Genocs.Persistence.MongoDB/IMongoDatabaseProvider.cs)
- MongoDatabaseProvider in [src/Genocs.Persistence.MongoDB/MongoDatabaseProvider.cs](src/Genocs.Persistence.MongoDB/MongoDatabaseProvider.cs)
- IMongoSessionFactory in [src/Genocs.Persistence.MongoDB/Repositories/IMongoSessionFactory.cs](src/Genocs.Persistence.MongoDB/Repositories/IMongoSessionFactory.cs)
- MongoSessionFactory in [src/Genocs.Persistence.MongoDB/Factories/MongoSessionFactory.cs](src/Genocs.Persistence.MongoDB/Factories/MongoSessionFactory.cs)

### Initialization and conventions

- IMongoInitializer in [src/Genocs.Persistence.MongoDB/Repositories/IMongoInitializer.cs](src/Genocs.Persistence.MongoDB/Repositories/IMongoInitializer.cs)
- MongoInitializer in [src/Genocs.Persistence.MongoDB/Initializers/MongoDbInitializer.cs](src/Genocs.Persistence.MongoDB/Initializers/MongoDbInitializer.cs)
- IMongoSeeder in [src/Genocs.Persistence.MongoDB/Repositories/IMongoSeeder.cs](src/Genocs.Persistence.MongoDB/Repositories/IMongoSeeder.cs)
- MongoSeeder in [src/Genocs.Persistence.MongoDB/Seeders/MongoSeeder.cs](src/Genocs.Persistence.MongoDB/Seeders/MongoSeeder.cs)

### Query pagination helpers

- PaginateAsync overloads in [src/Genocs.Persistence.MongoDB/Repositories/Pagination.cs](src/Genocs.Persistence.MongoDB/Repositories/Pagination.cs)
- Limit overloads in [src/Genocs.Persistence.MongoDB/Repositories/Pagination.cs](src/Genocs.Persistence.MongoDB/Repositories/Pagination.cs)
- ToLambda property resolver in [src/Genocs.Persistence.MongoDB/Repositories/Pagination.cs](src/Genocs.Persistence.MongoDB/Repositories/Pagination.cs)

## Decision Matrix For Agents

| Goal | Preferred API | Notes |
|---|---|---|
| Register Mongo support from configuration | AddMongo(builder, sectionName) | Uses mongoDb section by default and validates options before DI setup |
| Register Mongo support from explicit values | AddMongo(builder, MongoOptions options) | Useful for tests or dynamic runtime options |
| Enable default repository registration | AddMongoWithRegistration | Adds scoped IMongoRepository<> to MongoRepository<> |
| Register custom repository implementations | RegisterMongoRepositories(assembly, lifetime) | Scans dependencies for classes assignable to IMongoRepository<> |
| Add repository by explicit collection name | AddMongoRepository<TEntity, TKey>(collectionName) | Registers IMongoBaseRepository<TEntity, TKey> with provided collection |
| Get database and client from DI | IMongoDatabaseProvider | Provides both IMongoClient and IMongoDatabase |
| Create Mongo session for transactions | IMongoSessionFactory.CreateAsync | Starts a client session handle via MongoClient |
| Execute paged queries | Pagination.PaginateAsync | Applies page/result defaults and optional orderBy/sortOrder |

## Minimal Integration Recipe

### Install

```bash
dotnet add package Genocs.Persistence.MongoDB
```

### Setup in Program.cs

```csharp
using Genocs.Core.Builders;
using Genocs.Persistence.MongoDB.Extensions;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder gnxBuilder = builder.AddGenocs();

gnxBuilder.AddMongoWithRegistration();

gnxBuilder.Build();

var app = builder.Build();

app.Run();
```

## Behavior Notes That Affect Agent Decisions

- Conventions are registered once per process through the static _conventionsRegistered flag.
- AddMongo may append a GUID suffix to the database name when SetRandomDatabaseSuffix is true.
- IMongoInitializer initialization runs once due to static interlocked guard and optionally triggers seeding.
- EnableTracing configures DiagnosticsActivityEventSubscriber on MongoClientSettings.
- Pagination defaults page and resultsPerPage to 1 and 10 when invalid values are provided.
- Pagination.PaginateAsync returns PagedResult<T>.Empty when the query source has no records.

## Source-Accurate Capability Map

### Mongo service wiring

- Binds mongoDb configuration into MongoOptions.
- Validates required options before registering Mongo client/database services.
- Registers seeder, initializer, session factory, and database provider abstractions.
- Supports optional conventions and repository registration extension paths.

Files:

- [src/Genocs.Persistence.MongoDB/Extensions/MongoExtensions.cs](src/Genocs.Persistence.MongoDB/Extensions/MongoExtensions.cs)
- [src/Genocs.Persistence.MongoDB/Extensions/ServiceCollectionExtensions.cs](src/Genocs.Persistence.MongoDB/Extensions/ServiceCollectionExtensions.cs)

### Repository API surface

- Defines generic repository contracts for querying, paging, add/update, and existence checks.
- Provides ObjectId-specialized repository abstractions for Mongo entity types.
- Includes two repository implementation styles: collection-name constructor and provider-based repository.
- Resolves collection name by TableMappingAttribute when available, otherwise by entity type name.

Files:

- [src/Genocs.Persistence.MongoDB/Domain/Repositories/IMongoBaseRepository.cs](src/Genocs.Persistence.MongoDB/Domain/Repositories/IMongoBaseRepository.cs)
- [src/Genocs.Persistence.MongoDB/Domain/Repositories/IMongoRepository.cs](src/Genocs.Persistence.MongoDB/Domain/Repositories/IMongoRepository.cs)
- [src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoBaseRepository.cs](src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoBaseRepository.cs)
- [src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoBaseRepositoryOfType.cs](src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoBaseRepositoryOfType.cs)
- [src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoRepository.cs](src/Genocs.Persistence.MongoDB/Domain/Repositories/MongoRepository.cs)

### Initialization and seeding lifecycle

- Defines initialization contract compatible with Genocs initializer pipeline.
- Executes database seed only when options.Seed is enabled.
- Provides default seed strategy that exits early when collections already exist.

Files:

- [src/Genocs.Persistence.MongoDB/Repositories/IMongoInitializer.cs](src/Genocs.Persistence.MongoDB/Repositories/IMongoInitializer.cs)
- [src/Genocs.Persistence.MongoDB/Initializers/MongoDbInitializer.cs](src/Genocs.Persistence.MongoDB/Initializers/MongoDbInitializer.cs)
- [src/Genocs.Persistence.MongoDB/Repositories/IMongoSeeder.cs](src/Genocs.Persistence.MongoDB/Repositories/IMongoSeeder.cs)
- [src/Genocs.Persistence.MongoDB/Seeders/MongoSeeder.cs](src/Genocs.Persistence.MongoDB/Seeders/MongoSeeder.cs)

### Database/session providers and option models

- Exposes direct IMongoClient and IMongoDatabase access via provider abstraction.
- Creates client settings from connection string and optional tracing instrumentation.
- Exposes session factory abstraction for transaction-capable workflows.
- Provides configuration objects for runtime Mongo and encryption settings.

Files:

- [src/Genocs.Persistence.MongoDB/IMongoDatabaseProvider.cs](src/Genocs.Persistence.MongoDB/IMongoDatabaseProvider.cs)
- [src/Genocs.Persistence.MongoDB/MongoDatabaseProvider.cs](src/Genocs.Persistence.MongoDB/MongoDatabaseProvider.cs)
- [src/Genocs.Persistence.MongoDB/Repositories/IMongoSessionFactory.cs](src/Genocs.Persistence.MongoDB/Repositories/IMongoSessionFactory.cs)
- [src/Genocs.Persistence.MongoDB/Factories/MongoSessionFactory.cs](src/Genocs.Persistence.MongoDB/Factories/MongoSessionFactory.cs)
- [src/Genocs.Persistence.MongoDB/Configurations/MongoOptions.cs](src/Genocs.Persistence.MongoDB/Configurations/MongoOptions.cs)
- [src/Genocs.Persistence.MongoDB/Configurations/MongoEncryptionOptions.cs](src/Genocs.Persistence.MongoDB/Configurations/MongoEncryptionOptions.cs)

### Query pagination utilities

- Converts IPagedQuery into ordered and limited Mongo IQueryable execution.
- Supports dynamic sort property resolution through expression generation.
- Computes total pages and returns typed paged result payloads.

Files:

- [src/Genocs.Persistence.MongoDB/Repositories/Pagination.cs](src/Genocs.Persistence.MongoDB/Repositories/Pagination.cs)

## Dependencies

From [src/Genocs.Persistence.MongoDB/Genocs.Persistence.MongoDB.csproj](src/Genocs.Persistence.MongoDB/Genocs.Persistence.MongoDB.csproj):

- Genocs.Core (project reference in Debug, package reference in Release)
- MongoDB.Driver
- MongoDB.Driver.Core.Extensions.DiagnosticSources

## Related Docs

- NuGet package readme: [src/Genocs.Persistence.MongoDB/README_NUGET.md](src/Genocs.Persistence.MongoDB/README_NUGET.md)
- Legacy package notes: [src/Genocs.Persistence.MongoDB/_docs/README_NUGET.md](src/Genocs.Persistence.MongoDB/_docs/README_NUGET.md)
- Repository guide: [README.md](README.md)
