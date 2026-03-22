# Genocs.Messaging.Outbox Agent Reference

## Consumer Mode for Agents

- Assume package is installed from NuGet.
- Do not rely on repository source code access.
- Prefer stable public APIs and extension methods documented here.
- If behavior is uncertain, fail safely and request config/package version details.

## Purpose

`Genocs.Messaging.Outbox` provides outbox abstractions and a background processor for reliable outbound publish and idempotent inbound handling.

## Quick Facts

| Key | Value |
|---|---|
| Package | `Genocs.Messaging.Outbox` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | Outbox abstraction and processing runtime |
| Core entry points | `AddMessageOutbox`, `AddInMemory`, `IMessageOutbox`, `IMessageOutboxAccessor` |

## Install

```bash
dotnet add package Genocs.Messaging.Outbox
```

## Minimal Integration Recipe (Program.cs)

```csharp
using Genocs.Core.Builders;
using Genocs.Messaging.Outbox;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder gnxBuilder = builder.AddGenocs();
gnxBuilder.AddMessageOutbox();
gnxBuilder.Build();

var app = builder.Build();
app.Run();
```

## Configuration

Use the `outbox` section.

```json
{
	"outbox": {
		"enabled": true,
		"expiry": 3600,
		"intervalMilliseconds": 5000,
		"inboxCollection": "inbox",
		"outboxCollection": "outbox",
		"type": "sequential",
		"disableTransactions": false
	}
}
```

| Setting | Type | Description |
|---|---|---|
| `enabled` | `bool` | Enables the outbox runtime and hosted processing loop. |
| `expiry` | `int` | Expiry window used by providers that support processed-message cleanup. |
| `intervalMilliseconds` | `double` | Polling interval used by the background processor. Must be positive when processing is enabled. |
| `inboxCollection` | `string` | Inbox storage name used by durable providers such as MongoDB. |
| `outboxCollection` | `string` | Outbox storage name used by durable providers such as MongoDB. |
| `type` | `string` | Processing strategy hint used by the runtime, for example sequential versus batch-style processing. |
| `disableTransactions` | `bool` | Disables transaction usage for providers that support transactional handling. |

For simple in-memory usage, only `enabled` and `intervalMilliseconds` are typically required. Collection and transaction settings become relevant when you add a durable provider.

## Decision Matrix For Agents

| Goal | Preferred API |
|---|---|
| Enable outbox with default storage | `AddMessageOutbox()` |
| Select explicit in-memory provider | `AddMessageOutbox(o => o.AddInMemory())` |
| Buffer outbound message before publish | `IMessageOutbox.SendAsync(...)` |
| Enforce idempotent inbound handling | `IMessageOutbox.HandleAsync(...)` |
| Retrieve and mark pending outbox records | `IMessageOutboxAccessor.GetUnsentAsync()` and `ProcessAsync(...)` |

## Behavior Notes / Constraints

- Outbox background processing starts only when enabled by configuration.
- Processing interval must be positive when outbox processing is enabled.
- In-memory provider is non-durable and suited for development or simple single-instance scenarios.

## Public Capability Map

- Outbox registration through `AddMessageOutbox`.
- Provider selection through the outbox configurator.
- Outbound buffering and inbound deduplication via `IMessageOutbox`.
- Pending-record retrieval and processed marking via `IMessageOutboxAccessor`.

## Dependencies

- `Genocs.Messaging`
- Optional durable provider package for production usage

## Troubleshooting

1. Outbox processor never starts.
Fix: Set `outbox.enabled` to true and configure a positive `outbox.intervalMilliseconds` value.
2. Messages disappear after service restart.
Fix: Replace in-memory outbox storage with a durable outbox provider.
3. Duplicate inbound processing still occurs.
Fix: Call `HandleAsync` with stable message identifiers for each consumed message.
