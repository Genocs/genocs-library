# Genocs.Telemetry Implementation Backlog

## Purpose

This backlog translates observed architectural and quality concerns in Genocs.Telemetry into issue-sized implementation work.

The backlog follows the same execution-oriented model used by Genocs.Logging and is ordered by delivery risk, not namespace.

## Current Status

Observed baseline (April 2026):

- Genocs.Telemetry builds successfully for net10.0, net9.0, and net8.0.
- A dedicated Genocs.Telemetry unit test project exists under src/tests.
- SQL statement text scrubbing is implemented through a custom activity processor when telemetry.sqlClient.enableStatementText is false.
- telemetry.sqlClient.enabled semantics are now enforced at runtime; telemetry.mongoDB now exposes only enabled and enableTracing.
- Deterministic log-export ownership is now enforced: Genocs.Telemetry handles traces and metrics, while Genocs.Logging owns logs.

Next recommended items:

- Start with M1 baseline hardening and overlap guardrails (TELEMETRY-001 to TELEMETRY-004).

## Planning Assumptions

- Fix correctness, option-contract consistency, and overlap risks before widening feature surface.
- Keep Genocs.Telemetry focused on OpenTelemetry setup, signal instrumentation, and exporter wiring.
- Keep Serilog sink ownership in Genocs.Logging; avoid duplicate log export paths across packages.
- Pair every runtime behavior change with focused tests and migration notes.
- Preserve backward-compatible defaults where feasible and make breaking behavior explicit when required.

## Functional Overlap Issues To Track

1. Dual log export overlap:
- Genocs.Logging can export logs to OTLP and Azure Application Insights.
- Genocs.Telemetry no longer wires log exporters.
- Logs should be exported through Genocs.Logging only to avoid duplicate ingestion.

2. Correlation overlap risk:
- Genocs.Logging correlation middleware enriches log events with request context.
- Genocs.Telemetry ASP.NET instrumentation enriches traces with correlation and route tags.
- Without clear ownership documentation, teams may assume both are required for the same outcome or may over-enrich data.

3. Configuration overlap ambiguity:
- Option flags exist in Telemetry models that are currently no-op at runtime.
- This can make consumers believe features are active when they are not.

## Milestones

| Milestone | Goal | Tasks |
|---|---|---|
| M1 | Contract correctness and overlap-safe baseline | TELEMETRY-001 to TELEMETRY-004 |
| M2 | Runtime safety and exporter behavior hardening | TELEMETRY-005 to TELEMETRY-009 |
| M3 | Signal governance, cardinality, and performance robustness | TELEMETRY-010 to TELEMETRY-013 |
| M4 | Testability, docs polish, migration guidance, and quality gates | TELEMETRY-014 to TELEMETRY-018 |

## Execution Order

1. Complete M1 before introducing broader exporter changes.
2. Complete M2 before recommending combined Logging and Telemetry production templates.
3. Complete M3 before enabling broad wildcard source collection by default guidance.
4. Use M4 to lock in quality gates and adoption guidance.

---

## M1: Contract Correctness and Overlap-Safe Baseline

### TELEMETRY-001 Enforce telemetry.sqlClient.enabled semantics

**Status**: Implemented (validated April 2026)

**Priority**: P0

**Problem**

SqlClient instrumentation is always added, while telemetry.sqlClient.enabled exists and implies toggle behavior.

**Scope**

- decide and enforce whether telemetry.sqlClient.enabled is authoritative
- align runtime behavior with documented configuration contract
- add tests for enabled and disabled SQL instrumentation flows

**Likely touch points**

- [src/Genocs.Telemetry/Extensions.cs](src/Genocs.Telemetry/Extensions.cs)
- [src/Genocs.Telemetry/Configurations/SqlClientOptions.cs](src/Genocs.Telemetry/Configurations/SqlClientOptions.cs)
- [src/Genocs.Telemetry/README_NUGET.md](src/Genocs.Telemetry/README_NUGET.md)

**Acceptance criteria**

- telemetry.sqlClient.enabled has clear and test-verified behavior
- docs and runtime behavior are aligned

**Dependencies**

- none

**Implementation notes**

- `telemetry.sqlClient.enabled` is now authoritative for SQL client tracing registration.
- SQL client tracing remains enabled by default when `telemetry.sqlClient` is omitted to preserve backward compatibility.
- SQL statement text scrubbing now runs only when SQL client tracing is enabled and `telemetry.sqlClient.enableStatementText` is `false`.
- Added focused unit tests in `src/tests/Genocs.Telemetry.UnitTests` for enabled/disabled SQL instrumentation semantics.

**Validation**

- `dotnet build src/Genocs.Telemetry/Genocs.Telemetry.csproj -c Debug --nologo`
- `dotnet test src/tests/Genocs.Telemetry.UnitTests/Genocs.Telemetry.UnitTests.csproj -c Debug --nologo`

### TELEMETRY-002 Clarify or remove non-implemented MongoDB options

**Status**: Implemented (validated April 2026)

**Priority**: P1

**Problem**

MongoDbOptions exposed enableMetrics and enableLogging but runtime code only used enableTracing.

**Scope**

- remove non-implemented MongoDB option flags from public configuration contract
- align docs so `telemetry.mongoDB` is clearly tracing-only
- verify package build and telemetry tests remain green

**Likely touch points**

- [src/Genocs.Telemetry/Configurations/MongoDbOptions.cs](src/Genocs.Telemetry/Configurations/MongoDbOptions.cs)
- [src/Genocs.Telemetry/Extensions.cs](src/Genocs.Telemetry/Extensions.cs)
- [docs/Genocs.Telemetry-Agent-Documentation.md](docs/Genocs.Telemetry-Agent-Documentation.md)

**Acceptance criteria**

- consumer expectations around MongoDB telemetry options are explicit and correct

**Dependencies**

- none

**Implementation notes**

- Removed `EnableMetrics` and `EnableLogging` from `MongoDbOptions`.
- Kept MongoDB telemetry contract focused on `enabled` + `enableTracing`.
- Updated telemetry agent documentation to remove stale no-op-option guidance.

**Validation**

- `dotnet build src/Genocs.Telemetry/Genocs.Telemetry.csproj -c Debug --nologo`
- `dotnet test src/tests/Genocs.Telemetry.UnitTests/Genocs.Telemetry.UnitTests.csproj -c Debug --nologo`

### TELEMETRY-003 Add deterministic overlap policy for log exporters

**Status**: Implemented (validated April 2026)

**Priority**: P0

**Problem**

Logging and Telemetry packages can export logs to the same OTLP or Azure backend, causing duplicate ingestion.

**Scope**

- define one clear ownership model for logs per deployment profile
- provide explicit conflict detection guidance and recommended config combinations
- update package docs with non-overlapping templates

**Likely touch points**

- [src/Genocs.Telemetry/README_NUGET.md](src/Genocs.Telemetry/README_NUGET.md)
- [src/Genocs.Logging/README_NUGET.md](src/Genocs.Logging/README_NUGET.md)
- [docs/Genocs.Telemetry-Agent-Documentation.md](docs/Genocs.Telemetry-Agent-Documentation.md)

**Acceptance criteria**

- docs define overlap-safe deployment templates for OTLP and Azure
- no ambiguous dual-log-export guidance remains

**Dependencies**

- none

**Implementation notes**

- Completed overlap assessment across `Genocs.Telemetry` and `Genocs.Logging` runtime/exporter paths.
- Removed OpenTelemetry log exporter wiring from `Genocs.Telemetry` to establish single-owner behavior.
- Removed telemetry-side logging flags from telemetry option contracts (`OtlpExportOptions`, `ConsoleOptions`, `AzureOptions`).
- Updated Telemetry and Logging package docs with explicit ownership guidance and overlap-safe templates.

**Validation**

- `dotnet build src/Genocs.Telemetry/Genocs.Telemetry.csproj -c Debug --nologo`
- `dotnet test src/tests/Genocs.Telemetry.UnitTests/Genocs.Telemetry.UnitTests.csproj -c Debug --nologo`

### TELEMETRY-004 Normalize analyzer and nullability baseline

**Status**: Implemented (validated April 2026)

**Priority**: P1

**Problem**

Telemetry should maintain warning-clean quality and avoid nullable or style regressions in the main extension surface.

**Scope**

- ensure warning-clean baseline for Genocs.Telemetry in touched files
- tighten nullability and guard patterns where needed
- add build validation command to package docs

**Likely touch points**

- [src/Genocs.Telemetry/Extensions.cs](src/Genocs.Telemetry/Extensions.cs)
- [src/Genocs.Telemetry/Genocs.Telemetry.csproj](src/Genocs.Telemetry/Genocs.Telemetry.csproj)

**Acceptance criteria**

- Genocs.Telemetry builds warning-clean, or explicit exceptions are documented

**Implementation notes**

- Hardened guard clauses across the telemetry extension surface to make nullability expectations explicit for entry points and enrichment helpers.
- Added project-level nullable warning enforcement in `Genocs.Telemetry.csproj` via `WarningsAsErrors` to prevent silent nullable regressions.
- Added a package-level validation section in `README_NUGET.md` with the canonical telemetry build command for maintainers.

**Validation**

- `dotnet build src/Genocs.Telemetry/Genocs.Telemetry.csproj -c Debug --nologo`

**Dependencies**

- none

---

## M2: Runtime Safety and Exporter Behavior Hardening

### TELEMETRY-005 Guard invalid OTLP endpoint configuration

**Status**: Not started

**Priority**: P1

**Problem**

Invalid telemetry.exporter.otlpEndpoint values can fail with startup exceptions when converted to Uri.

**Scope**

- validate endpoint format before applying exporter options
- skip invalid exporter wiring with clear diagnostics
- preserve host startup safety for misconfigured environments

**Likely touch points**

- [src/Genocs.Telemetry/Extensions.cs](src/Genocs.Telemetry/Extensions.cs)
- [src/Genocs.Telemetry/Configurations/OtlpExportOptions.cs](src/Genocs.Telemetry/Configurations/OtlpExportOptions.cs)

**Acceptance criteria**

- invalid OTLP endpoint does not crash startup
- behavior is deterministic and documented

**Dependencies**

- TELEMETRY-004

### TELEMETRY-006 Validate exporter batch settings bounds

**Status**: Not started

**Priority**: P2

**Problem**

Exporter queue and batch settings are directly applied without sanity bounds, enabling pathological values.

**Scope**

- define and enforce minimum and maximum safe bounds for batch settings
- apply safe fallback defaults for out-of-range values
- document tuning guidance for high-throughput workloads

**Likely touch points**

- [src/Genocs.Telemetry/Extensions.cs](src/Genocs.Telemetry/Extensions.cs)
- [src/Genocs.Telemetry/Configurations/OtlpExportOptions.cs](src/Genocs.Telemetry/Configurations/OtlpExportOptions.cs)
- [src/Genocs.Telemetry/README_NUGET.md](src/Genocs.Telemetry/README_NUGET.md)

**Acceptance criteria**

- invalid batch settings are normalized or rejected predictably
- docs describe supported ranges and defaults

**Dependencies**

- TELEMETRY-005

### TELEMETRY-007 Formalize host-mode behavior for OpenTelemetry logs

**Status**: Not started

**Priority**: P2

**Problem**

OpenTelemetry log export is only wired when WebApplicationBuilder is available, but this is easy to miss.

**Scope**

- make host-mode behavior explicit in docs and diagnostics
- verify behavior for host modes where WebApplicationBuilder is absent
- add tests that assert traces and metrics still register without log export

**Likely touch points**

- [src/Genocs.Telemetry/Extensions.cs](src/Genocs.Telemetry/Extensions.cs)
- [src/Genocs.Telemetry/README_NUGET.md](src/Genocs.Telemetry/README_NUGET.md)

**Acceptance criteria**

- host-mode log export behavior is explicit, deterministic, and test-covered

**Dependencies**

- none

### TELEMETRY-008 Rationalize unused Jaeger exporter dependency

**Status**: Not started

**Priority**: P3

**Problem**

The package currently references OpenTelemetry.Exporter.Jaeger while registration uses OTLP and does not wire Jaeger exporter directly.

**Scope**

- confirm whether Jaeger package is required transitively or obsolete
- remove unused dependency if not required
- document Jaeger usage via OTLP path only

**Likely touch points**

- [src/Genocs.Telemetry/Genocs.Telemetry.csproj](src/Genocs.Telemetry/Genocs.Telemetry.csproj)
- [src/Genocs.Telemetry/README_NUGET.md](src/Genocs.Telemetry/README_NUGET.md)

**Acceptance criteria**

- dependency set reflects actual runtime/exporter behavior

**Dependencies**

- none

### TELEMETRY-009 Add explicit exporter conflict guidance for Azure and OTLP

**Status**: Not started

**Priority**: P2

**Problem**

Telemetry allows multiple exporters per signal, which is valid but can be accidental and costly without clear guidance.

**Scope**

- document intentional multi-export scenarios vs accidental duplicates
- provide profile-based configuration examples
- add overlap warning notes for logs when Genocs.Logging is enabled

**Likely touch points**

- [src/Genocs.Telemetry/README_NUGET.md](src/Genocs.Telemetry/README_NUGET.md)
- [docs/Genocs.Telemetry-Agent-Documentation.md](docs/Genocs.Telemetry-Agent-Documentation.md)

**Acceptance criteria**

- exporter combinations are explicit and operationally predictable

**Dependencies**

- TELEMETRY-003

---

## M3: Signal Governance, Cardinality, and Performance Robustness

### TELEMETRY-010 Constrain wildcard activity source collection

**Status**: Not started

**Priority**: P1

**Problem**

AddSource("*") can increase noise and cardinality by collecting spans from unintended third-party sources.

**Scope**

- define policy for wildcard usage (default allow or constrained allowlist)
- provide configurable source include list if needed
- preserve explicit Genocs sources while reducing accidental trace noise

**Likely touch points**

- [src/Genocs.Telemetry/Extensions.cs](src/Genocs.Telemetry/Extensions.cs)
- [src/Genocs.Telemetry/Configurations/TelemetryOptions.cs](src/Genocs.Telemetry/Configurations/TelemetryOptions.cs)
- [src/Genocs.Telemetry/README_NUGET.md](src/Genocs.Telemetry/README_NUGET.md)

**Acceptance criteria**

- source collection strategy is documented, bounded, and testable

**Dependencies**

- none

### TELEMETRY-011 Harden route and correlation enrichment cardinality

**Status**: Not started

**Priority**: P2

**Problem**

Fallback to raw request path can create high-cardinality route tags and hinder aggregation quality.

**Scope**

- define route-tag fallback semantics and optional normalization
- avoid high-cardinality enrichment in default configuration
- add tests for route metadata present and absent scenarios

**Likely touch points**

- [src/Genocs.Telemetry/Extensions.cs](src/Genocs.Telemetry/Extensions.cs)
- [docs/Genocs.Telemetry-Agent-Documentation.md](docs/Genocs.Telemetry-Agent-Documentation.md)

**Acceptance criteria**

- route enrichment remains useful without introducing avoidable cardinality explosions

**Dependencies**

- TELEMETRY-010

### TELEMETRY-012 Add guardrails for exception tag payload size

**Status**: Not started

**Priority**: P3

**Problem**

Unbounded exception message values can create oversized span attributes and backend ingestion pressure.

**Scope**

- define truncation/sanitization policy for exception tags
- preserve diagnostic value while bounding payload size
- document behavior and rationale

**Likely touch points**

- [src/Genocs.Telemetry/Extensions.cs](src/Genocs.Telemetry/Extensions.cs)
- [src/Genocs.Telemetry/README_NUGET.md](src/Genocs.Telemetry/README_NUGET.md)

**Acceptance criteria**

- exception enrichment is bounded and predictable under failure storms

**Dependencies**

- none

### TELEMETRY-013 Verify no-op overhead when exporters are disabled

**Status**: Not started

**Priority**: P2

**Problem**

When telemetry exporters are disabled, registration and enrichment paths should still remain low-overhead and predictable.

**Scope**

- benchmark and verify disabled-exporter runtime overhead expectations
- document minimal-overhead profile configuration
- add regression tests for disabled exporter mode

**Likely touch points**

- [src/Genocs.Telemetry/Extensions.cs](src/Genocs.Telemetry/Extensions.cs)
- [src/tests](src/tests)

**Acceptance criteria**

- disabled exporter profile has documented and validated overhead behavior

**Dependencies**

- none

---

## M4: Testability, Contract Polish, and Adoption Readiness

### TELEMETRY-014 Introduce dedicated Genocs.Telemetry unit test project

**Status**: Not started

**Priority**: P0

**Problem**

Telemetry package behavior is not protected by a focused unit test project.

**Scope**

- add Genocs.Telemetry.UnitTests under src/tests
- cover registration guards, option semantics, and exporter safety
- wire project into solution test workflows

**Likely touch points**

- [src/tests](src/tests)
- [src/Genocs.Telemetry](src/Genocs.Telemetry)

**Acceptance criteria**

- telemetry package has focused automated unit coverage
- critical runtime and config paths are test-protected

**Dependencies**

- none

### TELEMETRY-015 Add integration tests for host registration outcomes

**Status**: Not started

**Priority**: P1

**Problem**

Some behaviors are best validated in host-level tests, especially signal registration and exporter combinations.

**Scope**

- add host-level integration tests for AddTelemetry under realistic service builder flows
- verify behavior when app.service is missing, telemetry.enabled is false, or host mode lacks WebApplicationBuilder

**Likely touch points**

- [src/tests](src/tests)
- [src/Genocs.Telemetry](src/Genocs.Telemetry)

**Acceptance criteria**

- host registration behaviors are deterministic and integration-tested

**Dependencies**

- TELEMETRY-014

### TELEMETRY-016 Align XML docs and README with runtime reality

**Status**: Not started

**Priority**: P2

**Problem**

Current guidance can be interpreted as broader capability than runtime behavior in some option areas.

**Scope**

- align comments and README examples with implemented behavior
- explicitly separate concerns between Genocs.Logging and Genocs.Telemetry
- remove ambiguous language around no-op options

**Likely touch points**

- [src/Genocs.Telemetry/README_NUGET.md](src/Genocs.Telemetry/README_NUGET.md)
- [src/Genocs.Telemetry/Configurations](src/Genocs.Telemetry/Configurations)
- [docs/Genocs.Telemetry-Agent-Documentation.md](docs/Genocs.Telemetry-Agent-Documentation.md)

**Acceptance criteria**

- package docs match real behavior and overlap boundaries

**Dependencies**

- TELEMETRY-001
- TELEMETRY-002
- TELEMETRY-003

### TELEMETRY-017 Add migration notes for behavior-affecting hardening

**Status**: Not started

**Priority**: P2

**Problem**

Hardening in M1 to M3 can change runtime observability behavior and should ship with explicit migration notes.

**Scope**

- capture behavior deltas and migration notes for existing adopters
- add release-note checklist entries for telemetry behavior-affecting changes

**Likely touch points**

- [CHANGELOG.md](CHANGELOG.md)
- [docs/Genocs.Telemetry-Agent-Documentation.md](docs/Genocs.Telemetry-Agent-Documentation.md)

**Acceptance criteria**

- behavior changes have explicit migration guidance

**Dependencies**

- TELEMETRY-001 to TELEMETRY-013

### TELEMETRY-018 Establish package quality gate for warnings and telemetry tests

**Status**: Not started

**Priority**: P2

**Problem**

Without package-specific gates, warning regressions and coverage gaps can reappear.

**Scope**

- enforce no-new-warning policy for Genocs.Telemetry
- ensure telemetry tests run in CI workflows
- document validation commands for maintainers

**Likely touch points**

- [Directory.Build.props](Directory.Build.props)
- [Makefile](Makefile)
- [scripts](scripts)

**Acceptance criteria**

- CI/local workflows fail on warning regressions for Genocs.Telemetry
- telemetry package tests are part of routine validation

**Validation**

- dotnet build src/Genocs.Telemetry/Genocs.Telemetry.csproj -c Debug --nologo
- dotnet test <telemetry-tests-csproj> -c Debug --nologo

**Dependencies**

- TELEMETRY-014

---

## Release Strategy Notes

- M1 should ship first as a contract-correctness stabilization slice.
- M2 and M3 may affect runtime telemetry shape and should include migration notes.
- M4 should complete before broad recommendation of advanced exporter and source-governance configurations.

## Cross-Package Coordination

The following packages and hosts are likely affected by telemetry behavior changes and should be validated during execution:

- Genocs.Core
- Genocs.Logging
- Genocs.WebApi
- Genocs.WebApi.CQRS
- host applications under [src/demo](src/demo) and [src/apps](src/apps)

## Suggested First Sprint

1. Deliver TELEMETRY-001 through TELEMETRY-004 with build validation.
2. Add Genocs.Telemetry unit test project and land initial coverage (TELEMETRY-014).
3. Implement OTLP endpoint hardening (TELEMETRY-005) and overlap-safe exporter templates (TELEMETRY-003 and TELEMETRY-009).