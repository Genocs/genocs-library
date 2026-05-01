# Genocs.WebApi.OpenApi Assessment (May 2026)

## Purpose

This assessment evaluates the current `Genocs.WebApi.OpenApi` implementation to identify runtime risks, documentation correctness gaps, and modernization opportunities.

Breaking changes are considered acceptable for this assessment track.

## Assessment Scope

Reviewed areas:

- OpenAPI registration and runtime middleware behavior
- document filter correctness and schema generation quality
- nullability/analyzer posture in package-local code paths
- package dependency/configuration consistency across build configurations
- package-level test and quality-gate coverage
- package documentation/API surface alignment

Primary files inspected:

- `src/Genocs.WebApi.OpenApi/Extensions.cs`
- `src/Genocs.WebApi.OpenApi/Filters/WebApiDocumentFilter.cs`
- `src/Genocs.WebApi.OpenApi/Configurations/OpenApiOptions.cs`
- `src/Genocs.WebApi.OpenApi/Configurations/IOpenApiOptionsBuilder.cs`
- `src/Genocs.WebApi.OpenApi/Builders/OpenApiOptionsBuilder.cs`
- `src/Genocs.WebApi.OpenApi/Genocs.WebApi.OpenApi.csproj`
- `src/Genocs.WebApi.OpenApi/README_NUGET.md`
- `docs/Genocs.WebApi.OpenApi-Agent-Documentation.md`

## Validation Evidence

Executed on 2026-05-01:

- `dotnet build src/Genocs.WebApi.OpenApi/Genocs.WebApi.OpenApi.csproj -c Debug --nologo -warnaserror -t:Rebuild`
- `rg --files src/tests | rg "OpenApi|WebApi.OpenApi"`

Results:

- strict build fails in workspace due to upstream baseline warnings in `Genocs.Core` and OpenApi-local strict findings
- OpenApi-local strict findings confirmed in `WebApiDocumentFilter.cs`:
  - `CS8602` possible null dereference
  - `SA1515` style violations
- no dedicated package-level unit test project found for `Genocs.WebApi.OpenApi`

## Executive Summary

`Genocs.WebApi.OpenApi` works for basic Swagger exposure but has several correctness and maintainability risks. The highest-impact concerns are:

- brittle runtime contract between `AddOpenApiDocs(...)` and `UseOpenApiDocs()` that can crash hosts,
- schema/document generation regressions in .NET 10 where all schemas collapse to string,
- nullability defects in the document filter that already fail strict builds,
- configuration drift between Debug/Release dependency strategies.

The package is a strong candidate for a breaking-change cleanup similar to the CQRS modernization pathway.

## Findings (Ordered by Severity)

### 1) Runtime failure risk when OpenAPI registration is skipped but middleware is still used

Severity: Critical  
ID: `WEBAPI-OPENAPI-001`

Where:

- `src/Genocs.WebApi.OpenApi/Extensions.cs`

Details:

- `AddOpenApiDocs(OpenApiOptions)` returns early when `settings.Enabled` is false, and in that branch does not register `OpenApiOptions` in DI.
- `UseOpenApiDocs()` unconditionally calls `GetRequiredService<OpenApiOptions>()`.

Impact:

- host startup/runtime exception if middleware is invoked while docs are disabled or registration is omitted.
- package behavior depends on call ordering and optional configuration presence.

Recommendation:

- make middleware resilient with `GetService<OpenApiOptions>()` + no-op fallback, or
- always register options in DI and guard internals by `Enabled`.

### 2) .NET 10 schema generation currently degrades all request/response schemas to string

Severity: Critical  
ID: `WEBAPI-OPENAPI-002`

Where:

- `src/Genocs.WebApi.OpenApi/Filters/WebApiDocumentFilter.cs`

Details:

- net10 schema builders return `OpenApiSchema { Type = JsonSchemaType.String }` for both parameters and responses.
- example serialization/commented mapping is unfinished.

Impact:

- generated OpenAPI contract loses structural type information.
- downstream tooling (client generation, validation, AI tool integrations) gets low-fidelity contracts.

Recommendation:

- implement schema mapping using Swashbuckle schema generator/context instead of hard-coded primitive fallback.
- fail fast if schema generation cannot map a type rather than silently degrading every schema.

### 3) Nullability defects in document filter cause strict-build failure and potential runtime faults

Severity: High  
ID: `WEBAPI-OPENAPI-003`

Where:

- `src/Genocs.WebApi.OpenApi/Filters/WebApiDocumentFilter.cs`

Details:

- strict build reports multiple `CS8602` dereference warnings at operation creation/usage points.
- `_getOperation(...)` can return null for unrecognized methods but is dereferenced immediately.

Impact:

- strict-mode quality gate fails.
- runtime `NullReferenceException` risk for unsupported or unexpectedly cased HTTP methods.

Recommendation:

- normalize HTTP method parsing and use explicit guard clauses for unsupported methods.
- remove nullable dereference paths and make behavior deterministic (skip with diagnostics or throw with context).

### 4) Security scheme configuration is inconsistent across target frameworks

Severity: High  
ID: `WEBAPI-OPENAPI-004`

Where:

- `src/Genocs.WebApi.OpenApi/Extensions.cs`

Details:

- net10 branch adds only security definition and does not add security requirement.
- net8/net9 branch adds a requirement but uses mixed semantics (`Type = ApiKey` definition with requirement scheme configured as `oauth2`).

Impact:

- inconsistent UI/operation auth behavior across TFMs.
- clients may not infer Bearer auth requirements correctly.

Recommendation:

- standardize on OpenAPI HTTP bearer scheme (`Type=Http`, `Scheme="bearer"`, `BearerFormat="JWT"`) and add consistent requirement policy across all TFMs.

### 5) Debug/Release dependency strategy is split (project reference vs pinned package)

Severity: Medium  
ID: `WEBAPI-OPENAPI-005`

Where:

- `src/Genocs.WebApi.OpenApi/Genocs.WebApi.OpenApi.csproj`

Details:

- Debug uses `ProjectReference` to `Genocs.WebApi`.
- Release uses pinned `PackageReference` (`Genocs.WebApi` `9.0.0-beta007`).

Impact:

- local and packaged behavior can drift.
- modernization and regression diagnosis become less deterministic.

Recommendation:

- unify dependency strategy across configurations (prefer deterministic single model).

### 6) Documentation and API surface are out of sync

Severity: Medium  
ID: `WEBAPI-OPENAPI-006`

Where:

- `src/Genocs.WebApi.OpenApi/README_NUGET.md`
- `src/Genocs.WebApi.OpenApi/Configurations/IOpenApiOptionsBuilder.cs`
- `src/Genocs.WebApi.OpenApi/Configurations/OpenApiOptions.cs`

Details:

- README references `AddWebApiOpenApiDocs`, but no such API exists in source.
- options model includes fields (contact URL/email, license, terms, servers), but builder interface exposes only a subset.

Impact:

- higher chance of misconfiguration and false assumptions by consumers.
- fluent configuration path is incomplete relative to object model.

Recommendation:

- align README to actual APIs.
- either expand builder surface to full options model or explicitly document unsupported fluent fields.

### 7) Document filter semantics for query contracts can violate expected GET conventions

Severity: Medium  
ID: `WEBAPI-OPENAPI-007`

Where:

- `src/Genocs.WebApi.OpenApi/Filters/WebApiDocumentFilter.cs`

Details:

- when a query parameter type implements `IQuery`, filter emits request body (`application/json`) even in query flow.
- behavior diverges from common query-string representation and can surprise generated clients.

Impact:

- inconsistent contract semantics and potential tooling incompatibility.

Recommendation:

- define explicit query representation policy and apply consistently (query parameters for GET unless endpoint explicitly accepts body).

### 8) No package-specific tests or package-local validation target

Severity: Medium  
ID: `WEBAPI-OPENAPI-008`

Where:

- `src/tests`
- `Makefile` and validation makefiles

Details:

- no dedicated `Genocs.WebApi.OpenApi` test project found.
- no package-level validation target analogous to other package validators.

Impact:

- regressions in document generation and middleware behavior can pass unnoticed.

Recommendation:

- add package-level tests covering middleware registration/use behavior, schema generation, auth metadata, and document filter edge cases.
- add `validate-webapi-openapi` target to build in strict package-local mode and run dedicated tests.

## Suggested Modernization Workstream

1. `WEBAPI-OPENAPI-001`
2. `WEBAPI-OPENAPI-002`
3. `WEBAPI-OPENAPI-003`
4. `WEBAPI-OPENAPI-004`
5. `WEBAPI-OPENAPI-008`
6. `WEBAPI-OPENAPI-005`
7. `WEBAPI-OPENAPI-006`
8. `WEBAPI-OPENAPI-007`

This order addresses crash risk and contract correctness first, then quality gates, then consistency/documentation cleanup.

## Validation Commands (Target State)

- `dotnet build src/Genocs.WebApi.OpenApi/Genocs.WebApi.OpenApi.csproj -c Debug --nologo`
- `dotnet build src/Genocs.WebApi.OpenApi/Genocs.WebApi.OpenApi.csproj -c Debug --nologo -warnaserror -p:BuildProjectReferences=false`
- `dotnet test src/tests/Genocs.WebApi.OpenApi.UnitTests/Genocs.WebApi.OpenApi.UnitTests.csproj -c Debug --nologo`
- `make validate-webapi-openapi`
