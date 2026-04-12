using System.Diagnostics;
using System.Reflection;
using Genocs.Messaging.RabbitMQ;
using RabbitMQ.Client;
using Xunit;

namespace Genocs.Messaging.Tracing.UnitTests;

public class TraceContextPropagationUnitTests
{
    [Fact]
    public void RabbitMqIncludeTraceHeaders_ShouldPopulateTraceParentAndTraceState()
    {
        Type rabbitClientType = Type.GetType("Genocs.Messaging.RabbitMQ.Clients.RabbitMQClient, Genocs.Messaging.RabbitMQ")
            ?? throw new InvalidOperationException("RabbitMQClient type not found.");

        MethodInfo includeTraceHeaders = rabbitClientType.GetMethod(
            "IncludeTraceHeaders",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("IncludeTraceHeaders method not found.");

        var properties = new BasicProperties
        {
            Headers = new Dictionary<string, object?>()
        };

        using var activity = new Activity("publish");
        activity.SetIdFormat(ActivityIdFormat.W3C);
        activity.TraceStateString = "vendor=value";
        activity.Start();

        includeTraceHeaders.Invoke(null, [properties, activity, null]);

        Assert.NotNull(properties.Headers);
        Assert.True(properties.Headers!.ContainsKey("traceparent"));
        Assert.Equal(activity.Id, properties.Headers["traceparent"]?.ToString());
        Assert.True(properties.Headers.ContainsKey("tracestate"));
        Assert.Equal("vendor=value", properties.Headers["tracestate"]?.ToString());
    }

    [Fact]
    public void RabbitMqIncludeTraceHeaders_ShouldUseFallbackTraceParent()
    {
        Type rabbitClientType = Type.GetType("Genocs.Messaging.RabbitMQ.Clients.RabbitMQClient, Genocs.Messaging.RabbitMQ")
            ?? throw new InvalidOperationException("RabbitMQClient type not found.");

        MethodInfo includeTraceHeaders = rabbitClientType.GetMethod(
            "IncludeTraceHeaders",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("IncludeTraceHeaders method not found.");

        var properties = new BasicProperties
        {
            Headers = new Dictionary<string, object?>()
        };

        const string fallbackTraceParent = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";

        includeTraceHeaders.Invoke(null, [properties, null, fallbackTraceParent]);

        Assert.NotNull(properties.Headers);
        Assert.True(properties.Headers!.ContainsKey("traceparent"));
        Assert.Equal(fallbackTraceParent, properties.Headers["traceparent"]?.ToString());
    }

    [Fact]
    public void AzureServiceBusQueueInjectTraceContext_ShouldPopulateTraceHeaders()
    {
        Type queueType = Type.GetType("Genocs.Messaging.AzureServiceBus.Queues.AzureServiceBusQueue, Genocs.Messaging.AzureServiceBus")
            ?? throw new InvalidOperationException("AzureServiceBusQueue type not found.");

        MethodInfo injectMethod = queueType.GetMethod(
            "InjectTraceContext",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("InjectTraceContext method for queue not found.");

        var applicationProperties = new Dictionary<string, object>();

        using var activity = new Activity("queue-send");
        activity.SetIdFormat(ActivityIdFormat.W3C);
        activity.TraceStateString = "queue=v1";
        activity.Start();

        injectMethod.Invoke(null, [applicationProperties]);

        Assert.True(applicationProperties.ContainsKey("traceparent"));
        Assert.Equal(activity.Id, applicationProperties["traceparent"]?.ToString());
        Assert.True(applicationProperties.ContainsKey("tracestate"));
        Assert.Equal("queue=v1", applicationProperties["tracestate"]?.ToString());
    }

    [Fact]
    public void AzureServiceBusTopicInjectTraceContext_ShouldPopulateTraceHeaders()
    {
        Type topicType = Type.GetType("Genocs.Messaging.AzureServiceBus.Topics.AzureServiceBusTopic, Genocs.Messaging.AzureServiceBus")
            ?? throw new InvalidOperationException("AzureServiceBusTopic type not found.");

        MethodInfo injectMethod = topicType.GetMethod(
            "InjectTraceContext",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("InjectTraceContext method for topic not found.");

        var applicationProperties = new Dictionary<string, object>();

        using var activity = new Activity("topic-publish");
        activity.SetIdFormat(ActivityIdFormat.W3C);
        activity.TraceStateString = "topic=v1";
        activity.Start();

        injectMethod.Invoke(null, [applicationProperties]);

        Assert.True(applicationProperties.ContainsKey("traceparent"));
        Assert.Equal(activity.Id, applicationProperties["traceparent"]?.ToString());
        Assert.True(applicationProperties.ContainsKey("tracestate"));
        Assert.Equal("topic=v1", applicationProperties["tracestate"]?.ToString());
    }

    [Fact]
    public async Task RabbitMqPublisherPublishAsync_ShouldReturnUnderlyingSendTask()
    {
        Type publisherType = Type.GetType("Genocs.Messaging.RabbitMQ.Publishers.RabbitMQPublisher, Genocs.Messaging.RabbitMQ")
            ?? throw new InvalidOperationException("RabbitMQPublisher type not found.");

        var client = new DelayedRabbitMqClient();
        var conventionsProvider = new StaticConventionsProvider();

        object publisher = Activator.CreateInstance(publisherType, client, conventionsProvider)
            ?? throw new InvalidOperationException("Unable to create RabbitMQPublisher instance.");

        MethodInfo publishMethod = publisherType
            .GetMethod("PublishAsync")
            ?.MakeGenericMethod(typeof(TestMessage))
            ?? throw new InvalidOperationException("PublishAsync method not found.");

        var publishTask = (Task)publishMethod.Invoke(publisher,
            [new TestMessage(), null, null, null, null, null, CancellationToken.None])!;

        Assert.False(publishTask.IsCompleted);

        client.SendCompletionSource.SetResult();
        await publishTask;
    }

    private sealed class TestMessage;

    private sealed class StaticConventionsProvider : IConventionsProvider
    {
        private static readonly IConventions Conventions = new StaticConventions();

        public IConventions Get<T>() => Conventions;

        public IConventions Get(Type type) => Conventions;
    }

    private sealed class StaticConventions : IConventions
    {
        public Type Type => typeof(TestMessage);
        public string RoutingKey => "test.route";
        public string Exchange => "test.exchange";
        public string Queue => "test.queue";
    }

    private sealed class DelayedRabbitMqClient : IRabbitMQClient
    {
        public TaskCompletionSource SendCompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task SendAsync(
            object message,
            IConventions conventions,
            string messageId = null,
            string correlationId = null,
            string spanContext = null,
            object? messageContext = null,
            IDictionary<string, object>? headers = null)
            => SendCompletionSource.Task;
    }
}
