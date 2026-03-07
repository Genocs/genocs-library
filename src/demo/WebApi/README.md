# Demo WebApi - BookStore (EF Core + SQL Server)

This folder contains the Genocs demo WebApi host plus a complete BookStore sample implemented with EF Core and SQL Server.

The BookStore flow includes:
- Domain model: `Book`, `Author`, many-to-many `BookAuthor`.
- EF Core DbContext configuration with SQL Server provider.
- Minimal APIs for author and book operations.
- Migration-based database initialization at startup.
- Seed data for a quick end-to-end demo.

## Where to find the BookStore code

- DbContext: `BookStore/Data/BookStoreDbContext.cs`
- Design-time factory (for EF CLI): `BookStore/Data/BookStoreDbContextFactory.cs`
- DB initializer (migrate + seed): `BookStore/Data/BookStoreDatabaseInitializer.cs`
- Domain entities: `BookStore/Domain/*`
- Request/response contracts: `BookStore/Contracts/BookStoreContracts.cs`
- API endpoints: `Features/BookStoreFeature.cs`
- API registration: `Features/FeatureEndpointsModule.cs`
- Sample HTTP client requests: `Genocs.Core.Demo.HelloWorld.http`

## Run the demo API

From repository root:

```bash
dotnet run --project src/demo/WebApi/Host.csproj
```

Swagger/OpenAPI is enabled by default in this demo host. Check configured routes in `appsettings.json` under `openapi`.

## Connection string

BookStore uses:

- Config key: `ConnectionStrings:BookStore`
- Default value in `appsettings.json`:

```json
"ConnectionStrings": {
  "BookStore": "Server=(localdb)\\MSSQLLocalDB;Database=Genocs.BookStore.Demo;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

You can override it via environment variables or your preferred appsettings profile.

## Migration support

This WebApi uses EF migrations (not `EnsureCreated`) so schema evolution is tracked and repeatable.

### What is already in place

- Initial migration generated:
  - `BookStore/Migrations/20260307142613_InitialBookStore.cs`
  - `BookStore/Migrations/BookStoreDbContextModelSnapshot.cs`
- Startup applies migrations via:
  - `BookStoreDatabaseInitializer.InitializeAsync(...)`
  - `dbContext.Database.MigrateAsync(...)`
- Local EF tool manifest at repository root:
  - `dotnet-tools.json` (pins `dotnet-ef` to `10.0.3`)

### Legacy database compatibility note

If a local DB was created earlier with `EnsureCreated`, applying migrations directly can fail with errors like:

`There is already an object named 'Authors' in the database.`

To handle this scenario, the initializer contains a one-time baseline step:
- It detects an existing legacy BookStore schema.
- It creates `__EFMigrationsHistory` if missing.
- It inserts the initial migration id before running `MigrateAsync`.

This keeps local developer databases compatible during the transition to migration-based management.

## EF commands

Before running EF commands in a fresh clone:

```bash
dotnet tool restore
```

List migrations:

```bash
dotnet dotnet-ef migrations list \
  --project src/demo/WebApi/Host.csproj \
  --startup-project src/demo/WebApi/Host.csproj \
  --context BookStoreDbContext
```

Add a new migration:

```bash
dotnet dotnet-ef migrations add <MigrationName> \
  --project src/demo/WebApi/Host.csproj \
  --startup-project src/demo/WebApi/Host.csproj \
  --context BookStoreDbContext \
  --output-dir BookStore/Migrations
```

Apply migrations:

```bash
dotnet dotnet-ef database update \
  --project src/demo/WebApi/Host.csproj \
  --startup-project src/demo/WebApi/Host.csproj \
  --context BookStoreDbContext
```

Remove last migration (if not applied to shared DB environments):

```bash
dotnet dotnet-ef migrations remove \
  --project src/demo/WebApi/Host.csproj \
  --startup-project src/demo/WebApi/Host.csproj \
  --context BookStoreDbContext
```

## BookStore API endpoints

Base route: `/api/bookstore`

Authors:
- `GET /api/bookstore/authors`
- `GET /api/bookstore/authors/{id}`
- `POST /api/bookstore/authors`

Books:
- `GET /api/bookstore/books`
- `GET /api/bookstore/books/{id}`
- `POST /api/bookstore/books`
- `PUT /api/bookstore/books/{id}`
- `DELETE /api/bookstore/books/{id}`

Book create/update requests accept `authorIds` (one or many) to manage relationships.

## Quick smoke test

Use the included HTTP file:

- `src/demo/WebApi/Genocs.Core.Demo.HelloWorld.http`

It contains ready-to-run requests for:
- listing/creating authors,
- listing/creating/updating/deleting books,
- and retrieving resources by id.
