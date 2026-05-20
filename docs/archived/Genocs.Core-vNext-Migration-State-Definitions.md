# Genocs.Core vNext Migration State Definitions

## Purpose

This file defines the migration state values used in vNext dependency trackers.

## State Values

| State | Meaning | Exit Criteria |
|---|---|---|
| QUEUED-W1 | Planned for Wave 1 | Scope confirmed and owner acknowledged |
| QUEUED-W2 | Planned for Wave 2 | Wave 1 dependencies complete |
| IN-PROGRESS | Migration work has started | PR(s) open with migration changes |
| BLOCKED | Work cannot proceed | Blocker documented with owner and next action |
| MIGRATED | Code migrated to vNext contracts | Project builds and relevant tests pass |
| VERIFIED | Migration validated in solution context | Downstream integrations and CI checks pass |
| CLOSED | Fully complete | Changelog and documentation updates are merged |

## Wave Policy

- Wave 1 focuses on direct package dependents.
- Wave 2 focuses on transitive consumers and host applications.
- Projects can move waves only with documented rationale.
