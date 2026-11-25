# Genocs.Persistence.CosmosDB Agent Reference

## Agent Operating Mode

- Assume `Genocs.Persistence.CosmosDB` is consumed from NuGet only.
- Do not assume repository or source-code visibility.
- Treat documented extension methods, options, interfaces, and repository types as the only safe API surface.
- Generate Cosmos DB registration, document types, and repository injection code only.
- Do not invent container provisioning, seeding, patch updates, optimistic concurrency, sessions, transactional batches, or change feed.
- If package composition is unclear, ask whether `Genocs.Core` is already installed because registration extends `IGenocsBuilder`.

## Package Identity

| Key | Value |
|---|---|
| Package | `Genocs.Persistence.CosmosDB` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | Azure Cosmos DB connectivity and container repository CRUD |
| Main value | `AddCosmos(...)`, `AddCosmosRepository<TEntity, TKey>(...)`, `ICosmosRepository<TEntity, TKey>`, `ICosmosEntity<TKey>` |
| Requires | `Genocs.Core` and Azure Cosmos DB |
| Configuration section | `cosmosDb` (`CosmosOptions.Position`) |

## What This Package Is For

Use `Genocs.Persistence.CosmosDB` when you need to:

- register one shared `CosmosClient` and `Database` for a Genocs host
- inject a typed repository for one existing container
- point-read, query, insert, replace, and delete documents whose id is a `string` or `Guid`
- store the partition value on the document as `partitionKey`

## What This Package Does Not Do By Itself

Do not assume `Genocs.Persistence.CosmosDB` can:

- create the database or the container
- run schema migrations
- seed data during startup
- scan assemblies and register repositories automatically
- patch a subset of properties
- apply optimistic concurrency or ETag checks
- expose a unit of work, session, transactional batch, or change feed
- accept keys other than `string` and `Guid`
- validate connectivity while `AddCosmos` is called
- throw when the `cosmosDb` section is missing or invalid

## Safe Default Mental Model

Treat `Genocs.Persistence.CosmosDB` as three things:

1. A Genocs builder extension that registers one `CosmosClient` and one `Database`
2. An explicit per-container repository registration
3. A camelCase System.Text.Json serializer so `Id` becomes the Cosmos item `id` and `PartitionKey` becomes `partitionKey`

The container must already exist and must use the partition key path `/partitionKey`.

## Choose The Right Registration Path

| Situation | Prefer | Why |
|---|---|---|
| The host needs the client and database, and repositories are registered separately | `AddCosmos()` | smallest registration path |
| One document type maps to one container | `AddCosmos()` then `AddCosmosRepository<TEntity, TKey>(containerName)` | binds `ICosmosRepository<TEntity, TKey>` |
| The caller needs raw SDK access | inject `CosmosClient` or `Database` | bypass the repository deliberately |

There is one repository implementation. Do not invent a second ObjectId-style registration path.

## Fast Start Recipes

### Recipe 1: Register The Client Only

```csharp
using Genocs.Core.Builders;
using Genocs.Persistence.CosmosDB.Extensions;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
    .AddGenocs()
    .AddCosmos();

genocs.Build();

var app = builder.Build();
app.Run();
```

```json
{
  "cosmosDb": {
    "connectionString": "AccountEndpoint=https://localhost:8081/;AccountKey=...",
    "database": "orders"
  }
}
```

Effect:

- registers `CosmosOptions` as a singleton instance
- registers `CosmosClient` as a singleton
- registers `Database` as a singleton for `cosmosDb.database`
- does not register `ICosmosRepository<TEntity, TKey>`

Important behavior:

- a missing section returns the builder unchanged
- invalid options (empty connection string or database) return the builder unchanged
- a second `AddCosmos` call does not replace the first registration (`TryRegister("cosmosDb")`)

### Recipe 2: Register A Container Repository

```csharp
using Genocs.Core.Builders;
using Genocs.Persistence.CosmosDB.Domain.Entities;
using Genocs.Persistence.CosmosDB.Domain.Repositories;
using Genocs.Persistence.CosmosDB.Extensions;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
    .AddGenocs()
    .AddCosmos()
    .AddCosmosRepository<OrderDocument, Guid>("orders");

genocs.Build();

public sealed class OrdersService(ICosmosRepository<OrderDocument, Guid> repository)
{
}

public sealed class OrderDocument : ICosmosEntity<Guid>
{
    public Guid Id { get; set; }

    public string PartitionKey { get; set; } = default!;

    public bool IsTransient() => Id == Guid.Empty;
}
```

`AddCosmosRepository` throws `ArgumentException` when `containerName` is blank and `NotSupportedException` when `TKey` is not `string` or `Guid`.

## Repository Contract

`ICosmosRepository<TEntity, TKey>` requires `TEntity : class, ICosmosEntity<TKey>`.

| Call | Cosmos operation | Result |
|---|---|---|
| `GetAsync(id, partitionKey)` | `ReadItem` | `null` on HTTP 404 |
| `GetAsync(predicate)` | LINQ query, all pages | `null` when empty; `InvalidOperationException` when more than one match |
| `FindAsync(predicate)` | LINQ query, all pages | empty list when nothing matches |
| `AddAsync(entity)` | `CreateItem` in `entity.PartitionKey` | conflict when the id already exists |
| `UpdateAsync(entity)` | `ReplaceItem` in `entity.PartitionKey` | full document replacement; not found when the item is missing |
| `DeleteAsync(id, partitionKey)` | `DeleteItem` | not found when the item is missing |
| `ExistsAsync(id, partitionKey)` | `ReadItem` | `false` on HTTP 404 |

Queries can span partitions. Point reads and deletes require a non-empty partition key. `Guid.Empty` and a blank string id throw `ArgumentException`.

## Serialization

The client serializer is System.Text.Json:

- property names are camelCase
- `Id` is written as `id`
- `PartitionKey` is written as `partitionKey`
- null properties are omitted
- `Guid` values are JSON strings

Do not add a second serializer or a Newtonsoft naming convention in application code unless the caller replaces the client. The package references Newtonsoft.Json only because the Cosmos SDK requires that package reference.

## Configuration Ownership

`Genocs.Persistence.CosmosDB` owns the `cosmosDb` section through `CosmosOptions`.

```json
{
  "cosmosDb": {
    "connectionString": "AccountEndpoint=https://localhost:8081/;AccountKey=...",
    "database": "orders"
  }
}
```

Both fields are required:

- `connectionString`: AccountEndpoint and AccountKey connection string
- `database`: database id passed to `CosmosClient.GetDatabase`

No other fields are read. Do not generate `seed`, tracing, or container-throughput settings for this package.

## Public Capability Map

### Registration

- `AddCosmos(sectionName = "cosmosDb")`
- `AddCosmos(CosmosOptions)`
- `AddCosmosRepository<TEntity, TKey>(containerName)`

### Options and documents

- `CosmosOptions`
- `ICosmosEntity<TKey>`

### Repository

- `ICosmosRepository<TEntity, TKey>`

### Driver access

- `CosmosClient`
- `Database`

`CosmosRepository<TEntity, TKey>` is internal. Inject the interface.

## Source-Blind Guardrails For Agents

1. Do not assume `AddCosmos()` throws when configuration is missing. It can no-op.
2. Do not assume repeated `AddCosmos(...)` calls override one another. Registration is guarded by `TryRegister("cosmosDb")`.
3. Do not assume repositories are registered by `AddCosmos()` alone.
4. Do not assume an `int`, `long`, or `ObjectId` key works. Only `string` and `Guid` are supported.
5. Do not assume the package creates containers or databases.
6. Do not assume the partition key path is configurable. It is `/partitionKey`.
7. Do not assume updates are partial. `UpdateAsync` replaces the document.
8. Do not assume `GetAsync(predicate)` returns the first match. More than one match throws.
9. Do not assume a missing point read throws. `GetAsync(id, partitionKey)` and `ExistsAsync` treat HTTP 404 as absence.
10. Do not assume ETag, `IVersioned`, sessions, or seeding exist in this package.
11. Do not construct a second `CosmosClient` in application code when the DI client is available.

## Agent Decision Checklist

Before generating code that depends on `Genocs.Persistence.CosmosDB`, answer these questions:

1. Is the host using `Genocs.Core`?
2. Does configuration contain non-empty `cosmosDb.connectionString` and `cosmosDb.database`?
3. Does the container already exist with partition key path `/partitionKey`?
4. Is the document id a `string` or a `Guid`?
5. Does every point read, delete, and write have the partition key value?
6. Is a full-document replace acceptable for updates?

If any answer is unknown, register with `AddCosmos()` and name the container explicitly in `AddCosmosRepository`.

## Common Tasks And Safe Responses

### Task: "Register Cosmos DB for a service"

Safe response:

- call `AddGenocs()` then `AddCosmos()`
- keep `connectionString` and `database` explicit
- call `AddCosmosRepository<TEntity, TKey>(containerName)` for each container
- inject `ICosmosRepository<TEntity, TKey>`

### Task: "Store documents with Guid keys"

Safe response:

- implement `ICosmosEntity<Guid>`
- register `AddCosmosRepository<TEntity, Guid>("container-name")`
- leave `Id` as `Guid`; the repository formats it as a dashed string

### Task: "Query without a partition key"

Safe response:

- use `FindAsync` or `GetAsync(predicate)`
- state that the query can span partitions
- use `GetAsync(id, partitionKey)` when the partition value is known

### Task: "Create the container if it is missing"

Safe response:

- tell the caller this package does not provision containers
- keep provisioning outside the repository registration

### Task: "Protect a document from lost updates"

Safe response:

- state that this package does not check ETags or versions
- `UpdateAsync` is last-write-wins full replacement

## Failure Modes And Troubleshooting

1. `ICosmosRepository<TEntity, TKey>` cannot be resolved.
Fix: call `AddCosmosRepository<TEntity, TKey>(containerName)` for that closed type. `AddCosmos()` alone does not register it.

2. Cosmos services are missing and startup does not fail.
Fix: check that the `cosmosDb` section exists and that both `connectionString` and `database` are non-empty.

3. A second `AddCosmos` call appears to be ignored.
Fix: the first successful registration wins. Change the first options instead of registering twice.

4. `AddCosmosRepository` throws `NotSupportedException`.
Fix: change `TKey` to `string` or `Guid`.

5. Point reads return `null` for documents that exist.
Fix: pass the same partition value stored in `partitionKey`, and use the same id string the repository writes for `Guid` keys.

6. Queries or writes fail because the container is missing.
Fix: create the container beforehand with partition key path `/partitionKey`.

7. An update drops fields.
Fix: `UpdateAsync` replaces the whole document. Load the current document, change it, then replace it.

8. `GetAsync(predicate)` throws `InvalidOperationException`.
Fix: the filter matched more than one document. Use `FindAsync` or tighten the filter.

## Related Packages To Ask About

- `Genocs.Core`
- `Genocs.Common`
- `Genocs.Persistence.MongoDB` when the host already uses MongoDB and must not be mixed into the Cosmos registration by accident

## One-Line Recommendation For Agents

If you only know that `Genocs.Persistence.CosmosDB` is installed, call `AddCosmos()`, then `AddCosmosRepository<TEntity, TKey>(containerName)` for a `string` or `Guid` document that implements `ICosmosEntity<TKey>`, and require an existing container whose partition key path is `/partitionKey`.
