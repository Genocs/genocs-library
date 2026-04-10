# Genocs.Logging Implementation Backlog

## Purpose

This backlog translates the observed architectural and quality concerns in `Genocs.Logging` into issue-sized implementation work.

The backlog follows the same execution-oriented model used by `Genocs.Common` and is ordered by delivery risk, not namespace.

## Current Status

Observed baseline (April 2026):

- `Genocs.Logging` builds successfully for `net10.0`, `net9.0`, and `net8.0`.
- M1 (`LOGGING-001` to `LOGGING-004`) implemented and validated as of April 2026.
- M2 (`LOGGING-005` to `LOGGING-009`) implemented and validated as of April 2026.
- M3 `LOGGING-010` implemented and validated as of April 2026.
- M3 `LOGGING-011` and `LOGGING-012` implemented and validated as of April 2026.
- M4 `LOGGING-014` implemented and validated as of April 2026.
- `Genocs.Logging.UnitTests` has 25 passing tests covering CQRS decorator registration, host startup guards, middleware payload timing/limits/no-buffering semantics, sink safety, and level endpoint parsing (April 2026).
- M4 `LOGGING-015` implemented and validated as of April 2026.
- M4 `LOGGING-016` implemented and validated as of April 2026.
- `Genocs.Logging.IntegrationTests` has 3 host-level passing tests covering `MapLogLevelHandler`, `UseLogging`, and correlation middleware request/response flows (April 2026).
- `InternalsVisibleTo` is configured so the test project can access and test `internal` extension methods.

Next recommended items:

- Continue with M3 correlation and payload capture robustness (`LOGGING-013`) and M4 migration/quality tasks (`LOGGING-017` to `LOGGING-018`).

## Planning Assumptions

- Fix correctness and reliability issues before expanding package surface area.
- Keep `Genocs.Logging` focused on Serilog host integration, correlation middleware, and CQRS logging decorators.
- Treat changes in logging behavior as observable runtime changes and document migration impact explicitly.
- Pair each behavior change with focused tests before broad downstream adoption.
- Keep tracing/metrics responsibilities in `Genocs.Telemetry`; avoid scope overlap.

## Milestones

| Milestone | Goal | Tasks |
|---|---|---|
| M1 | Warning-clean and decorator correctness baseline | `LOGGING-001` to `LOGGING-004` |
| M2 | Runtime safety and config behavior hardening | `LOGGING-005` to `LOGGING-009` |
| M3 | Correlation/payload capture robustness and performance | `LOGGING-010` to `LOGGING-013` |
| M4 | Testability, contract polish, and adoption readiness | `LOGGING-014` to `LOGGING-018` |

## Execution Order

1. Complete M1 before changing sink behavior or middleware semantics.
2. Complete M2 before enabling broader production adoption guidance.
3. Complete M3 before recommending HTTP payload capture in high-traffic services.
4. Use M4 to lock in quality gates and migration guidance.

---

## M1: Warning-Clean and Decorator Correctness Baseline

### `LOGGING-001` Eliminate nullable hazard in CQRS decorator registration

**Status**: Implemented & validated (April 2026)

**Priority**: P0

**Problem**

`CQRS/Extensions.cs` currently triggers nullable warning `CS8604` when building decorator types from potentially null interface metadata.

**Scope**

- make handler interface selection explicit and null-safe
- avoid constructing generic decorator types with potentially null generic arguments
- keep behavior unchanged for valid handler registrations

**Likely touch points**

- [src/Genocs.Logging/CQRS/Extensions.cs](src/Genocs.Logging/CQRS/Extensions.cs)

**Acceptance criteria**

- `CS8604` warning is removed for `Genocs.Logging`
- valid command and event handlers remain decoratable

**Validation**

- `dotnet build src/Genocs.Logging/Genocs.Logging.csproj -c Debug --nologo`

**Dependencies**

- none

### `LOGGING-002` Replace brittle reflection-based `TryDecorate` discovery

**Status**: Implemented & validated (April 2026)

**Priority**: P0

**Problem**

Decorator registration discovers extension methods via broad reflection against Scrutor internals, which is brittle and harder to reason about across package/runtime updates.

**Scope**

- replace broad reflection scan with deterministic decorator registration path
- preserve current behavior for command and event handler decoration
- fail predictably when no matching handlers are found

**Likely touch points**

- [src/Genocs.Logging/CQRS/Extensions.cs](src/Genocs.Logging/CQRS/Extensions.cs)
- [src/Genocs.Logging/Genocs.Logging.csproj](src/Genocs.Logging/Genocs.Logging.csproj)

**Acceptance criteria**

- decorator registration no longer depends on best-effort extension-method discovery
- registration behavior is deterministic and testable

**Dependencies**

- `LOGGING-001`

### `LOGGING-003` Stabilize handler assembly selection semantics

**Status**: Implemented & documented (April 2026)

**Priority**: P1

**Problem**

Defaulting to `Assembly.GetCallingAssembly()` can be fragile in wrapper/inlined call paths and can cause missing decorator application in some hosting scenarios.

**Scope**

- define explicit assembly-selection behavior when `assembly` is not provided
- document expected usage for host applications
- add coverage for default and explicit assembly flows

**Likely touch points**

- [src/Genocs.Logging/CQRS/Extensions.cs](src/Genocs.Logging/CQRS/Extensions.cs)
- [src/Genocs.Logging/README_NUGET.md](src/Genocs.Logging/README_NUGET.md)

**Acceptance criteria**

- handler decoration works reliably with both default and explicit assembly input
- docs describe the safe default and override behavior

**Dependencies**

- `LOGGING-002`

### `LOGGING-004` Normalize analyzer/style baseline for Logging

**Status**: Implemented & validated (April 2026)

**Priority**: P1

**Problem**

Current build emits StyleCop warnings (`SA1116`, `SA1028`) in logging sources, reducing warning signal quality.

**Scope**

- fix current formatting/style warnings in `Genocs.Logging`
- ensure warning-clean baseline for touched files
- align with repository analyzer expectations

**Likely touch points**

- [src/Genocs.Logging/CQRS/Extensions.cs](src/Genocs.Logging/CQRS/Extensions.cs)
- [src/Genocs.Logging/Configurations/LokiOptions.cs](src/Genocs.Logging/Configurations/LokiOptions.cs)

**Acceptance criteria**

- `Genocs.Logging` builds warning-clean, or documented exceptions exist

**Validation**

- `dotnet build src/Genocs.Logging/Genocs.Logging.csproj -c Debug --nologo`

**Dependencies**

- none

---

## M2: Runtime Safety and Config Behavior Hardening

### `LOGGING-005` Guard sink configuration for enabled-but-invalid endpoints

**Status**: Implemented & validated (April 2026)

**Priority**: P1

**Problem**

Some sink paths can dereference null or invalid URLs when a sink is enabled but required endpoint settings are missing.

**Scope**

- add explicit validation/guards for Seq and Loki endpoint configuration
- skip sink wiring safely with clear diagnostics when configuration is invalid
- avoid runtime null-forgiving assumptions in sink setup

**Likely touch points**

- [src/Genocs.Logging/Extensions.cs](src/Genocs.Logging/Extensions.cs)
- [src/Genocs.Logging/Configurations/SeqOptions.cs](src/Genocs.Logging/Configurations/SeqOptions.cs)
- [src/Genocs.Logging/Configurations/LokiOptions.cs](src/Genocs.Logging/Configurations/LokiOptions.cs)

**Acceptance criteria**

- invalid enabled sink config does not crash host startup
- sink skip behavior is predictable and documented

**Dependencies**

- `LOGGING-004`

### `LOGGING-006` Decide and enforce `logger.enabled` semantics

**Status**: Implemented & validated (April 2026)

**Priority**: P1

**Problem**

`LoggerOptions.Enabled` exists but is not currently used to gate host logger setup, creating configuration ambiguity.

**Scope**

- decide whether `logger.enabled` is authoritative, deprecated, or removed
- implement chosen behavior consistently
- document behavior and migration guidance

**Likely touch points**

- [src/Genocs.Logging/Configurations/LoggerOptions.cs](src/Genocs.Logging/Configurations/LoggerOptions.cs)
- [src/Genocs.Logging/Extensions.cs](src/Genocs.Logging/Extensions.cs)
- [docs/Genocs.Logging-Agent-Documentation.md](docs/Genocs.Logging-Agent-Documentation.md)

**Acceptance criteria**

- no ambiguous enabled/disabled behavior remains for top-level logging setup
- consumers can rely on one documented model

**Dependencies**

- none

### `LOGGING-007` Clarify or remove non-implemented `mongo.enabled` settings

**Status**: Implemented & validated (April 2026)

**Priority**: P2

**Problem**

Mongo logging options were present in the public model but no Mongo sink wiring is implemented in this package, which could mislead consumers.

**Scope**

- remove unused Mongo logging options from the package configuration model
- align docs and options model with implementation reality
- avoid implied sink support that does not exist

**Likely touch points**

- [src/Genocs.Logging/Configurations/LoggerOptions.cs](src/Genocs.Logging/Configurations/LoggerOptions.cs)
- [docs/Genocs.Logging-Agent-Documentation.md](docs/Genocs.Logging-Agent-Documentation.md)

**Acceptance criteria**

- no unused Mongo logging option remains in `Genocs.Logging`

**Dependencies**

- none

### `LOGGING-008` Harden runtime log-level endpoint contract

**Status**: Implemented & validated (April 2026)

**Priority**: P2

**Problem**

`MapLogLevelHandler` currently accepts query-string input with minimal contract shape and no explicit response payload, making client automation and troubleshooting harder.

**Scope**

- formalize input contract and response shape for level-switch endpoint
- include explicit invalid-level behavior and response diagnostics
- keep in-process level-switch behavior backward-compatible where possible

**Likely touch points**

- [src/Genocs.Logging/Extensions.cs](src/Genocs.Logging/Extensions.cs)
- [src/Genocs.Logging/README_NUGET.md](src/Genocs.Logging/README_NUGET.md)

**Acceptance criteria**

- endpoint behavior is documented and machine-consumable
- invalid input paths are explicit and test-covered

**Dependencies**

- none

### `LOGGING-009` Make exclusion matching semantics explicit and testable

**Status**: Implemented & validated (April 2026)

**Priority**: P2

**Problem**

Path/property exclusion behavior is currently implementation-driven and can be surprising for partial path or property matching cases.

**Scope**

- define exact matching rules for `excludePaths` and `excludeProperties`
- align implementation and documentation with those rules
- add focused tests for expected and edge-case filters

**Likely touch points**

- [src/Genocs.Logging/Extensions.cs](src/Genocs.Logging/Extensions.cs)
- [src/Genocs.Logging/Configurations/LoggerOptions.cs](src/Genocs.Logging/Configurations/LoggerOptions.cs)

**Acceptance criteria**

- exclusion matching has one documented and test-verified behavior model

**Dependencies**

- none

---

## M3: Correlation and Payload Capture Robustness

### `LOGGING-010` Fix response payload correlation scope timing

**Status**: Implemented & validated (April 2026)

**Priority**: P1

**Problem**

Response body payload is captured after `next(context)` returns, so it is not reliably available to downstream logs emitted during request execution.

**Scope**

- define intended semantics for response payload enrichment (activity tags vs scope state)
- ensure behavior is consistent with log emission timing
- preserve streaming/body copy safety in middleware

**Likely touch points**

- [src/Genocs.Logging/CorrelationContextLoggingMiddleware.cs](src/Genocs.Logging/CorrelationContextLoggingMiddleware.cs)
- [docs/Genocs.Logging-Agent-Documentation.md](docs/Genocs.Logging-Agent-Documentation.md)

**Acceptance criteria**

- response payload behavior is deterministic and documented
- middleware does not imply unavailable scope data during pipeline execution

**Dependencies**

- none

### `LOGGING-011` Add payload size/content-type safety limits and policy docs

**Status**: Implemented & validated (April 2026)

**Priority**: P2

**Problem**

Payload capture can create security/performance risk without explicit policy guidance for large or sensitive content.

**Scope**

- validate and normalize `maxBodyLength` handling
- tighten content-type matching semantics where needed
- document secure defaults and production guidance

**Likely touch points**

- [src/Genocs.Logging/Configurations/HttpPayloadOptions.cs](src/Genocs.Logging/Configurations/HttpPayloadOptions.cs)
- [src/Genocs.Logging/CorrelationContextLoggingMiddleware.cs](src/Genocs.Logging/CorrelationContextLoggingMiddleware.cs)
- [src/Genocs.Logging/README_NUGET.md](src/Genocs.Logging/README_NUGET.md)

**Acceptance criteria**

- payload capture behavior is bounded and explicitly documented
- high-risk defaults are avoided

**Dependencies**

- `LOGGING-010`

### `LOGGING-012` Prevent unnecessary buffering when payload capture is disabled

**Status**: Implemented & validated (April 2026)

**Priority**: P2

**Problem**

Correlation middleware should impose near-zero overhead when payload capture features are disabled.

**Scope**

- verify no request/response buffering paths execute when capture is disabled
- document expected overhead model
- add micro-level regression tests for disabled capture paths

**Likely touch points**

- [src/Genocs.Logging/CorrelationContextLoggingMiddleware.cs](src/Genocs.Logging/CorrelationContextLoggingMiddleware.cs)

**Acceptance criteria**

- disabled payload capture avoids buffering work
- behavior is validated with focused tests

**Dependencies**

- none

### `LOGGING-013` Ensure correlation baggage enrichment remains safe and bounded

**Status**: Not started

**Priority**: P3

**Problem**

Activity baggage can grow unexpectedly; unbounded enrichment may bloat log events in high-throughput scenarios.

**Scope**

- define guardrails for baggage key/value ingestion
- avoid pathological growth in scope payload
- document recommended baggage usage limits for consumers

**Likely touch points**

- [src/Genocs.Logging/CorrelationContextLoggingMiddleware.cs](src/Genocs.Logging/CorrelationContextLoggingMiddleware.cs)
- [docs/Genocs.Logging-Agent-Documentation.md](docs/Genocs.Logging-Agent-Documentation.md)

**Acceptance criteria**

- baggage enrichment behavior is bounded and predictable

**Dependencies**

- none

---

## M4: Testability, Contract Polish, and Adoption Readiness

### `LOGGING-014` Introduce dedicated `Genocs.Logging` unit test project

**Status**: Implemented & validated (April 2026)

**Priority**: P0

**Problem**

There is currently no dedicated logging test project, limiting confidence in sink wiring, middleware behavior, and decorator registration.

**Scope**

- add `Genocs.Logging.UnitTests` project under `src/tests`
- include tests for host setup, level switch behavior, middleware payload capture, and CQRS decorators
- integrate project in solution test workflows

**Likely touch points**

- [src/tests](src/tests)
- [src/Genocs.Logging](src/Genocs.Logging)

**Acceptance criteria**

- logging package has focused automated unit coverage
- critical runtime paths are test-protected

**Dependencies**

- none

### `LOGGING-015` Add integration tests for middleware and endpoint behavior

**Status**: Implemented & validated (April 2026)

**Priority**: P1

**Problem**

Some behaviors (middleware ordering, endpoint contracts, and request/response body capture) are best validated at host-integration level.

**Scope**

- add host-level tests for `UseLogging`, `MapLogLevelHandler`, and correlation middleware pipeline use
- verify behavior under representative ASP.NET Core request flows

**Likely touch points**

- [src/tests](src/tests)
- [src/Genocs.Logging](src/Genocs.Logging)

**Acceptance criteria**

- critical host behaviors are verified in integration-style tests

**Dependencies**

- `LOGGING-014`

### `LOGGING-016` Align XML docs and NuGet README with actual behavior

**Status**: Implemented & validated (April 2026)

**Priority**: P2

**Problem**

Some comments and package guidance still imply broader capabilities than currently implemented or omit important caveats.

**Scope**

- align XML comments and README examples with current implementation
- highlight non-overlap with `Genocs.Telemetry`
- remove ambiguous wording around unsupported or optional sinks

**Likely touch points**

- [src/Genocs.Logging/README_NUGET.md](src/Genocs.Logging/README_NUGET.md)
- [src/Genocs.Logging/Extensions.cs](src/Genocs.Logging/Extensions.cs)
- [docs/Genocs.Logging-Agent-Documentation.md](docs/Genocs.Logging-Agent-Documentation.md)

**Acceptance criteria**

- package docs match real behavior and known constraints

**Dependencies**

- `LOGGING-006`
- `LOGGING-007`

### `LOGGING-017` Add migration notes for runtime behavior changes

**Status**: Not started

**Priority**: P2

**Problem**

Upcoming hardening tasks can alter runtime observability behavior and should be released with explicit migration notes.

**Scope**

- capture behavior deltas and migration notes for hosts using current logging setup
- add release-note checklist entries for logging behavior-affecting changes

**Likely touch points**

- [CHANGELOG.md](CHANGELOG.md)
- [docs/Genocs.Logging-Agent-Documentation.md](docs/Genocs.Logging-Agent-Documentation.md)

**Acceptance criteria**

- behavior changes have explicit migration guidance

**Dependencies**

- `LOGGING-005` to `LOGGING-013`

### `LOGGING-018` Establish package-level quality gate for new warnings/tests

**Status**: Not started

**Priority**: P2

**Problem**

Without package-specific quality gates, warning regressions and coverage gaps can return over time.

**Scope**

- enforce no-new-warning policy for `Genocs.Logging`
- ensure logging tests execute in CI test workflows
- document expected validation commands for maintainers

**Likely touch points**

- [Directory.Build.props](Directory.Build.props)
- [Makefile](Makefile)
- [scripts](scripts)

**Acceptance criteria**

- CI/local workflows fail on warning regressions for `Genocs.Logging`
- logging package test coverage is part of routine validation

**Validation**

- `dotnet build src/Genocs.Logging/Genocs.Logging.csproj -c Debug --nologo`
- `dotnet test <logging-tests-csproj> -c Debug --nologo`

**Dependencies**

- `LOGGING-014`

---

## Release Strategy Notes

- M1 should ship first as a stabilization release slice.
- M2 and M3 may include behavior changes and should be accompanied by migration notes.
- M4 should complete before broad recommendation of advanced payload/decorator features.

## Cross-Package Coordination

The following packages are likely affected by logging behavior changes and should be validated during execution:

- `Genocs.Core`
- `Genocs.WebApi`
- `Genocs.WebApi.CQRS`
- `Genocs.Telemetry`
- host applications under [src/demo](src/demo) and [src/apps](src/apps)

## Suggested First Sprint

1. Deliver `LOGGING-001` through `LOGGING-004` with build validation.
2. Add `Genocs.Logging` unit test project and land initial coverage (`LOGGING-014`).
3. Implement sink-guard hardening (`LOGGING-005`) and `logger.enabled` decision (`LOGGING-006`) with migration notes.