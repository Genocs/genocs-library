# Genocs.WebApi.CQRS Implementation Backlog

## Purpose

This backlog converts the current `Genocs.WebApi.CQRS` assessment findings into issue-sized implementation work ordered by runtime risk, host safety, and long-term maintainability.

The backlog is structured for phased delivery similar to existing package backlogs and assumes deep upgrade changes are allowed.

Related assessment document:

- `docs/Genocs.WebApi.CQRS-Assessment.md`

## Current Status

Observed baseline (May 2026):

- `Genocs.WebApi.CQRS` targets `net10.0`, `net9.0`, and `net8.0`.
- Package currently mixes legacy app-pipeline composition (`UseRouting`/`UseAuthorization`/`UseEndpoints`) with endpoint mapping helpers.
- `PublicContractsMiddleware` uses static mutable process-wide state and one-time initialization.
- CQRS package has strict-build warnings/errors in touched files (nullability and analyzer formatting).
- No dedicated package-level unit test project for `Genocs.WebApi.CQRS` was found under `src/tests`.

Latest assessment runs:

- `dotnet build src/Genocs.WebApi.CQRS/Genocs.WebApi.CQRS.csproj -c Debug --nologo -t:Rebuild`
- `dotnet build src/Genocs.WebApi.CQRS/Genocs.WebApi.CQRS.csproj -c Debug --nologo -warnaserror -t:Rebuild`

Strict rebuild currently fails in workspace due to upstream baseline warnings and CQRS-local warnings, so package-local quality gates are needed.

## Planning Assumptions

- Breaking changes are allowed and preferred when they remove legacy behavior traps.
- Host pipeline ownership should remain with the host application, not package extension methods.
- Nullability contracts should be explicit and non-ambiguous across public interfaces and implementations.
- Runtime contract discovery should be deterministic and safe under multi-host and test execution.
- Behavior changes must be paired with package-level regression tests and a dedicated validation target.

## Milestones

| Milestone | Goal | Tasks |
|---|---|---|
| M1 | Runtime correctness and process safety baseline | WEBAPI-CQRS-001 to WEBAPI-CQRS-003 |
| M2 | Endpoint API and host integration modernization | WEBAPI-CQRS-004 to WEBAPI-CQRS-005 |
| M3 | Nullability, analyzer, and dependency baseline | WEBAPI-CQRS-006 to WEBAPI-CQRS-007 |
| M4 | Testability and quality gates | WEBAPI-CQRS-008 |

## Execution Order

1. Complete M1 before changing endpoint contracts.
2. Complete M2 before broad service-host adoption.
3. Complete M3 before enabling strict package-level warning gates.
4. Complete M4 to lock in long-term maintainability.

---

## M1: Runtime Correctness and Process Safety Baseline

### WEBAPI-CQRS-001 Fix async response completion in public contracts middleware

**Status**: Completed (May 2026)

**Priority**: P0

**Problem**

`PublicContractsMiddleware.InvokeAsync(...)` does not await `Response.WriteAsync`, so the middleware can complete before payload write is finished.

**Likely touch points**

- `src/Genocs.WebApi.CQRS/Middlewares/PublicContractsMiddleware.cs`

**Acceptance criteria**

- Contracts response body write is awaited.
- Middleware returns a single completion task representing full response write.
- Regression tests verify contracts endpoint always emits complete payload.

**Dependencies**

- none

**Implementation notes**

- Updated `InvokeAsync(HttpContext)` in `src/Genocs.WebApi.CQRS/Middlewares/PublicContractsMiddleware.cs` to use async/await semantics and await `context.Response.WriteAsync(_serializedContracts)`.
- Kept non-matching path behavior unchanged while ensuring branch completion is represented by awaited middleware tasks.
- Added focused regression tests in `src/tests/Genocs.WebApi.CQRS.UnitTests/Middlewares/PublicContractsMiddlewareTests.cs` covering:
  - middleware completion waits for response write completion,
  - contracts endpoint writes payload and content type.
- Added dedicated test project `src/tests/Genocs.WebApi.CQRS.UnitTests/Genocs.WebApi.CQRS.UnitTests.csproj` and included it in `genocs.slnx`.
- Validation: `dotnet test src/tests/Genocs.WebApi.CQRS.UnitTests/Genocs.WebApi.CQRS.UnitTests.csproj -c Debug --nologo` and `dotnet build src/Genocs.WebApi.CQRS/Genocs.WebApi.CQRS.csproj -c Debug --nologo`.

### WEBAPI-CQRS-002 Remove process-wide static mutable state from contract discovery

**Status**: Completed (May 2026)

**Priority**: P0

**Problem**

`PublicContractsMiddleware` caches discovery state in static fields (`_initialized`, `Contracts`, `_serializedContracts`) causing cross-host nondeterminism and option leakage.

**Likely touch points**

- `src/Genocs.WebApi.CQRS/Middlewares/PublicContractsMiddleware.cs`
- `src/Genocs.WebApi.CQRS/Extensions.cs`

**Acceptance criteria**

- Contract discovery and serialization state is scoped by host configuration, not process-global static fields.
- Multiple hosts/tests with different contract options do not leak behavior.
- Discovery has deterministic concurrency behavior.

**Dependencies**

- `WEBAPI-CQRS-001`

**Implementation notes**

- Removed process-wide static mutable discovery state from `src/Genocs.WebApi.CQRS/Middlewares/PublicContractsMiddleware.cs` (`Contracts`, `_initialized`, and static `_serializedContracts`).
- Changed contract snapshot generation to instance-scoped initialization via constructor (`_serializedContracts = Load(attributeType, attributeRequired)`), ensuring middleware instances do not leak discovery output across hosts/options.
- Updated discovery/load logic to operate on local `ContractTypes` instances and return serialized payload deterministically per middleware instance.
- Added regression test in `src/tests/Genocs.WebApi.CQRS.UnitTests/Middlewares/PublicContractsMiddlewareTests.cs` validating that two middleware instances using different attribute filters produce isolated payloads without cross-instance leakage.
- Validation: `dotnet test src/tests/Genocs.WebApi.CQRS.UnitTests/Genocs.WebApi.CQRS.UnitTests.csproj -c Debug --nologo` and `dotnet build src/Genocs.WebApi.CQRS/Genocs.WebApi.CQRS.csproj -c Debug --nologo`.

### WEBAPI-CQRS-003 Harden contract discovery against reflection and naming failures

**Status**: Completed (May 2026)

**Priority**: P1

**Problem**

Current assembly scanning and dictionary keying can fail on loadable-type issues and duplicate simple names.

**Likely touch points**

- `src/Genocs.WebApi.CQRS/Middlewares/PublicContractsMiddleware.cs`

**Acceptance criteria**

- Discovery handles partial type-load failures without hard process failure.
- Duplicate contract names follow explicit policy (fail-fast with diagnostics or deterministic key strategy).
- Unit tests cover duplicate-name and partial-load scenarios.

**Dependencies**

- `WEBAPI-CQRS-002`

**Implementation notes**

- Hardened contract type discovery in `src/Genocs.WebApi.CQRS/Middlewares/PublicContractsMiddleware.cs` to handle partial type-load failures safely by catching `ReflectionTypeLoadException` and continuing with loadable types.
- Added resilient fallback behavior for unexpected discovery exceptions by returning an empty type set instead of failing middleware initialization.
- Replaced duplicate simple-name hard-fail behavior with deterministic keying:
  - first contract uses simple type name,
  - collisions are keyed by full name,
  - further collisions fall back to assembly-qualified deterministic key fragment.
- Added focused regression coverage in `src/tests/Genocs.WebApi.CQRS.UnitTests/Middlewares/PublicContractsMiddlewareTests.cs` for:
  - duplicate-name command discovery without exception and with deterministic distinct keys,
  - `ReflectionTypeLoadException` handling that still emits payload from loadable types.
- Validation: `dotnet test src/tests/Genocs.WebApi.CQRS.UnitTests/Genocs.WebApi.CQRS.UnitTests.csproj -c Debug --nologo` and `dotnet build src/Genocs.WebApi.CQRS/Genocs.WebApi.CQRS.csproj -c Debug --nologo`.

---

## M2: Endpoint API and Host Integration Modernization

### WEBAPI-CQRS-004 Replace legacy pipeline mutation with endpoint-route mapping API

**Status**: Completed (May 2026)

**Priority**: P0

**Problem**

`UseDispatcherEndpoints(...)` mutates app middleware pipeline by calling `UseRouting`, `UseAuthorization`, and `UseEndpoints`, which conflicts with host-owned ordering and modern endpoint routing practices.

**Likely touch points**

- `src/Genocs.WebApi.CQRS/Extensions.cs`
- `src/Genocs.WebApi.CQRS/README_NUGET.md`
- `docs/Genocs.WebApi.CQRS-Agent-Documentation.md`

**Acceptance criteria**

- New endpoint-first API maps dispatcher endpoints via route builder semantics.
- Existing pipeline-mutating API is removed or marked obsolete with migration guidance.
- Documentation includes before/after migration examples.

**Dependencies**

- `WEBAPI-CQRS-001`
- `WEBAPI-CQRS-002`

**Implementation notes**

- Added endpoint-route mapping APIs in `src/Genocs.WebApi.CQRS/Extensions.cs`:
  - `MapDispatcherEndpoints(this IEndpointRouteBuilder, Action<IDispatcherEndpointsBuilder>)`
  - `MapDispatcherEndpoints(this IEndpointRouteBuilder, Func<IDispatcherEndpointsBuilder, IDispatcherEndpointsBuilder>)`
- Marked legacy `UseDispatcherEndpoints(...)` as obsolete with migration guidance and changed it to delegate to route-builder mapping on compatible hosts instead of configuring routing/authorization middleware itself.
- Migrated in-repo hosts from legacy API to route-builder mapping:
  - `src/demo/identities/WebApi/Program.cs`
  - `src/demo/notifications/WebApi/Program.cs`
  - `src/demo/orders/WebApi/Program.cs`
  - `src/demo/products/WebApi/Program.cs`
- Added regression coverage in `src/tests/Genocs.WebApi.CQRS.UnitTests/Extensions/MapDispatcherEndpointsExtensionsTests.cs` validating mapped CQRS routes update endpoint definitions via route-builder mapping.
- Updated migration guidance with before/after examples in:
  - `src/Genocs.WebApi.CQRS/README_NUGET.md`
  - `docs/Genocs.WebApi.CQRS-Agent-Documentation.md`
- Validation:
  - `dotnet test src/tests/Genocs.WebApi.CQRS.UnitTests/Genocs.WebApi.CQRS.UnitTests.csproj -c Debug --nologo`
  - `dotnet build src/Genocs.WebApi.CQRS/Genocs.WebApi.CQRS.csproj -c Debug --nologo`
  - `dotnet build src/demo/identities/WebApi/Host.csproj -c Debug --nologo`
  - `dotnet build src/demo/notifications/WebApi/Host.csproj -c Debug --nologo`
  - `dotnet build src/demo/orders/WebApi/Host.csproj -c Debug --nologo`
  - `dotnet build src/demo/products/WebApi/Host.csproj -c Debug --nologo`

### WEBAPI-CQRS-005 Normalize dispatch callback contracts and cancellation semantics

**Status**: Completed (May 2026)

**Priority**: P1

**Problem**

Current callback signatures allow nullable `HttpContext` while implementation behavior assumes runtime context availability; cancellation tokens are not propagated.

**Likely touch points**

- `src/Genocs.WebApi.CQRS/IDispatcherEndpointsBuilder.cs`
- `src/Genocs.WebApi.CQRS/Builders/DispatcherEndpointsBuilder.cs`
- `src/Genocs.WebApi.CQRS/Extensions.cs`
- `src/Genocs.WebApi.CQRS/IDispatcher.cs`
- `src/Genocs.WebApi.CQRS/InMemoryDispatcher.cs`

**Acceptance criteria**

- Public callback contracts use non-null `HttpContext`.
- Request cancellation (`HttpContext.RequestAborted`) flows into command/query/event dispatch.
- Null context fallback paths are removed.
- Unit tests cover cancellation propagation and contract behavior.

**Dependencies**

- `WEBAPI-CQRS-004`

**Implementation notes**

- Normalized public callback contracts to non-null context + explicit cancellation token in `src/Genocs.WebApi.CQRS/IDispatcherEndpointsBuilder.cs`:
  - `Func<HttpContext, CancellationToken, Task>`
  - `Func<T, HttpContext, CancellationToken, Task>`
  - `Func<TQuery, TResult?, HttpContext, CancellationToken, Task>`
- Updated `src/Genocs.WebApi.CQRS/Builders/DispatcherEndpointsBuilder.cs` to consistently propagate `HttpContext.RequestAborted` into:
  - command dispatch via `ICommandDispatcher.SendAsync(..., cancellationToken)`
  - query dispatch via `IQueryDispatcher.QueryAsync(..., cancellationToken)`
  - pre/post dispatch callbacks.
- Removed nullable-context fallback behavior in CQRS adapter paths by enforcing non-null context at the API boundary and throwing deterministic errors if underlying nullable adapters pass null.
- Updated helper extensions in `src/Genocs.WebApi.CQRS/Extensions.cs` so `HttpContext.SendAsync(...)` and `HttpContext.QueryAsync(...)` pass `RequestAborted` to the in-memory dispatcher.
- Migrated callback callsites in host applications to new signature shape `(payload, context, cancellationToken)`:
  - `src/demo/identities/WebApi/Program.cs`
  - `src/demo/notifications/WebApi/Program.cs`
  - `src/demo/orders/WebApi/Program.cs`
  - `src/demo/products/WebApi/Program.cs`
- Added and stabilized cancellation propagation unit coverage in `src/tests/Genocs.WebApi.CQRS.UnitTests/Builders/DispatcherEndpointsBuilderCancellationTests.cs` using deterministic endpoint delegate capture (no TestServer dependency).
- Validation:
  - `dotnet test src/tests/Genocs.WebApi.CQRS.UnitTests/Genocs.WebApi.CQRS.UnitTests.csproj -c Debug --nologo`
  - `dotnet build src/Genocs.WebApi.CQRS/Genocs.WebApi.CQRS.csproj -c Debug --nologo`
  - `dotnet build src/demo/identities/WebApi/Host.csproj -c Debug --nologo`
  - `dotnet build src/demo/notifications/WebApi/Host.csproj -c Debug --nologo`
  - `dotnet build src/demo/orders/WebApi/Host.csproj -c Debug --nologo`
  - `dotnet build src/demo/products/WebApi/Host.csproj -c Debug --nologo`
- Remaining analyzer warnings in `DispatcherEndpointsBuilder.cs` are SA1117 formatting-only warnings and are tracked under `WEBAPI-CQRS-006`.

---

## M3: Nullability, Analyzer, and Dependency Baseline

### WEBAPI-CQRS-006 Normalize nullability/analyzer baseline in touched files

**Status**: Completed (May 2026)

**Priority**: P1

**Problem**

CQRS touched files currently emit nullability/analyzer issues in strict mode, reducing signal-to-noise for future regressions.

**Likely touch points**

- `src/Genocs.WebApi.CQRS/Middlewares/PublicContractsMiddleware.cs`
- `src/Genocs.WebApi.CQRS/Builders/DispatcherEndpointsBuilder.cs`
- `src/Genocs.WebApi.CQRS/IDispatcherEndpointsBuilder.cs`

**Acceptance criteria**

- Nullability contracts align between interface and implementation for updated APIs.
- Current strict-mode CQRS-local warnings are eliminated in touched files.
- Analyzer/style violations in changed code paths are resolved.

**Dependencies**

- `WEBAPI-CQRS-005`

**Implementation notes**

- Resolved remaining CQRS-local analyzer blockers in `src/Genocs.WebApi.CQRS/Builders/DispatcherEndpointsBuilder.cs` by normalizing multi-parameter call formatting to satisfy `SA1117` (one parameter per line for mixed multiline invocations).
- Preserved the non-null callback contracts introduced in `WEBAPI-CQRS-005` and verified no new nullable fallback paths were introduced while reformatting.
- Confirmed touched CQRS files now build cleanly in normal mode and strict package-local mode.
- Validation:
  - `dotnet build src/Genocs.WebApi.CQRS/Genocs.WebApi.CQRS.csproj -c Debug --nologo`
  - `dotnet build src/Genocs.WebApi.CQRS/Genocs.WebApi.CQRS.csproj -c Debug --nologo -warnaserror -t:Rebuild -p:BuildProjectReferences=false`
  - `dotnet test src/tests/Genocs.WebApi.CQRS.UnitTests/Genocs.WebApi.CQRS.UnitTests.csproj -c Debug --nologo`
- Note: full strict rebuild with project references still fails due pre-existing nullability errors in upstream `Genocs.Core`; WEBAPI-CQRS touched files are now clean.

### WEBAPI-CQRS-007 Align Debug/Release dependency model for deterministic package behavior

**Status**: Completed (May 2026)

**Priority**: P1

**Problem**

Project currently uses configuration-specific dependency wiring (`ProjectReference` in Debug, pinned `PackageReference` in Release), increasing drift risk between local behavior and packaged artifact behavior.

**Likely touch points**

- `src/Genocs.WebApi.CQRS/Genocs.WebApi.CQRS.csproj`
- central version management files if needed (for example `Directory.Build.props`)

**Acceptance criteria**

- Dependency strategy is deterministic across configurations.
- Release package no longer depends on a stale/pinned version unless intentionally centralized.
- CI and local builds validate the same effective dependency graph.

**Dependencies**

- `WEBAPI-CQRS-006`

**Implementation notes**

- Unified `src/Genocs.WebApi.CQRS/Genocs.WebApi.CQRS.csproj` dependency strategy by removing configuration-specific reference blocks:
  - removed Debug-only `ProjectReference` condition,
  - removed Release-only pinned `PackageReference` (`Genocs.WebApi` `9.0.0-beta007`).
- Added a single unconditional `ProjectReference` to `src/Genocs.WebApi/Genocs.WebApi.csproj`, ensuring the same effective dependency graph for both Debug and Release builds.
- This removes configuration drift and eliminates package-version pinning from CQRS project-level configuration.
- Validation:
  - `dotnet build src/Genocs.WebApi.CQRS/Genocs.WebApi.CQRS.csproj -c Debug --nologo`
  - `dotnet build src/Genocs.WebApi.CQRS/Genocs.WebApi.CQRS.csproj -c Release --nologo`
  - `dotnet test src/tests/Genocs.WebApi.CQRS.UnitTests/Genocs.WebApi.CQRS.UnitTests.csproj -c Debug --nologo`
- Note: unit-test execution still surfaces pre-existing nullability warnings from upstream `Genocs.Core` during project-reference builds; CQRS project builds and tests pass with the unified model.

---

## M4: Testability and Quality Gates

### WEBAPI-CQRS-008 Add package-specific tests and introduce package-level quality gate

**Status**: Completed (May 2026)

**Priority**: P0

**Problem**

No dedicated package-level unit test project or package-specific validation target currently guards `Genocs.WebApi.CQRS` behavior.

**Likely touch points**

- `src/tests` (new `Genocs.WebApi.CQRS.UnitTests` project)
- `Makefile`
- new package validation target file (for example `validate-webapi-cqrs.mk`)
- `src/Genocs.WebApi.CQRS/README_NUGET.md`
- `docs/Genocs.WebApi.CQRS-Agent-Documentation.md`

**Acceptance criteria**

- New unit test project covers:
  - contracts endpoint async completion and output shape,
  - contract discovery duplicate/partial-load behavior,
  - endpoint mapping and dispatch callback flow,
  - cancellation propagation,
  - null-result query behavior policy.
- Introduce `make validate-webapi-cqrs` target that builds package and runs CQRS tests.
- Validation guidance is documented for maintainers.

**Dependencies**

- `WEBAPI-CQRS-001`
- `WEBAPI-CQRS-002`
- `WEBAPI-CQRS-003`
- `WEBAPI-CQRS-004`
- `WEBAPI-CQRS-005`
- `WEBAPI-CQRS-006`
- `WEBAPI-CQRS-007`

**Implementation notes**

- Established package-specific quality gate target:
  - added `validate-webapi-cqrs.mk` with:
    - `validate-webapi-cqrs-build` (`dotnet build ... -warnaserror -p:BuildProjectReferences=false` for CQRS-local warning baseline),
    - `validate-webapi-cqrs-tests` (CQRS unit tests),
    - aggregate `validate-webapi-cqrs` target.
  - wired root `Makefile` target:
    - `make validate-webapi-cqrs` -> `$(MAKE) -f validate-webapi-cqrs.mk validate-webapi-cqrs`.
- Confirmed package test project coverage now includes required areas:
  - contracts endpoint async completion and output shape,
  - duplicate-name and partial-load contract discovery behavior,
  - endpoint mapping behavior,
  - dispatch cancellation propagation,
  - null-result query policy (`404` default) via `Get_QueryWithoutAfterDispatch_Returns404WhenResultIsNull` in `src/tests/Genocs.WebApi.CQRS.UnitTests/Builders/DispatcherEndpointsBuilderCancellationTests.cs`.
- Added maintainer validation guidance in:
  - `src/Genocs.WebApi.CQRS/README_NUGET.md`
  - `docs/Genocs.WebApi.CQRS-Agent-Documentation.md`
- Validation:
  - `dotnet build src/Genocs.WebApi.CQRS/Genocs.WebApi.CQRS.csproj -f net10.0 -c Debug --nologo -warnaserror -p:BuildProjectReferences=false`
  - `dotnet test src/tests/Genocs.WebApi.CQRS.UnitTests/Genocs.WebApi.CQRS.UnitTests.csproj -c Debug --nologo`
- Note: this shell environment did not have GNU `make` installed, so the aggregate target was validated by running its underlying commands directly.

---

## Suggested First Execution Slice

1. `WEBAPI-CQRS-001`
2. `WEBAPI-CQRS-002`
3. `WEBAPI-CQRS-004`
4. `WEBAPI-CQRS-005`
5. `WEBAPI-CQRS-008`

This slice fixes the highest runtime and host-integration risks first while introducing early regression coverage for subsequent deep refactors.

## Validation Commands (Target State)

- `dotnet build src/Genocs.WebApi.CQRS/Genocs.WebApi.CQRS.csproj -c Debug --nologo`
- `dotnet test src/tests/Genocs.WebApi.CQRS.UnitTests/Genocs.WebApi.CQRS.UnitTests.csproj -c Debug --nologo`
- `make validate-webapi-cqrs`