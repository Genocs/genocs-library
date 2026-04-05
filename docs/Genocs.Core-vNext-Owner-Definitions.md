# Genocs.Core vNext Owner Definitions

## Purpose

This file defines owner codes used by vNext migration trackers.

## Owner Codes

| Code | Team | Scope |
|---|---|---|
| CORE | Core Platform Team | Core runtime contracts, startup pipeline, shared abstractions |
| PERSIST | Persistence Team | EF Core, MongoDB, Redis adapters and repository integration |
| MSG | Messaging Team | Messaging abstractions and transport integrations |
| WEBAPI | WebApi Team | WebApi host wiring, endpoint composition, WebApi adapters |
| SEC | Security Team | Security and secrets-related package integration |
| OBS | Observability Team | Logging, telemetry, metrics package integration |
| APPS | Application Team | App and demo host migration and runtime adoption |

## Assignment Rules

- A project has one primary owner code.
- Cross-cutting migrations can include a secondary owner in notes.
- For blocking architecture decisions, default escalation owner is CORE.
