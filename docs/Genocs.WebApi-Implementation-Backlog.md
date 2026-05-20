# Genocs.WebApi Implementation Backlog

## Purpose

This backlog converts the current `Genocs.WebApi` assessment findings into issue-sized implementation work ordered by runtime risk, security impact, and long-term maintainability.

The backlog is structured for phased delivery similar to existing package backlogs.

## Current Status

Observed baseline (April 2026):

- `Genocs.WebApi` builds for `net10.0`, `net9.0`, and `net8.0`.
- Package has warning-heavy baseline in normal builds (23 warnings per target framework).
- Strict rebuild with warnings as errors fails due to nullability and analyzer violations.
- No dedicated package-level unit test project for `Genocs.WebApi` was found under `src/tests`.
- A runtime-throwing options configurator remains in package source.

Latest validation runs:

- `dotnet build src/Genocs.WebApi/Genocs.WebApi.csproj -c Debug --nologo`
- `dotnet build src/Genocs.WebApi/Genocs.WebApi.csproj -c Debug --nologo -warnaserror -t:Rebuild`

Builds succeed in normal mode, but strict mode fails due to warning baseline.

## Planning Assumptions

- Fix runtime correctness and security-sensitive defaults before API expansion.
- Reduce warning noise to recover signal-to-noise for future regressions.
- Align nullability contracts between public interfaces and implementations.
- Pair behavior changes with focused package-level tests.
- Keep package README and agent documentation synchronized with delivered behavior.

## Milestones

| Milestone | Goal | Tasks |
|---|---|---|
| M1 | Runtime correctness and safety baseline | WEBAPI-001 to WEBAPI-003 |
| M2 | Endpoint DSL and binding contract hardening | WEBAPI-004 to WEBAPI-005 |
| M3 | Nullability and analyzer baseline reduction | WEBAPI-006 |
| M4 | Testability and quality gates | WEBAPI-007 |

## Execution Order

1. Complete M1 before introducing new endpoint features.
2. Complete M2 before broad host adoption updates.
3. Complete M3 before enforcing package-level warning gates.
4. Complete M4 to lock in long-term maintainability.

---

## M1: Runtime Correctness and Safety Baseline

### WEBAPI-001 Implement or remove runtime-throwing WebApi options configurator

**Status**: Completed (April 2026)

**Priority**: P0

**Problem**

`WebApiConfigureOptions.Configure(WebApiOptions)` throws `NotImplementedException`, which can fail startup/options paths at runtime.

**Likely touch points**

- `src/Genocs.WebApi/Configurations/WebApiConfigureOptions.cs`
- `src/Genocs.WebApi/Extensions.cs`

**Acceptance criteria**

- No `NotImplementedException` remains in options configuration path.
- `WebApiOptions` has deterministic defaults and safe binding behavior.
- Unit tests validate options configuration behavior.

**Dependencies**

- none

**Implementation notes**

- Replaced the `NotImplementedException` in `WebApiConfigureOptions.Configure(WebApiOptions)` with a safe null-guard and deterministic no-mutation behavior in `src/Genocs.WebApi/Configurations/WebApiConfigureOptions.cs`.
- Preserved constructor compatibility by keeping a parameterless constructor and an `IOptions<WebApiOptions>` overload.
- Added focused unit coverage in `src/tests/Genocs.WebApi.UnitTests/Configurations/WebApiConfigureOptionsTests.cs`.
- Added the new test project `src/tests/Genocs.WebApi.UnitTests/Genocs.WebApi.UnitTests.csproj` and included it in `genocs.slnx`.
- Validation: `dotnet build src/Genocs.WebApi/Genocs.WebApi.csproj -c Debug --nologo` and `dotnet test src/tests/Genocs.WebApi.UnitTests/Genocs.WebApi.UnitTests.csproj -c Debug --nologo`.

### WEBAPI-002 Correct exception fallback semantics in middleware

**Status**: Completed (April 2026)

**Priority**: P0

**Problem**

`ErrorHandlerMiddleware` falls back to `400 BadRequest` when no exception mapping is provided, misclassifying unhandled server exceptions.

**Likely touch points**

- `src/Genocs.WebApi/Exceptions/ErrorHandlerMiddleware.cs`
- `src/Genocs.WebApi/Exceptions/IExceptionToResponseMapper.cs`

**Acceptance criteria**

- Unmapped exceptions default to `500 InternalServerError`.
- Mapped exceptions continue honoring mapper-provided status code and payload.
- Unit tests cover mapped and unmapped exception paths.

**Dependencies**

- none

**Implementation notes**

- Updated `ErrorHandlerMiddleware` fallback status in `src/Genocs.WebApi/Exceptions/ErrorHandlerMiddleware.cs` from `400 BadRequest` to `500 InternalServerError` when mapper returns `null`.
- Added focused middleware tests in `src/tests/Genocs.WebApi.UnitTests/Exceptions/ErrorHandlerMiddlewareTests.cs` for:
  - mapped exception path preserving mapper status and payload serialization,
  - unmapped exception path returning `500` with empty body.
- Validation: `dotnet build src/Genocs.WebApi/Genocs.WebApi.csproj -c Debug --nologo` and `dotnet test src/tests/Genocs.WebApi.UnitTests/Genocs.WebApi.UnitTests.csproj -c Debug --nologo`.

### WEBAPI-003 Make forwarded-header trust model safe by default

**Status**: Completed (April 2026)

**Priority**: P0

**Problem**

`UseAllForwardedHeaders` clears trusted proxy/network lists by default when `resetKnownNetworksAndProxies=true`, which can enable unsafe trust behavior on untrusted edges.

**Likely touch points**

- `src/Genocs.WebApi/Extensions.cs`
- `src/Genocs.WebApi/README_NUGET.md`
- `docs/Genocs.WebApi-Agent-Documentation.md`

**Acceptance criteria**

- Safe default behavior does not blindly trust all forwarders.
- Permissive trust mode is explicit opt-in and documented with deployment guidance.
- Tests or host-level validation examples cover trusted-proxy configuration.

**Dependencies**

- none

**Implementation notes**

- Updated `UseAllForwardedHeaders(...)` in `src/Genocs.WebApi/Extensions.cs` to default `resetKnownNetworksAndProxies` to `false`, preserving ASP.NET Core trusted proxy/network checks by default.
- Kept permissive trust behavior as explicit opt-in via `UseAllForwardedHeaders(resetKnownNetworksAndProxies: true)`.
- Added behavior tests in `src/tests/Genocs.WebApi.UnitTests/Extensions/ForwardedHeadersExtensionsTests.cs` covering strict default mode and permissive opt-in mode.
- Updated package and agent documentation in `src/Genocs.WebApi/README_NUGET.md` and `docs/Genocs.WebApi-Agent-Documentation.md` to document strict defaults and explicit opt-in guidance.
- Validation: `dotnet test src/tests/Genocs.WebApi.UnitTests/Genocs.WebApi.UnitTests.csproj -c Debug --nologo` and `dotnet build src/Genocs.WebApi/Genocs.WebApi.csproj -c Debug --nologo`.

---

## M2: Endpoint DSL and Binding Contract Hardening

### WEBAPI-004 Remove implicit anonymous access metadata from endpoint defaults

**Status**: Completed (April 2026)

**Priority**: P1

**Problem**

`EndpointsBuilder.ApplyAuthRolesAndPolicies(...)` applies `AllowAnonymous()` when no auth metadata is passed, which can override host fallback policies and increase accidental exposure risk.

**Likely touch points**

- `src/Genocs.WebApi/EndpointsBuilder.cs`
- `src/Genocs.WebApi/IEndpointsBuilder.cs`
- `src/Genocs.WebApi/README_NUGET.md`

**Acceptance criteria**

- Endpoints do not receive explicit anonymous metadata unless requested.
- Role, policy, and auth behavior remains unchanged when explicitly configured.
- Unit tests cover no-auth metadata, explicit auth, explicit roles, and explicit policies.

**Dependencies**

- `WEBAPI-002`

**Implementation notes**

- Updated authorization metadata flow in `src/Genocs.WebApi/EndpointsBuilder.cs` by removing implicit `AllowAnonymous()` assignment when `auth`, `roles`, and `policies` are not provided.
- Kept explicit authorization behavior unchanged for:
  - `auth: true` (`RequireAuthorization()`),
  - `roles` (`RequireAuthorization(new AuthorizeAttribute { Roles = ... })`),
  - `policies` (`RequireAuthorization(policyNames)`).
- Added focused metadata tests in `src/tests/Genocs.WebApi.UnitTests/Endpoints/EndpointsBuilderAuthorizationMetadataTests.cs` validating:
  - default no-auth mapping has no anonymous or authorization metadata,
  - explicit auth, roles, and policies continue producing expected authorization metadata.
- Updated package docs in `src/Genocs.WebApi/README_NUGET.md` to state default metadata behavior and explicit anonymous opt-in through endpoint convention callbacks.
- Validation: `dotnet test src/tests/Genocs.WebApi.UnitTests/Genocs.WebApi.UnitTests.csproj -c Debug --nologo` and `dotnet build src/Genocs.WebApi/Genocs.WebApi.csproj -c Debug --nologo`.

### WEBAPI-005 Remove static cross-host binding state from request pipeline helpers

**Status**: Completed (April 2026)

**Priority**: P1

**Problem**

`Extensions` stores `BindRequestFromRoute` in static mutable field `_bindRequestFromRoute`, which can leak behavior across hosts/tests and reduce determinism.

**Likely touch points**

- `src/Genocs.WebApi/Extensions.cs`
- `src/Genocs.WebApi/Configurations/WebApiOptions.cs`

**Acceptance criteria**

- Request binding behavior reads from options/configuration per host scope instead of static state.
- Multi-host test runs are deterministic.
- Unit tests validate behavior for enabled and disabled route-to-body merge.

**Dependencies**

- `WEBAPI-001`

**Implementation notes**

- Removed static mutable route-binding state (`_bindRequestFromRoute`) from `src/Genocs.WebApi/Extensions.cs`.
- Updated `ReadJsonAsync<T>(...)` to resolve `WebApiOptions` from `HttpContext.RequestServices` per request and derive route-binding behavior from the current host scope.
- Kept existing route merge behavior intact when `BindRequestFromRoute` is enabled.
- Added focused tests in `src/tests/Genocs.WebApi.UnitTests/Extensions/ReadJsonAsyncRouteBindingTests.cs` covering:
  - route merge enabled,
  - route merge disabled,
  - per-host determinism with separate service providers to prevent cross-host leakage.
- Validation: `dotnet test src/tests/Genocs.WebApi.UnitTests/Genocs.WebApi.UnitTests.csproj -c Debug --nologo` and `dotnet build src/Genocs.WebApi/Genocs.WebApi.csproj -c Debug --nologo`.

---

## M3: Nullability and Analyzer Baseline Reduction

### WEBAPI-006 Normalize nullability contracts and resolve analyzer issues in touched files

**Status**: Completed (April 2026)

**Priority**: P1

**Problem**

`Genocs.WebApi` emits recurring nullability/analyzer warnings, including contract mismatches (`IEndpointsBuilder` vs `EndpointsBuilder`) and ASP.NET header mutation analyzer violations.

**Likely touch points**

- `src/Genocs.WebApi/IEndpointsBuilder.cs`
- `src/Genocs.WebApi/EndpointsBuilder.cs`
- `src/Genocs.WebApi/Extensions.cs`
- `src/Genocs.WebApi/WebApiEndpointDefinition.cs`
- `src/Genocs.WebApi/GenocsFormatterResolver.cs`
- `src/Genocs.WebApi/Parsers/JsonParser.cs`

**Acceptance criteria**

- Nullability contracts align between interfaces and implementations in touched APIs.
- Response header helpers use indexer/append semantics (`ASP0019` resolved).
- Package warning count is reduced materially and tracked in backlog notes.

**Dependencies**

- `WEBAPI-004`
- `WEBAPI-005`

**Implementation notes**

- Normalized endpoint delegate return behavior in `src/Genocs.WebApi/EndpointsBuilder.cs` so verb mapping callbacks always return a non-null `Task` when no handler delegate is provided.
- Aligned formatter resolver nullability contract in `src/Genocs.WebApi/GenocsFormatterResolver.cs` by adding deterministic fallback/exception behavior for missing formatters.
- Updated parser nullability in `src/Genocs.WebApi/Parsers/JsonParser.cs` (`Dictionary<string, string?>` and parse contract) to remove null-assignment warnings for empty JSON nodes.
- Hardened null handling in `src/Genocs.WebApi/Extensions.cs` across binding/query parsing/validation paths and replaced response header `Add` usage with indexer assignment in creation helpers to resolve analyzer guidance.
- Existing nullable endpoint definition members in `src/Genocs.WebApi/WebApiEndpointDefinition.cs` remain aligned with current usage and no longer emit initialization warnings.
- Validation result: `dotnet build src/Genocs.WebApi/Genocs.WebApi.csproj -c Debug --nologo` now completes warning-free for `Genocs.WebApi` targets.
- Validation commands: `dotnet build src/Genocs.WebApi/Genocs.WebApi.csproj -c Debug --nologo` and `dotnet test src/tests/Genocs.WebApi.UnitTests/Genocs.WebApi.UnitTests.csproj -c Debug --nologo`.

---

## M4: Testability and Quality Gates

### WEBAPI-007 Add package-specific tests and introduce package-level quality gate

**Status**: Completed (April 2026)

**Priority**: P0

**Problem**

`Genocs.WebApi` lacks a dedicated package-level test project, and there is no package-specific validation target analogous to `validate-messaging`.

**Likely touch points**

- `src/tests` (new `Genocs.WebApi.UnitTests` project)
- `Makefile`
- new package validation make target file (for example `validate-webapi.mk`)
- `src/Genocs.WebApi/README_NUGET.md`
- `docs/Genocs.WebApi-Agent-Documentation.md`

**Acceptance criteria**

- New unit test project covers:
  - error middleware mapped/unmapped behavior,
  - endpoint auth metadata behavior,
  - request binding merge behavior,
  - response helper header behavior.
- Introduce `make validate-webapi` target that builds package and runs WebApi tests.
- Validation guidance is documented for maintainers.

**Dependencies**

- `WEBAPI-002`
- `WEBAPI-003`
- `WEBAPI-004`
- `WEBAPI-005`
- `WEBAPI-006`

**Implementation notes**

- Added package-level quality gate file `validate-webapi.mk` with:
  - `validate-webapi-build`: warning-as-error build for `src/Genocs.WebApi/Genocs.WebApi.csproj` on `net10.0`,
  - `validate-webapi-tests`: regression test execution for `src/tests/Genocs.WebApi.UnitTests/Genocs.WebApi.UnitTests.csproj`.
- Added root Make target in `Makefile`: `make validate-webapi`.
- Added/expanded package-specific tests under `src/tests/Genocs.WebApi.UnitTests` to cover:
  - error middleware mapped/unmapped behavior,
  - endpoint authorization metadata behavior,
  - request binding route merge behavior,
  - forwarded headers trust model behavior.
- Documented validation command in `src/Genocs.WebApi/README_NUGET.md` and `docs/Genocs.WebApi-Agent-Documentation.md`.
- Validation: `make validate-webapi`.

---

## Suggested First Execution Slice

1. `WEBAPI-001`
2. `WEBAPI-002`
3. `WEBAPI-003`
4. `WEBAPI-004`
5. `WEBAPI-007`

This slice addresses the highest runtime and security risks first while introducing early regression coverage.

## Validation Commands (Target State)

- `dotnet build src/Genocs.WebApi/Genocs.WebApi.csproj -c Debug --nologo`
- `dotnet test src/tests/Genocs.WebApi.UnitTests/Genocs.WebApi.UnitTests.csproj -c Debug --nologo`
- `make validate-webapi`
