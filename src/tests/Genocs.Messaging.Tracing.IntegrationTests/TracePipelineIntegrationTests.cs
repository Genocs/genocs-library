using System.Diagnostics;
using System.Reflection;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;
using RabbitMQ.Client;
using Xunit;

namespace Genocs.Messaging.Tracing.IntegrationTests;

public sealed class TracePipelineIntegrationTests
{
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    [Fact]
    public void RabbitMqPipeline_ShouldCreateConnectedTrace_WithInMemoryExporter()
    {
        var exportedSpans = new List<Activity>();

        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("Genocs.WebApi")
            .AddSource("Genocs.Messaging.RabbitMQ")
            .AddInMemoryExporter(exportedSpans)
            .Build();

        using var webApiSource = new ActivitySource("Genocs.WebApi");
        using var rabbitSource = new ActivitySource("Genocs.Messaging.RabbitMQ");

        var properties = new BasicProperties
        {
            Headers = new Dictionary<string, object?>()
        };

        using (var webApiActivity = webApiSource.StartActivity("http.request", ActivityKind.Server)
            ?? throw new InvalidOperationException("Failed to start web API activity."))
        using (var producerActivity = rabbitSource.StartActivity("rabbitmq.publish", ActivityKind.Producer)
            ?? throw new InvalidOperationException("Failed to start RabbitMQ producer activity."))
        {
            InvokeRabbitMqHeaderInjection(properties, producerActivity);

            var carrier = ConvertHeadersToCarrier(properties.Headers);
            var parentContext = Propagator.Extract(default, carrier, ExtractHeaderValues);

            using var consumerActivity = rabbitSource.StartActivity(
                "rabbitmq.process",
                ActivityKind.Consumer,
                parentContext.ActivityContext)
                ?? throw new InvalidOperationException("Failed to start RabbitMQ consumer activity.");
        }

        tracerProvider.ForceFlush();

        AssertTraceChain(exportedSpans, "http.request", "rabbitmq.publish", "rabbitmq.process");
    }

    [Fact]
    public void AzureServiceBusPipeline_ShouldCreateConnectedTrace_WithInMemoryExporter()
    {
        var exportedSpans = new List<Activity>();

        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("Genocs.WebApi")
            .AddSource("Genocs.Messaging.AzureServiceBus")
            .AddInMemoryExporter(exportedSpans)
            .Build();

        using var webApiSource = new ActivitySource("Genocs.WebApi");
        using var azureSource = new ActivitySource("Genocs.Messaging.AzureServiceBus");

        var applicationProperties = new Dictionary<string, object?>();

        using (var webApiActivity = webApiSource.StartActivity("http.request", ActivityKind.Server)
            ?? throw new InvalidOperationException("Failed to start web API activity."))
        using (var producerActivity = azureSource.StartActivity("azureservicebus.publish", ActivityKind.Producer)
            ?? throw new InvalidOperationException("Failed to start Azure Service Bus producer activity."))
        {
            InvokeAzureServiceBusHeaderInjection(applicationProperties);

            var carrier = ConvertHeadersToCarrier(applicationProperties);
            var parentContext = Propagator.Extract(default, carrier, ExtractHeaderValues);

            using var consumerActivity = azureSource.StartActivity(
                "azureservicebus.process",
                ActivityKind.Consumer,
                parentContext.ActivityContext)
                ?? throw new InvalidOperationException("Failed to start Azure Service Bus consumer activity.");
        }

        tracerProvider.ForceFlush();

        AssertTraceChain(exportedSpans, "http.request", "azureservicebus.publish", "azureservicebus.process");
    }

    private static void InvokeRabbitMqHeaderInjection(BasicProperties properties, Activity activity)
    {
        Type rabbitClientType = Type.GetType("Genocs.Messaging.RabbitMQ.Clients.RabbitMQClient, Genocs.Messaging.RabbitMQ")
            ?? throw new InvalidOperationException("RabbitMQClient type not found.");

        MethodInfo includeTraceHeaders = rabbitClientType.GetMethod(
            "IncludeTraceHeaders",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("IncludeTraceHeaders method not found.");

        includeTraceHeaders.Invoke(null, [properties, activity, null]);
    }

    private static void InvokeAzureServiceBusHeaderInjection(IDictionary<string, object?> applicationProperties)
    {
        Type topicType = Type.GetType("Genocs.Messaging.AzureServiceBus.Topics.AzureServiceBusTopic, Genocs.Messaging.AzureServiceBus")
            ?? throw new InvalidOperationException("AzureServiceBusTopic type not found.");

        MethodInfo injectTraceContext = topicType.GetMethod(
            "InjectTraceContext",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("InjectTraceContext method not found.");

        injectTraceContext.Invoke(null, [applicationProperties]);
    }

    private static Dictionary<string, string> ConvertHeadersToCarrier(IDictionary<string, object?>? source)
    {
        var carrier = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (source is null)
        {
            return carrier;
        }

        foreach (var (key, value) in source)
        {
            if (string.IsNullOrWhiteSpace(key) || value is null)
            {
                continue;
            }

            carrier[key] = value switch
            {
                byte[] bytes => System.Text.Encoding.UTF8.GetString(bytes),
                ReadOnlyMemory<byte> memory => System.Text.Encoding.UTF8.GetString(memory.Span),
                string text => text,
                _ => value.ToString() ?? string.Empty
            };
        }

        return carrier;
    }

    private static IEnumerable<string> ExtractHeaderValues(Dictionary<string, string> carrier, string key)
    {
        if (carrier.TryGetValue(key, out string? value) && !string.IsNullOrWhiteSpace(value))
        {
            return [value];
        }

        return [];
    }

    private static void AssertTraceChain(
        IReadOnlyCollection<Activity> exportedSpans,
        string rootSpanName,
        string producerSpanName,
        string consumerSpanName)
    {
        Activity root = exportedSpans.Single(span => span.DisplayName == rootSpanName);
        Activity producer = exportedSpans.Single(span => span.DisplayName == producerSpanName);
        Activity consumer = exportedSpans.Single(span => span.DisplayName == consumerSpanName);

        Assert.Equal(root.TraceId, producer.TraceId);
        Assert.Equal(root.TraceId, consumer.TraceId);
        Assert.Equal(root.SpanId, producer.ParentSpanId);
        Assert.Equal(producer.SpanId, consumer.ParentSpanId);
    }
}
