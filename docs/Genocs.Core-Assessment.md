# Genocs.Core Assessment (April 2026 Refresh)

## Purpose

This assessment re-evaluates the current Genocs.Core implementation against:

- docs/Genocs.Core-Implementation-Backlog.md
- docs/Genocs.Core-vNext-Dependency-Impact.md

The objective is to confirm completed hardening work, identify residual architecture risks, and define the next implementation slice for the vNext stream.

## Assessment Scope

Reviewed areas:

- startup/bootstrap flow
- endpoint mapping behavior
- CQRS registration and dispatch paths
- repository abstraction shape and async migration status
- diagnostics hooks
- package/dependency boundaries and downstream impact
- unit and integration test coverage status

Primary files inspected include:

- src/Genocs.Core/Builders/Extensions.cs
- src/Genocs.Core/Builders/GenocsBuilder.cs
- src/Genocs.Core/Builders/CoreDiagnostics.cs
- src/Genocs.Core/CQRS/Commons/HandlerRegistration.cs
- src/Genocs.Core/CQRS/Queries/Dispatchers/QueryDispatcher.cs
- src/Genocs.Core/Domain/Repositories/RepositoryBase.cs
- src/Genocs.Core/Extensions/Encryption.cs

## Validation Evidence

Executed on 2026-04-05:

- dotnet test src/tests/Genocs.Core.UnitTests/Genocs.Core.UnitTests.csproj -c Debug --nologo
- dotnet build src/Genocs.Core/Genocs.Core.csproj -c Debug --nologo

Results:

- tests: 84 total, 84 passed, 0 failed
- build: succeeded across net8.0, net9.0, net10.0
- diagnostics: no active compiler/analyzer errors for src/Genocs.Core

## Executive Summary

Current state is stable and materially improved versus pre-April baseline. CORE-001 through CORE-017 are implemented and reflected in source and test coverage. Runtime correctness, nullability alignment, and CQRS wiring robustness are in good shape.

The remaining work is now primarily boundary governance (CORE-018) and intentional vNext breaking-change execution. Endpoint exposure semantics are now aligned across MapDefaultEndpoints overloads and validated in tests for both Development and Production.

## Backlog Conformance Check

Status against docs/Genocs.Core-Implementation-Backlog.md:

- CORE-001 to CORE-017: Confirmed implemented
- CORE-018: Still planned, not yet implemented

Evidence highlights:

- startup async path exists and sync wrapper delegates to async without Task.Run hop
- reflection-heavy query dispatch path is replaced with cached typed invokers
- deterministic handler registration helper exists and is used by registration extensions
- repository base contains async-first override hooks and GetByIdAsync
- diagnostics module exists (AddCoreDiagnostics + state collection)
- focused unit/integration tests exist for startup sequencing, wiring, and dispatch behavior

## Findings (Ordered by Severity)

### 1) vNext startup boundary remains planned but not executed

Severity: Medium

Where:

- src/Genocs.Core/Builders/Extensions.cs
- src/apps/**/Program.cs
- docs/Genocs.Core-vNext-Dependency-Impact.md

Details:

- vNext ledger still marks startup API stream changes as Planned.
- UseGenocs() synchronous API remains active and widely used by downstream hosts.

Impact:

- vNext migration cannot progress to completed state without explicit startup API decision and coordinated host migrations

Recommendation:

- finalize CORE-vNext-001 plan: either deprecate UseGenocs() now or schedule hard removal date
- add migration notes and batch host updates by owner

### 2) Debug/Release dependency mode can drift over time

Severity: Medium

Where:

- src/Genocs.Core/Genocs.Core.csproj

Details:

- Debug uses ProjectReference to Genocs.Common.
- Release uses fixed PackageReference Genocs.Common 9.0.0-beta002.

Impact:

- local and CI behavior can diverge when Common evolves ahead of package publication
- harder to reason about true integration baseline during vNext refactors

Recommendation:

- define explicit policy for version lockstep in vNext branch
- ensure dependency-impact tracker records when package/reference mode intentionally differs

### 3) CQRS registration surface is still broad for vNext simplification goals

Severity: Low

Where:

- src/Genocs.Core/CQRS/Commands/Extensions.cs
- src/Genocs.Core/CQRS/Events/Extensions.cs
- src/Genocs.Core/CQRS/Queries/Extensions.cs
- src/Genocs.Core/CQRS/Commons/Extensions.cs

Details:

- canonical behavior improved, but multiple registration entry points remain available.
- this is functionally valid today, yet conflicts with vNext intent to reduce duplicate registration surface.

Impact:

- possible confusion for package consumers
- more migration/documentation overhead for CORE-vNext-002

Recommendation:

- converge to one recommended registration path and mark alternates for deprecation in vNext

## Dependency Impact Re-Check

Direct inbound ProjectReference dependents to Genocs.Core currently validate at 16 library projects plus Genocs.Core.UnitTests.

This matches the vNext impact rule document's direct dependent set and confirms the migration tracker scope remains accurate.

## Quality and Test Posture

Current posture is strong:

- startup sequencing, cancellation, and initializer execution are covered
- command/event/query dispatch paths include integration-style DI wiring tests
- repository async-first hook path is directly tested
- endpoint mapping behavior is tested for root and health routes

Residual gap:

- Startup API deprecation execution and downstream migration coverage remain pending under CORE-018

## Recommended Next Slice (CORE-018)

1. Finalize Core boundary decisions (keep/deprecate/extract list) and move CORE-vNext-001..004 to active execution states.
2. Define and publish startup API deprecation/removal timeline.
3. Update per-project migration tracker states from QUEUED-W1 to active/validated as packages migrate.
4. Add breaking-change callouts to CHANGELOG for any removed APIs.

## Conclusion

Genocs.Core is currently stable and test-validated for implemented backlog items through CORE-017, including aligned endpoint exposure behavior across hosting overloads. The project is ready for the boundary-focused vNext phase, with priority now on executing planned startup/CQRS surface reductions under CORE-018 governance.
