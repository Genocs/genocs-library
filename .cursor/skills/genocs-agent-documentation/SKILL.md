---
name: genocs-agent-documentation
description: >-
  Applies authoritative Genocs NuGet package guidance from docs/*-Agent-Documentation.md.
  Use when implementing or refactoring Genocs libraries (WebApi, Messaging, Saga, Http,
  Logging, Telemetry, Persistence, OpenAPI, CQRS, Auth, Outbox, RabbitMQ integrations),
  when the user names a Genocs package, or when agent behavior must match published
  contracts rather than guessing from source.
---

# Genocs Agent Documentation

## When to use this skill

Load and follow the matching agent reference **before** generating integration code, DI registration, or public API usage for a Genocs package. These files are written for AI consumption: they define safe API surfaces, common misconceptions, and recipes.

Skip deep reads only for trivial renames or comments with no behavioral impact.

## How to work with the docs

1. **Identify the NuGet package** the task concerns (for example `Genocs.Messaging.RabbitMQ`).
2. **Open the corresponding file** from the table below with the Read tool (paths are repo-relative from the workspace root).
3. **Honor the opening section** (`Agent Operating Mode`, `Consumer Mode for Agents`, or equivalent): NuGet-only assumption, no invented transport or persistence, ask about companion packages when unclear.
4. **Prefer documented facts** over inferred internals: use the package identity tables, “does not do” lists, mental models, and recipe code as the source of truth.
5. **Compose multiple packages** by reading each package’s doc when the change crosses boundaries (for example WebApi + Auth + OpenAPI).

## Active reference files (`docs/`)

| Package / topic | Agent documentation file |
|----------------|--------------------------|
| `Genocs.Http` | `docs/Genocs.Http-Agent-Documentation.md` |
| `Genocs.Http.RestEase` | `docs/Genocs.Http.RestEase-Agent-Documentation.md` |
| `Genocs.Logging` | `docs/Genocs.Logging-Agent-Documentation.md` |
| `Genocs.Messaging` | `docs/Genocs.Messaging-Agent-Documentation.md` |
| `Genocs.Messaging.Outbox` | `docs/Genocs.Messaging.Outbox-Agent-Documentation.md` |
| `Genocs.Messaging.Outbox.MongoDB` | `docs/Genocs.Messaging.Outbox.MongoDB-Agent-Documentation.md` |
| `Genocs.Messaging.RabbitMQ` | `docs/Genocs.Messaging.RabbitMQ-Agent-Documentation.md` |
| `Genocs.Persistence.MongoDB` | `docs/Genocs.Persistence.MongoDB-Agent-Documentation.md` |
| `Genocs.Saga` | `docs/Genocs.Saga-Agent-Documentation.md` |
| `Genocs.Saga.Integrations.MongoDB` | `docs/Genocs.Saga.Integrations.MongoDB-Agent-Documentation.md` |
| `Genocs.Saga.Integrations.Redis` | `docs/Genocs.Saga.Integrations.Redis-Agent-Documentation.md` |
| `Genocs.Telemetry` | `docs/Genocs.Telemetry-Agent-Documentation.md` |
| `Genocs.WebApi` | `docs/Genocs.WebApi-Agent-Documentation.md` |
| `Genocs.WebApi.Auth` | `docs/Genocs.WebApi.Auth-Agent-Documentation.md` |
| `Genocs.WebApi.CQRS` | `docs/Genocs.WebApi.CQRS-Agent-Documentation.md` |
| `Genocs.WebApi.OpenApi` | `docs/Genocs.WebApi.OpenApi-Agent-Documentation.md` |

If a package is not listed, search `docs/` for `Agent Reference` or `Agent Operating Mode` in filenames matching `*-Agent-Documentation.md`.

## Archived docs

`docs/archived/` holds older agent references (for example `Genocs.Core`, `Genocs.Common`, `Genocs.Auth`). **Do not treat archived files as current product guidance** unless the user explicitly asks to use or compare them. Prefer active `docs/` references and the live codebase for current APIs.

## Quality bar after reading

- Generated code uses only APIs and patterns described or clearly implied in the doc plus normal C# and ASP.NET Core usage.
- No capabilities are assumed for a package that its doc lists under “does not” / “do not assume” style sections.
- Companion packages are named or confirmed when the doc says behavior depends on another package.
