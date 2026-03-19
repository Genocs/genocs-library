# Migration Guide: Azure Service Bus Client Upgrade

This guide explains how to upgrade your application when moving from the legacy `Microsoft.Azure.ServiceBus` SDK to the modern `Azure.Messaging.ServiceBus` SDK used by **Genocs.Messaging.AzureServiceBus**.

---

## Why Upgrade?

The `Microsoft.Azure.ServiceBus` NuGet package has been **deprecated** by Microsoft. The replacement is `Azure.Messaging.ServiceBus`, which provides:

- Unified Azure SDK design (`Azure.Core` based)
- Built-in `IAsyncDisposable` support for proper resource cleanup
- Improved performance and connection management via `ServiceBusClient`
- Native support for AMQP over WebSockets
- Better retry and error handling options

---

## Breaking Changes

### 1. Configuration — `RetryPolicy` Removed

The `RetryPolicy` property has been removed from both `AzureServiceBusQueueOptions` and `AzureServiceBusTopicOptions`. The new SDK manages retries internally through `ServiceBusClientOptions.RetryOptions`.

**Before (appsettings.json):**

```json
{
  "azureServiceBusQueue": {
    "Enabled": true,
    "ConnectionString": "Endpoint=sb://...",
    "QueueName": "my-queue",
    "MaxConcurrentCalls": 20,
    "PrefetchCount": 100,
    "ReceiveMode": "PeekLock",
    "RetryPolicy": "Default",
    "AutoComplete": true
  }
}
```

**After (appsettings.json):**

```json
{
  "azureServiceBusQueue": {
    "Enabled": true,
    "ConnectionString": "Endpoint=sb://...",
    "QueueName": "my-queue",
    "MaxConcurrentCalls": 20,
    "PrefetchCount": 100,
    "ReceiveMode": "PeekLock",
    "AutoComplete": true
  }
}
```

> Simply remove the `RetryPolicy` key from your configuration. The same applies to the `azureServiceBusTopic` section.

### 2. Configuration — `ReceiveMode` Enum Values

The enum type changed from `Microsoft.Azure.ServiceBus.ReceiveMode` to `Azure.Messaging.ServiceBus.ServiceBusReceiveMode`. The string values remain the same:

| Old Enum | New Enum | JSON Value |
|----------|----------|------------|
| `ReceiveMode.PeekLock` | `ServiceBusReceiveMode.PeekLock` | `"PeekLock"` |
| `ReceiveMode.ReceiveAndDelete` | `ServiceBusReceiveMode.ReceiveAndDelete` | `"ReceiveAndDelete"` |

No changes needed in `appsettings.json` — the string values are identical.

### 3. Serialization — Newtonsoft.Json Replaced with System.Text.Json

Message serialization/deserialization now uses `System.Text.Json` instead of `Newtonsoft.Json`. If your command or event classes rely on Newtonsoft-specific attributes, you will need to update them.

**Before:**

```csharp
using Newtonsoft.Json;

public class CreateOrderCommand : ICommand
{
    [JsonProperty("orderId")]
    public string OrderId { get; set; }

    [JsonIgnore]
    public string InternalField { get; set; }
}
```

**After:**

```csharp
using System.Text.Json.Serialization;

public class CreateOrderCommand : ICommand
{
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }

    [JsonIgnore]
    public string InternalField { get; set; }
}
```

#### Common Attribute Mappings

| Newtonsoft.Json | System.Text.Json |
|-----------------|------------------|
| `[JsonProperty("name")]` | `[JsonPropertyName("name")]` |
| `[JsonIgnore]` | `[JsonIgnore]` |
| `[JsonConverter(typeof(...))]` | `[JsonConverter(typeof(...))]` |
| `[JsonConstructor]` | `[JsonConstructor]` |

### 4. Message Properties — `Label` → `Subject`, `UserProperties` → `ApplicationProperties`

If you interact with `ServiceBusMessage` objects directly (outside this library), note the following property renames:

| Old Property | New Property |
|-------------|-------------|
| `Message.Label` | `ServiceBusMessage.Subject` |
| `Message.UserProperties` | `ServiceBusMessage.ApplicationProperties` |
| `Message.Body` (byte[]) | `ServiceBusMessage.Body` (BinaryData) |
| `Message.SystemProperties.LockToken` | Accessed via `ProcessMessageEventArgs` |

### 5. Disposal — `IAsyncDisposable`

Both `AzureServiceBusQueue` and `AzureServiceBusTopic` now implement `IAsyncDisposable`. If you instantiate them manually (not via DI), ensure you dispose them properly:

```csharp
await using var queue = new AzureServiceBusQueue(options, serviceProvider, logger);
// ... use the queue
// Automatically disposed at end of scope
```

When using dependency injection, register them as singletons and ensure the host disposes them on shutdown:

```csharp
services.AddSingleton<IAzureServiceBusQueue, AzureServiceBusQueue>();
services.AddSingleton<IAzureServiceBusTopic, AzureServiceBusTopic>();
```

---

## Step-by-Step Upgrade

1. **Update the Genocs.Messaging.AzureServiceBus package** to the latest version.

2. **Remove `RetryPolicy`** from your `appsettings.json` (both `azureServiceBusQueue` and `azureServiceBusTopic` sections).

3. **Update serialization attributes** on your command/event classes if you used Newtonsoft.Json-specific attributes (see table above).

4. **Verify your connection string** — the format is unchanged. Both the old and new SDKs use the same connection string format:
   ```
   Endpoint=sb://<namespace>.servicebus.windows.net/;SharedAccessKeyName=<key-name>;SharedAccessKey=<key>
   ```

5. **Build and test** your application. The public API of `IAzureServiceBusQueue` and `IAzureServiceBusTopic` interfaces is unchanged — `SendAsync`, `ScheduleAsync`, `Consume`, `PublishAsync`, and `Subscribe` all have the same signatures.

---

## Configuration Reference

### Queue Options (`azureServiceBusQueue`)

```json
{
  "azureServiceBusQueue": {
    "Enabled": true,
    "ConnectionString": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key",
    "QueueName": "your-queue-name",
    "MaxConcurrentCalls": 20,
    "PrefetchCount": 100,
    "ReceiveMode": "PeekLock",
    "AutoComplete": true
  }
}
```

### Topic Options (`azureServiceBusTopic`)

```json
{
  "azureServiceBusTopic": {
    "Enabled": true,
    "ConnectionString": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key",
    "TopicName": "your-topic-name",
    "SubscriptionName": "your-subscription-name",
    "MaxConcurrentCalls": 20,
    "PrefetchCount": 100,
    "ReceiveMode": "PeekLock",
    "AutoComplete": true
  }
}
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Enabled` | `bool` | `false` | Enables the service bus integration. |
| `ConnectionString` | `string` | — | The Azure Service Bus connection string. |
| `QueueName` / `TopicName` | `string` | — | The name of the queue or topic. |
| `SubscriptionName` | `string` | — | (Topic only) The subscription name. |
| `MaxConcurrentCalls` | `int` | `20` | Max number of messages processed concurrently. |
| `PrefetchCount` | `int` | `100` | Number of messages prefetched for performance. |
| `ReceiveMode` | `string` | `PeekLock` | `PeekLock` or `ReceiveAndDelete`. |
| `AutoComplete` | `bool` | `true` | Whether messages are auto-completed after processing. |

---

## Further Reading

- [Azure.Messaging.ServiceBus migration guide (Microsoft)](https://learn.microsoft.com/en-us/azure/service-bus-messaging/migrate-to-azure-messaging-service-bus)
- [Azure.Messaging.ServiceBus NuGet](https://www.nuget.org/packages/Azure.Messaging.ServiceBus)
- [Azure.Messaging.ServiceBus API reference](https://learn.microsoft.com/en-us/dotnet/api/azure.messaging.servicebus)
