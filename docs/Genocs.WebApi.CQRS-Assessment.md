# Genocs.WebApi.CQRS Assessment (May 2026 Refresh)

## Purpose

This assessment evaluates the current `Genocs.WebApi.CQRS` implementation against:

- `docs/Genocs.WebApi.CQRS-Implementation-Backlog.md`
- current package source in `src/Genocs.WebApi.CQRS`

Related planning document:

- `docs/Genocs.WebApi.CQRS-Implementation-Backlog.md`

The objective is to identify runtime risks, legacy design constraints, and modernization priorities for a deep upgrade where breaking changes are explicitly allowed.

## Assessment Scope

Reviewed areas:

- endpoint mapping and host-pipeline integration model
- command/query dispatch callback contracts
- middleware behavior for public contract discovery and serialization
- nullability/analyzer posture in package-local touched files
- package dependency/configuration consistency
- package-level test and quality-gate coverage

Primary files inspected include:

- `src/Genocs.WebApi.CQRS/Extensions.cs`
- `src/Genocs.WebApi.CQRS/IDispatcherEndpointsBuilder.cs`
- `src/Genocs.WebApi.CQRS/Builders/DispatcherEndpointsBuilder.cs`
- `src/Genocs.WebApi.CQRS/Middlewares/PublicContractsMiddleware.cs`
- `src/Genocs.WebApi.CQRS/IDispatcher.cs`
- `src/Genocs.WebApi.CQRS/InMemoryDispatcher.cs`
- `src/Genocs.WebApi.CQRS/Genocs.WebApi.CQRS.csproj`

## Validation Evidence

Executed on 2026-05-01:

- `dotnet build src/Genocs.WebApi.CQRS/Genocs.WebApi.CQRS.csproj -c Debug --nologo -warnaserror -t:Rebuild`
- `rg --files src/tests | rg "WebApi.CQRS|CQRS"`

Results:

- strict build: failed in workspace due to both upstream baseline warnings and CQRS-local findings
- CQRS-local strict findings confirmed:
  - `CS8600` nullability assignments in `PublicContractsMiddleware`
  - `SA1117` parameter formatting issue in `DispatcherEndpointsBuilder`
- test inventory: no dedicated package-level project under `src/tests` for `Genocs.WebApi.CQRS`

## Executive Summary

`Genocs.WebApi.CQRS` is functional but anchored to legacy composition patterns that now represent modernization risk. The highest concerns are runtime correctness in contracts middleware (non-awaited async write), process-wide static mutable state for contract caching/discovery, and host-pipeline mutation from package extensions.

The package is a strong candidate for a breaking-change cleanup pass with endpoint-first mapping APIs, deterministic contract discovery, aligned nullability contracts, and package-specific regression gates.

## Backlog Conformance Check

Status against `docs/Genocs.WebApi.CQRS-Implementation-Backlog.md`:

- `WEBAPI-CQRS-001` to `WEBAPI-CQRS-008`: Planned (not yet implemented)

Evidence highlights:

- `UseDispatcherEndpoints(...)` currently invokes `UseRouting`, optional `UseAuthorization`, and `UseEndpoints` internally.
- `PublicContractsMiddleware` retains static process-wide caches and one-time initialization guard.
- dispatch callbacks expose nullable `HttpContext` contracts while runtime code path assumes normal `HttpContext` availability.
- strict build surfaces CQRS-local nullability/style issues.

## Findings (Ordered by Severity)

### 1) Public contracts middleware does not await response body write

Severity: Critical

Where:

- `src/Genocs.WebApi.CQRS/Middlewares/PublicContractsMiddleware.cs`

Details:

- `InvokeAsync(...)` sets content type and calls `Response.WriteAsync(...)` without awaiting the returned task.

Impact:

- response completion can race payload write under load
- incomplete/truncated body risk and reduced error propagation reliability

Recommendation:

- implement `async` flow and await body write (`WEBAPI-CQRS-001`)

### 2) Contract discovery uses process-wide static mutable state

Severity: Critical

Where:

- `src/Genocs.WebApi.CQRS/Middlewares/PublicContractsMiddleware.cs`

Details:

- static `_initialized`, static `Contracts`, and static serialized payload are shared for the process lifetime
- initialization is effectively first-host-wins

Impact:

- cross-host leakage in tests/multi-host processes
- configuration drift depending on initialization order

Recommendation:

- move discovery/cache to host-scoped or options-scoped service and remove global mutable state (`WEBAPI-CQRS-002`)

### 3) Reflection-based contract loading lacks resilient failure policy

Severity: High

Where:

- `src/Genocs.WebApi.CQRS/Middlewares/PublicContractsMiddleware.cs`

Details:

- assembly scanning uses broad reflection and simple-name dictionary keys
- duplicate names trigger hard exceptions
- partial type-load scenarios are not explicitly handled

Impact:

- startup/runtime fragility in large solutions
- collision failures across contracts with same class name in different namespaces

Recommendation:

- add resilient type loading and explicit duplicate-name policy (`WEBAPI-CQRS-003`)

### 4) Package extension mutates host middleware pipeline

Severity: High

Where:

- `src/Genocs.WebApi.CQRS/Extensions.cs`

Details:

- `UseDispatcherEndpoints(...)` injects routing/auth/endpoints middleware directly

Impact:

- brittle ordering with host-defined middleware
- harder migration to endpoint-route-first hosting models

Recommendation:

- replace with endpoint-route mapping API and host-owned pipeline control (`WEBAPI-CQRS-004`)

### 5) Dispatch callback contracts are nullable-inconsistent and cancellation is not propagated

Severity: High

Where:

- `src/Genocs.WebApi.CQRS/IDispatcherEndpointsBuilder.cs`
- `src/Genocs.WebApi.CQRS/Builders/DispatcherEndpointsBuilder.cs`
- `src/Genocs.WebApi.CQRS/Extensions.cs`
- `src/Genocs.WebApi.CQRS/IDispatcher.cs`

Details:

- public callback signatures allow `HttpContext?`
- implementation largely assumes active request context
- dispatch paths do not consistently pass `HttpContext.RequestAborted`

Impact:

- contract ambiguity and warning noise
- less predictable cancellation behavior under aborted requests

Recommendation:

- normalize contracts to non-null context and flow cancellation end-to-end (`WEBAPI-CQRS-005`)

### 6) Debug/Release dependency model is configuration-dependent

Severity: Medium

Where:

- `src/Genocs.WebApi.CQRS/Genocs.WebApi.CQRS.csproj`

Details:

- Debug uses `ProjectReference` to `Genocs.WebApi`
- Release uses pinned `PackageReference` (`9.0.0-beta007`)

Impact:

- behavioral drift risk between local and packaged builds
- increased upgrade uncertainty while modernizing dependencies

Recommendation:

- unify dependency model and centralize versioning policy (`WEBAPI-CQRS-007`)

### 7) Package-local strict warnings remain unresolved in touched files

Severity: Medium

Where:

- `src/Genocs.WebApi.CQRS/Middlewares/PublicContractsMiddleware.cs`
- `src/Genocs.WebApi.CQRS/Builders/DispatcherEndpointsBuilder.cs`

Details:

- strict build reports CQRS-local nullability/style findings

Impact:

- reduced signal-to-noise for regression detection

Recommendation:

- complete nullability/analyzer normalization in touched files (`WEBAPI-CQRS-006`)

### 8) No dedicated package-level tests or validation gate

Severity: Medium

Where:

- `src/tests`
- `Makefile`

Details:

- no `Genocs.WebApi.CQRS` unit test project found
- no package-level quality target analogous to other package validators

Impact:

- elevated regression risk for breaking refactors

Recommendation:

- add package-specific tests and `validate-webapi-cqrs` target (`WEBAPI-CQRS-008`)

## Dependency Impact Notes

`Genocs.WebApi.CQRS` is a bridge package over `Genocs.WebApi` and Core CQRS abstractions. Changes to endpoint mapping semantics and callback contracts are likely to affect:

- host startup/wiring code in app projects using `UseDispatcherEndpoints(...)`
- package documentation examples and agent guidance
- downstream service templates that assume current null-context callback forms

A staged migration should provide explicit before/after API usage examples and temporary obsoletions where feasible.

## Quality and Test Posture

Current posture is limited for deep refactoring:

- no package-specific unit-test project detected
- strict build signal is partially obscured by upstream baseline warnings

Recommended target posture:

- dedicated `Genocs.WebApi.CQRS.UnitTests` project
- package-scoped validation target (`make validate-webapi-cqrs`)
- focused tests for middleware correctness, discovery determinism, endpoint mapping semantics, and cancellation propagation

## Recommended Next Slice

1. Execute `WEBAPI-CQRS-001` and `WEBAPI-CQRS-002` to remove highest runtime correctness/process-state risks.
2. Execute `WEBAPI-CQRS-004` and `WEBAPI-CQRS-005` to modernize endpoint integration and callback contracts.
3. Add `WEBAPI-CQRS-008` early for regression safety before broader contract/discovery hardening.
4. Complete `WEBAPI-CQRS-006` and `WEBAPI-CQRS-007` to stabilize warning and dependency baselines.

## Conclusion

`Genocs.WebApi.CQRS` remains viable but has clear legacy design debt that now limits safe evolution. The newly defined backlog is appropriate for a breaking-change modernization cycle, and the package should prioritize middleware correctness, deterministic discovery, and host-friendly endpoint mapping to establish a stable vNext foundation.