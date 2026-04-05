# Genocs.Common vNext Migration State Definitions

## Purpose

This file defines migration state values used in Genocs.Common vNext trackers.

## State Values

| State | Meaning | Exit Criteria |
|---|---|---|
| QUEUED-W1 | Planned for direct package migrations | Owner assigned and migration scope confirmed |
| QUEUED-W2 | Planned for transitive package/app migrations | All blocking Wave 1 dependencies resolved |
| IN-PROGRESS | Migration implementation started | PR open with contract/code updates |
| BLOCKED | Work blocked by dependency or decision | Blocker recorded with next action and owner |
| MIGRATED | Package updated to vNext contract surface | Build and targeted tests pass |
| VERIFIED | Migration validated in integrated solution flow | CI or integration validation passes |
| CLOSED | Migration fully complete | Changelog and docs updates are merged |

## Wave Policy

- Wave 1 focuses on direct Genocs.Common dependents.
- Wave 2 focuses on transitive package dependents and host projects.
- State transitions must be documented in the migration tracker table.
