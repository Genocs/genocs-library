# Genocs.WebApi.OpenApi Implementation Backlog

## Purpose

This backlog converts the current `Genocs.WebApi.OpenApi` assessment findings into issue-sized implementation work ordered by runtime risk, contract correctness, and long-term maintainability.

Related assessment document:

- `docs/Genocs.WebApi.OpenApi-Assessment.md`

## Current Status

Observed baseline (May 2026):

- `Genocs.WebApi.OpenApi` targets `net10.0`, `net9.0`, and `net8.0`.
- Runtime registration/use contract is brittle when docs are disabled.
- net10 schema generation currently degrades contracts to string-only schemas.
- strict build reports package-local nullability/analyzer issues in document filter.
- Debug/Release dependency strategy is configuration-dependent.
- no dedicated package-level unit test project was found under `src/tests`.

Latest assessment runs:

- `dotnet build src/Genocs.WebApi.OpenApi/Genocs.WebApi.OpenApi.csproj -c Debug --nologo -warnaserror -t:Rebuild`
- `rg --files src/tests | rg "OpenApi|WebApi.OpenApi"`

Strict rebuild currently fails in workspace due to upstream baseline warnings and OpenApi-local warnings, so package-local quality gates are needed.

## Planning Assumptions

- Breaking changes are allowed and preferred when they remove unsafe or ambiguous behavior.
- OpenAPI generation should be deterministic and high-fidelity across target frameworks.
- Middleware registration and execution contracts should be safe under optional configuration.
- Security metadata should be semantically correct and consistent across TFMs.
- Behavior changes must be paired with package-level regression tests and a dedicated validation target.

## Milestones

| Milestone | Goal | Tasks |
|---|---|---|
| M1 | Runtime safety and schema correctness baseline | WEBAPI-OPENAPI-001 to WEBAPI-OPENAPI-003 |
| M2 | Security and contract semantics normalization | WEBAPI-OPENAPI-004 and WEBAPI-OPENAPI-007 |
| M3 | Dependency and API-surface consistency | WEBAPI-OPENAPI-005 to WEBAPI-OPENAPI-006 |
| M4 | Testability and quality gates | WEBAPI-OPENAPI-008 |

## Execution Order

1. Complete M1 before broad host adoption of new OpenAPI changes.
2. Complete M2 before relying on generated auth metadata for clients.
3. Complete M3 before packaging/release hardening.
4. Complete M4 to lock in maintainability and regression safety.

---

## M1: Runtime Safety and Schema Correctness Baseline

### WEBAPI-OPENAPI-001 Harden registration/use contract between AddOpenApiDocs and UseOpenApiDocs

**Status**: Completed (May 2026)

**Priority**: P0

**Problem**

`UseOpenApiDocs()` assumes `OpenApiOptions` is registered, but `AddOpenApiDocs(OpenApiOptions)` currently returns early when disabled and may skip DI registration.

**Likely touch points**

- `src/Genocs.WebApi.OpenApi/Extensions.cs`

**Acceptance criteria**

- `UseOpenApiDocs()` does not throw when OpenAPI services/options were not registered.
- behavior is deterministic for disabled/missing OpenAPI config.
- migration note documents new runtime behavior.

**Dependencies**

- none

**Implementation notes**

- Updated `src/Genocs.WebApi.OpenApi/Extensions.cs` so `AddOpenApiDocs(this IGenocsBuilder, OpenApiOptions)` always registers `OpenApiOptions` in DI before `Enabled` short-circuit logic.
- Added argument validation guards in OpenApi extension entry points (`ArgumentNullException.ThrowIfNull`) for deterministic failure behavior on invalid calls.
- Hardened `UseOpenApiDocs(this IApplicationBuilder)` to no-op safely when:
  - `OpenApiOptions` was not registered,
  - OpenApi is disabled in options,
  - Swagger services (`ISwaggerProvider`) were not registered.
- This removes the startup/runtime crash path when hosts call `UseOpenApiDocs()` while OpenApi registration/config is missing or disabled.
- Validation:
  - `dotnet build src/Genocs.WebApi.OpenApi/Genocs.WebApi.OpenApi.csproj -c Debug --nologo -p:BuildProjectReferences=false`
  - `dotnet build src/Genocs.WebApi.OpenApi/Genocs.WebApi.OpenApi.csproj -c Debug --nologo -warnaserror -p:BuildProjectReferences=false`

### WEBAPI-OPENAPI-002 Restore high-fidelity schema generation for net10

**Status**: Completed (May 2026)

**Priority**: P0

**Problem**

`WebApiDocumentFilter` maps all request/response schemas to string on net10.

**Likely touch points**

- `src/Genocs.WebApi.OpenApi/Filters/WebApiDocumentFilter.cs`

**Acceptance criteria**

- schema generation reflects actual request/response model shapes.
- examples and primitive/object typing are preserved where available.
- unsupported mapping paths fail predictably with diagnostics.

**Dependencies**

- `WEBAPI-OPENAPI-001`

**Implementation notes**

- Updated `src/Genocs.WebApi.OpenApi/Filters/WebApiDocumentFilter.cs` to generate request/response schemas using Swashbuckle `DocumentFilterContext.SchemaGenerator` + `SchemaRepository` instead of hard-coded string schemas.
- Replaced net10 fallback behavior (`JsonSchemaType.String` for all contracts) with type-driven schema generation for both endpoint parameters and responses.
- Added deterministic null-type fallback schemas:
  - net10: `JsonSchemaType.Object`
  - net8/net9: `"object"`
- Preserved existing example serialization behavior on net8/net9 while adopting schema-generator output for model structure.
- Validation:
  - `dotnet build src/Genocs.WebApi.OpenApi/Genocs.WebApi.OpenApi.csproj -c Debug --nologo -p:BuildProjectReferences=false`
- Note: remaining `CS8602` warnings in `WebApiDocumentFilter.cs` are tracked under `WEBAPI-OPENAPI-003` (nullability and unsupported-method hardening).

### WEBAPI-OPENAPI-003 Eliminate nullability defects and unsupported-method crashes in document filter

**Status**: Completed (May 2026)

**Priority**: P1

**Problem**

Current operation creation path can return null and is dereferenced; strict builds fail with `CS8602`.

**Likely touch points**

- `src/Genocs.WebApi.OpenApi/Filters/WebApiDocumentFilter.cs`

**Acceptance criteria**

- strict package-local build has no OpenApi-local nullability/analyzer errors in touched files.
- unsupported/unknown methods are handled deterministically.
- no nullable dereference remains in operation setup path.

**Dependencies**

- `WEBAPI-OPENAPI-002`

**Implementation notes**

- Hardened operation creation flow in `src/Genocs.WebApi.OpenApi/Filters/WebApiDocumentFilter.cs` by replacing nullable operation factory usage with deterministic `TryCreateOperation(...)` behavior.
- Added explicit unsupported-method handling policy:
  - unrecognized HTTP methods are skipped (`continue`) instead of creating nullable operation paths.
- Removed nullable dereference paths in operation setup and response/parameter wiring by ensuring operations are non-null after `TryCreateOperation(...)` succeeds.
- Fixed query parameter type introspection to be null-safe (`parameter.Type?.GetInterface("IQuery")`) to prevent null-type dereferences.
- Normalized operation collection initialization to avoid framework-specific type mismatch and nullable collection assignment issues.
- Validation:
  - `dotnet build src/Genocs.WebApi.OpenApi/Genocs.WebApi.OpenApi.csproj -c Debug --nologo -warnaserror -p:BuildProjectReferences=false`

Result:

- OpenApi package strict package-local build now succeeds for `net10.0`, `net9.0`, and `net8.0` with no OpenApi-local nullability/analyzer errors in touched paths.

---

## M2: Security and Contract Semantics Normalization

### WEBAPI-OPENAPI-004 Normalize Bearer security scheme and requirement behavior across TFMs

**Status**: Completed (May 2026)

**Priority**: P1

**Problem**

Security definition/requirement behavior differs by target framework and uses mixed scheme semantics.

**Likely touch points**

- `src/Genocs.WebApi.OpenApi/Extensions.cs`

**Acceptance criteria**

- security scheme metadata is semantically correct (`http` bearer) and consistent for net8/net9/net10.
- requirement behavior is explicit and documented.
- generated document displays expected auth UX in Swagger UI/ReDoc.

**Dependencies**

- `WEBAPI-OPENAPI-003`

**Implementation notes**

- Updated security configuration in `src/Genocs.WebApi.OpenApi/Extensions.cs` to use consistent HTTP bearer semantics across TFMs:
  - `Type = SecuritySchemeType.Http`
  - `Scheme = "bearer"`
  - `BearerFormat = "JWT"`
- Added explicit security requirement wiring for all TFMs when `IncludeSecurity` is enabled:
  - net10 uses `AddSecurityRequirement(Func<OpenApiDocument, OpenApiSecurityRequirement>)` with `OpenApiSecuritySchemeReference("Bearer")`.
  - net8/net9 use `AddSecurityRequirement(OpenApiSecurityRequirement)` with an `OpenApiReference` to `Bearer` security scheme.
- Removed mixed/ambiguous scheme configuration (`ApiKey` + `oauth2`) and aligned behavior so generated documents consistently advertise bearer auth requirements.
- Validation:
  - `dotnet build src/Genocs.WebApi.OpenApi/Genocs.WebApi.OpenApi.csproj -c Debug --nologo -warnaserror -p:BuildProjectReferences=false`

Result:

- OpenApi package strict package-local build succeeds for `net10.0`, `net9.0`, and `net8.0` with normalized bearer security metadata behavior in configured OpenAPI output.

### WEBAPI-OPENAPI-007 Define and enforce query/body representation policy in document filter

**Status**: Completed (May 2026)

**Priority**: P2

**Problem**

Query flow currently emits request body for `IQuery` types, which can conflict with expected GET query semantics.

**Likely touch points**

- `src/Genocs.WebApi.OpenApi/Filters/WebApiDocumentFilter.cs`
- documentation files under `docs/`

**Acceptance criteria**

- representation policy is explicit for GET/query endpoints.
- generated contracts follow policy consistently.
- migration guidance is added for consumers affected by behavior changes.

**Dependencies**

- `WEBAPI-OPENAPI-002`
- `WEBAPI-OPENAPI-003`

**Implementation notes**

- Updated `src/Genocs.WebApi.OpenApi/Filters/WebApiDocumentFilter.cs` to enforce explicit representation policy for query metadata:
  - entries with `In = "query"` are always emitted as OpenAPI query parameters,
  - request bodies are emitted only for `In = "body"` entries.
- Removed the previous special-case path that emitted `application/json` request bodies for query contract types implementing `IQuery`.
- Added deterministic query-parameter defaults for robustness:
  - fallback parameter name `"query"` when metadata name is empty,
  - explicit `ParameterLocation.Query` assignment for generated query parameters.
- Added migration guidance for consumers in `docs/Genocs.WebApi.OpenApi-Agent-Documentation.md` under **Query Representation Policy** to document the breaking behavior change and expected client updates.
- Validation:
  - `dotnet build src/Genocs.WebApi.OpenApi/Genocs.WebApi.OpenApi.csproj -c Debug --nologo -warnaserror -p:BuildProjectReferences=false`

Result:

- OpenApi package strict package-local build succeeds for `net10.0`, `net9.0`, and `net8.0` with query/body policy now explicit and consistently enforced.

---

## M3: Dependency and API-Surface Consistency

### WEBAPI-OPENAPI-005 Align Debug/Release dependency model for deterministic package behavior

**Status**: Planned

**Priority**: P1

**Problem**

Project currently uses Debug `ProjectReference` and Release pinned `PackageReference` for `Genocs.WebApi`.

**Likely touch points**

- `src/Genocs.WebApi.OpenApi/Genocs.WebApi.OpenApi.csproj`
- central version management files if needed

**Acceptance criteria**

- dependency strategy is deterministic across configurations.
- release package no longer depends on stale/pinned version unless intentionally centralized.
- CI and local builds validate the same effective dependency graph.

**Dependencies**

- `WEBAPI-OPENAPI-003`

### WEBAPI-OPENAPI-006 Align README/fluent builder surface with actual option model and APIs

**Status**: Completed (May 2026)

**Priority**: P2

**Problem**

README references APIs that do not exist and fluent builder does not expose full options model.

**Likely touch points**

- `src/Genocs.WebApi.OpenApi/README_NUGET.md`
- `src/Genocs.WebApi.OpenApi/Configurations/IOpenApiOptionsBuilder.cs`
- `src/Genocs.WebApi.OpenApi/Builders/OpenApiOptionsBuilder.cs`
- `docs/Genocs.WebApi.OpenApi-Agent-Documentation.md`

**Acceptance criteria**

- docs list only existing public APIs.
- fluent builder either supports complete option surface or docs clearly mark unsupported fluent fields.
- package examples compile against current public API.

**Dependencies**

- `WEBAPI-OPENAPI-001`

**Implementation notes**

- Aligned README API references in `src/Genocs.WebApi.OpenApi/README_NUGET.md` by removing non-existent `AddWebApiOpenApiDocs` and documenting current entry points.
- Expanded fluent builder API to cover full `OpenApiOptions` model:
  - updated `src/Genocs.WebApi.OpenApi/Configurations/IOpenApiOptionsBuilder.cs` with:
    - `WithContactEmail(...)`, `WithContactUrl(...)`
    - `WithLicenseName(...)`, `WithLicenseUrl(...)`
    - `WithTermsOfService(...)`
    - `WithServers(...)`, `AddServer(...)`
  - implemented these methods in `src/Genocs.WebApi.OpenApi/Builders/OpenApiOptionsBuilder.cs`.
- Updated `docs/Genocs.WebApi.OpenApi-Agent-Documentation.md` to remove outdated fluent-builder limitation text and reflect full fluent coverage in Core Entry Points guidance.
- Validation:
  - `dotnet build src/Genocs.WebApi.OpenApi/Genocs.WebApi.OpenApi.csproj -c Debug --nologo -warnaserror -p:BuildProjectReferences=false`

Result:

- OpenApi package strict package-local build succeeds for `net10.0`, `net9.0`, and `net8.0`, and docs now match current API surface.

---

## M4: Testability and Quality Gates

### WEBAPI-OPENAPI-008 Add package-specific tests and introduce package-level quality gate

**Status**: Completed (May 2026)

**Priority**: P0

**Problem**

No dedicated package-level tests/validation target currently guards `Genocs.WebApi.OpenApi` behavior.

**Likely touch points**

- `src/tests` (new `Genocs.WebApi.OpenApi.UnitTests` project)
- `Makefile`
- new package validation target file (for example `validate-webapi-openapi.mk`)
- `src/Genocs.WebApi.OpenApi/README_NUGET.md`
- `docs/Genocs.WebApi.OpenApi-Agent-Documentation.md`

**Acceptance criteria**

- new unit test project covers:
  - registration/use behavior for enabled and disabled docs,
  - document filter operation creation and unsupported method handling,
  - schema generation fidelity policy,
  - security metadata consistency,
  - query/body representation policy.
- introduce `make validate-webapi-openapi` target that builds package and runs OpenApi tests.
- validation guidance is documented for maintainers.

**Dependencies**

- `WEBAPI-OPENAPI-001`
- `WEBAPI-OPENAPI-002`
- `WEBAPI-OPENAPI-003`
- `WEBAPI-OPENAPI-004`
- `WEBAPI-OPENAPI-005`
- `WEBAPI-OPENAPI-006`
- `WEBAPI-OPENAPI-007`

**Implementation notes**

- Added dedicated package test project:
  - `src/tests/Genocs.WebApi.OpenApi.UnitTests/Genocs.WebApi.OpenApi.UnitTests.csproj`
  - test suite: `src/tests/Genocs.WebApi.OpenApi.UnitTests/OpenApi/OpenApiDocsBehaviorTests.cs`
- Added focused package-level coverage for required areas:
  - registration/use behavior for enabled/disabled/missing OpenApi registration,
  - document filter behavior for unsupported methods,
  - schema generation fidelity for request/response model contracts,
  - security metadata consistency when `IncludeSecurity` is enabled,
  - query/body representation policy enforcement (`In = "query"` -> query parameters, no request body).
- Wired quality gate targets:
  - new file: `validate-webapi-openapi.mk`
    - `validate-webapi-openapi-build`
    - `validate-webapi-openapi-tests`
    - aggregate `validate-webapi-openapi`
  - updated root `Makefile` target:
    - `make validate-webapi-openapi` -> `$(MAKE) -f validate-webapi-openapi.mk validate-webapi-openapi`
- Included maintainer validation guidance in:
  - `src/Genocs.WebApi.OpenApi/README_NUGET.md`
  - `docs/Genocs.WebApi.OpenApi-Agent-Documentation.md`
- Added new test project to solution model:
  - `genocs.slnx`
- Validation:
  - `dotnet build src/Genocs.WebApi.OpenApi/Genocs.WebApi.OpenApi.csproj -f net10.0 -c Debug --nologo -warnaserror -p:BuildProjectReferences=false`
  - `dotnet test src/tests/Genocs.WebApi.OpenApi.UnitTests/Genocs.WebApi.OpenApi.UnitTests.csproj -c Debug --nologo`
- Note: this shell does not have GNU `make` installed (`make: command not found`), so the aggregate target was validated by executing the underlying build/test commands directly.

---

## Suggested First Execution Slice

1. `WEBAPI-OPENAPI-001`
2. `WEBAPI-OPENAPI-002`
3. `WEBAPI-OPENAPI-003`
4. `WEBAPI-OPENAPI-004`
5. `WEBAPI-OPENAPI-008`

This slice addresses crash and contract-fidelity risk first while adding quality gates early.

## Validation Commands (Target State)

- `dotnet build src/Genocs.WebApi.OpenApi/Genocs.WebApi.OpenApi.csproj -c Debug --nologo`
- `dotnet build src/Genocs.WebApi.OpenApi/Genocs.WebApi.OpenApi.csproj -c Debug --nologo -warnaserror -p:BuildProjectReferences=false`
- `dotnet test src/tests/Genocs.WebApi.OpenApi.UnitTests/Genocs.WebApi.OpenApi.UnitTests.csproj -c Debug --nologo`
- `make validate-webapi-openapi`
