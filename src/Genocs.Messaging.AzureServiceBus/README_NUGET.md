# Genocs.Messaging.AzureServiceBus

![Genocs Library Banner](https://raw.githubusercontent.com/Genocs/genocs-library/main/assets/genocs-library-banner.png)

Azure Service Bus integration for messaging and transport workflows. Supports `net10.0`, `net9.0`, and `net8.0`.

## Installation

```bash
dotnet add package Genocs.Messaging.AzureServiceBus
```

## Getting Started

Use this package to integrate Azure Service Bus transport capabilities with Genocs messaging workflows.

Register Azure Service Bus through the Genocs builder extension:

```csharp
using Genocs.Core.Builders;
using Genocs.Messaging.AzureServiceBus;

IGenocsBuilder genocs = builder
		.AddGenocs()
		.AddAzureServiceBus();
```

The extension binds the `azureServiceBusQueue` and `azureServiceBusTopic` sections and registers queue/topic services based on the corresponding `Enabled` flags.

When enabled, queue/topic processors are started and stopped through the host `IHostedService` lifecycle (fully async startup/shutdown), avoiding sync-over-async startup blocking.

Handler registration direction:

- Prefer modern CQRS handler registrations: `ConsumeModern<T, TH>()` and `SubscribeModern<T, TH>()`.
- Legacy registrations `Consume<T, TH>()` / `Subscribe<T, TH>()` remain available for compatibility and are marked obsolete.

```json
{
	"azureServiceBusQueue": {
		"enabled": true,
		"connectionString": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key",
		"queueName": "orders-queue"
	},
	"azureServiceBusTopic": {
		"enabled": true,
		"connectionString": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key",
		"topicName": "orders-topic",
		"subscriptionName": "orders-subscription"
	}
}
```

## Warning Policy

Messaging package warning baseline checks are enforced through [validate-messaging.mk](../../validate-messaging.mk):

- `net10.0` build must remain warning-free (`-warnaserror`).
- Messaging unit tests validate registration and migration contract behavior.

## Support

- Documentation Portal: https://learn.fiscanner.net/
- Documentation: https://github.com/Genocs/genocs-library/tree/main/docs
- Repository: https://github.com/Genocs/genocs-library

## Release Notes

- CHANGELOG: https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md
- Releases: https://github.com/Genocs/genocs-library/releases
