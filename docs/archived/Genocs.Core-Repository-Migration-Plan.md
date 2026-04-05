# Genocs.Core Repository Async-First Migration Plan

## Objective

Move Core repository usage toward async-first and provider-agnostic defaults while preserving compatibility for existing sync and IQueryable-based implementations.

## Versioning Path

1. Phase 1 (current, 9.x): Introduce async-first override hooks in Core base repositories without breaking existing contracts.
2. Phase 2 (next minor): Mark sync-first and provider-leaking entry points as compatibility paths and discourage new usage in docs/templates.
3. Phase 3 (next major): Remove legacy sync/IQueryable-first APIs from default contracts and keep provider-specific query access as explicit opt-in adapters.

## Phase 1 Delivered in CORE-009

- Added async-first extension points to [src/Genocs.Core/Domain/Repositories/RepositoryBase.cs](src/Genocs.Core/Domain/Repositories/RepositoryBase.cs):
  - GetAllListCoreAsync
  - FirstOrDefaultByIdCoreAsync
  - FirstOrDefaultCoreAsync
  - SingleCoreAsync
  - CountCoreAsync
  - LongCountCoreAsync
- Routed public async methods through the async-first hooks, so new implementations can override native async behavior directly.
- Kept existing synchronous methods and signatures intact for backward compatibility.
- Added GetByIdAsync to align with Genocs.Common repository expectations.

## Guidance for New Implementations

- Prefer overriding async core hooks in repository implementations.
- Treat synchronous methods as compatibility bridges for older providers.
- Avoid exposing IQueryable in domain-facing services; use specification-based or explicit query methods.

## Provider-Specific Querying Policy

- Provider-specific querying remains opt-in:
  - Keep provider-specific logic in persistence adapter packages.
  - Avoid leaking IQueryable-heavy behavior through default domain services.

## Validation

- Unit tests in [src/tests/Genocs.Core.UnitTests/Domain/Repositories/RepositoryBaseAsyncFlowTests.cs](src/tests/Genocs.Core.UnitTests/Domain/Repositories/RepositoryBaseAsyncFlowTests.cs) verify that async methods use async-first hooks.
