# Genocs.Common vNext Owner Definitions

## Purpose

This file defines owner codes used by Genocs.Common vNext dependency trackers.

## Owner Codes

| Code | Team | Scope |
|---|---|---|
| COMMON | Common Contracts Team | Shared contracts, primitives, and compatibility policy for Genocs.Common |
| CORE | Core Platform Team | Genocs.Core contract adoption and runtime alignment |
| PERSIST | Persistence Team | Repository and persistence adapter alignment |
| WEBAPI | WebApi Team | API surface and endpoint contract adoption |
| MSG | Messaging Team | CQRS/messaging contract adoption |
| APPS | Application Team | App and demo host adoption of updated contracts |

## Assignment Rules

- Each tracker row must have one primary owner.
- Cross-team migrations can declare secondary owners in notes.
- If ownership is unclear, assign COMMON as default owner until triage is complete.
