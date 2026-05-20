using System.Collections.Concurrent;
using Genocs.Messaging.RabbitMQ.Conventions;
using Genocs.Messaging.RabbitMQ.Internals;
using Genocs.Messaging.RabbitMQ.Plugins;
using Genocs.Messaging.RabbitMQ.Subscribers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xunit;

namespace Genocs.Messaging.RabbitMQ.UnitTests.Internals;

public class RabbitMqBackgroundServiceScopeTests
{
    [Fact]
    public async Task HandlerExecution_UsesScopedProvider_PerConsumedMessage()
    {
        IChannel channel = Substitute.For<IChannel>();
        IConnection consumerConnection = Substitute.For<IConnection>();
        IConnection producerConnection = Substitute.For<IConnection>();

        consumerConnection.CreateChannelAsync(Arg.Any<CreateChannelOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(channel));

        channel.QueueBindAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IDictionary<string, object?>>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        channel.BasicQosAsync(Arg.Any<uint>(), Arg.Any<ushort>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var consumerReady = new TaskCompletionSource<IAsyncBasicConsumer>(TaskCreationOptions.RunContinuationsAsynchronously);
        channel.BasicConsumeAsync(
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<bool>(),
                Arg.Any<IDictionary<string, object?>>(),
                Arg.Any<IAsyncBasicConsumer>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                consumerReady.TrySetResult(callInfo.ArgAt<IAsyncBasicConsumer>(6));
                return Task.FromResult("consumer-tag");
            });

        var subscribersChannel = new MessageSubscribersChannel();
        var resolvedScopeIds = new ConcurrentQueue<Guid>();

        var services = new ServiceCollection();
        services.AddSingleton(new ConsumerConnection(consumerConnection));
        services.AddSingleton(new ProducerConnection(producerConnection));
        services.AddSingleton(subscribersChannel);
        services.AddSingleton<IBusPublisher, NoOpBusPublisher>();
        services.AddSingleton<IRabbitMQSerializer, NoOpSerializer>();
        services.AddSingleton<IConventionsProvider>(new StaticConventionsProvider(new MessageConventions(
            typeof(TestMessage),
            "messages.test",
            "exchange.test",
            "queue.test")));
        services.AddSingleton<IContextProvider>(new StaticContextProvider());
        services.AddSingleton<IRabbitMqPluginsExecutor, PassthroughPluginsExecutor>();
        services.AddSingleton(new RabbitMQOptions
        {
            Queue = new RabbitMQOptions.QueueOptions
            {
                Declare = false,
                Durable = true,
                Exclusive = false,
                AutoDelete = false,
                Template = "{messageName}"
            },
            DeadLetter = new RabbitMQOptions.DeadLetterOptions
            {
                Enabled = false,
                Prefix = "dlx.",
                Suffix = ".dead",
                Declare = false,
                Durable = true,
                Exclusive = false,
                AutoDelete = false
            },
            Qos = new RabbitMQOptions.QosOptions
            {
                PrefetchCount = 1,
                PrefetchSize = 0,
                Global = false
            },
            Logger = new RabbitMQOptions.LoggerOptions
            {
                Enabled = false,
                LogConnectionStatus = false,
                LogMessagePayload = false
            },
            Retries = 0,
            RetryInterval = 1,
            RequeueFailedMessages = false,
            SpanContextHeader = "span_context"
        });
        services.AddSingleton<ILogger<RabbitMQSubscriber>>(NullLogger<RabbitMQSubscriber>.Instance);
        services.AddScoped<ScopedProbe>();
        services.AddScoped<IMessagePropertiesAccessor, MessagePropertiesAccessor>();
        services.AddScoped<ICorrelationContextAccessor, CorrelationContextAccessor>();

        await using ServiceProvider rootProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true
        });

        var service = new RabbitMqBackgroundService(rootProvider);
        await service.StartAsync(CancellationToken.None);

        await subscribersChannel.Writer.WriteAsync(MessageSubscriber.Subscribe(
            typeof(TestMessage),
            (sp, _, _) =>
            {
                resolvedScopeIds.Enqueue(sp.GetRequiredService<ScopedProbe>().Id);
                return Task.CompletedTask;
            }));

        IAsyncBasicConsumer registeredConsumer = await consumerReady.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await registeredConsumer.HandleBasicDeliverAsync(
            "consumer-tag",
            1,
            false,
            "exchange.test",
            "messages.test",
            CreateBasicProperties("message-1", "corr-1"),
            new ReadOnlyMemory<byte>(new byte[] { 1 }));

        await registeredConsumer.HandleBasicDeliverAsync(
            "consumer-tag",
            2,
            false,
            "exchange.test",
            "messages.test",
            CreateBasicProperties("message-2", "corr-2"),
            new ReadOnlyMemory<byte>(new byte[] { 2 }));

        await WaitUntilAsync(() => resolvedScopeIds.Count >= 2, TimeSpan.FromSeconds(5));

        Guid[] scopeIds = resolvedScopeIds.ToArray();
        Assert.Equal(2, scopeIds.Length);
        Assert.NotEqual(scopeIds[0], scopeIds[1]);
        await channel.Received(2).BasicAckAsync(Arg.Any<ulong>(), false, Arg.Any<CancellationToken>());

        await service.StopAsync(CancellationToken.None);
        service.Dispose();
    }

    [Fact]
    public async Task HandlerExecution_NacksMessage_WhenHandlerThrows()
    {
        IChannel channel = Substitute.For<IChannel>();
        IConnection consumerConnection = Substitute.For<IConnection>();
        IConnection producerConnection = Substitute.For<IConnection>();

        consumerConnection.CreateChannelAsync(Arg.Any<CreateChannelOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(channel));

        channel.QueueBindAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IDictionary<string, object?>>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        channel.BasicQosAsync(Arg.Any<uint>(), Arg.Any<ushort>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var consumerReady = new TaskCompletionSource<IAsyncBasicConsumer>(TaskCreationOptions.RunContinuationsAsynchronously);
        channel.BasicConsumeAsync(
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<bool>(),
                Arg.Any<IDictionary<string, object?>>(),
                Arg.Any<IAsyncBasicConsumer>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                consumerReady.TrySetResult(callInfo.ArgAt<IAsyncBasicConsumer>(6));
                return Task.FromResult("consumer-tag");
            });

        var subscribersChannel = new MessageSubscribersChannel();

        var services = new ServiceCollection();
        services.AddSingleton(new ConsumerConnection(consumerConnection));
        services.AddSingleton(new ProducerConnection(producerConnection));
        services.AddSingleton(subscribersChannel);
        services.AddSingleton<IBusPublisher, NoOpBusPublisher>();
        services.AddSingleton<IRabbitMQSerializer, NoOpSerializer>();
        services.AddSingleton<IConventionsProvider>(new StaticConventionsProvider(new MessageConventions(
            typeof(TestMessage),
            "messages.test",
            "exchange.test",
            "queue.test")));
        services.AddSingleton<IContextProvider>(new StaticContextProvider());
        services.AddSingleton<IRabbitMqPluginsExecutor, PassthroughPluginsExecutor>();
        services.AddSingleton(new RabbitMQOptions
        {
            Queue = new RabbitMQOptions.QueueOptions
            {
                Declare = false,
                Durable = true,
                Exclusive = false,
                AutoDelete = false,
                Template = "{messageName}"
            },
            DeadLetter = new RabbitMQOptions.DeadLetterOptions
            {
                Enabled = false,
                Prefix = "dlx.",
                Suffix = ".dead",
                Declare = false,
                Durable = true,
                Exclusive = false,
                AutoDelete = false
            },
            Qos = new RabbitMQOptions.QosOptions
            {
                PrefetchCount = 1,
                PrefetchSize = 0,
                Global = false
            },
            Logger = new RabbitMQOptions.LoggerOptions
            {
                Enabled = false,
                LogConnectionStatus = false,
                LogMessagePayload = false
            },
            Retries = 0,
            RetryInterval = 1,
            RequeueFailedMessages = false,
            SpanContextHeader = "span_context"
        });
        services.AddSingleton<ILogger<RabbitMQSubscriber>>(NullLogger<RabbitMQSubscriber>.Instance);
        services.AddScoped<IMessagePropertiesAccessor, MessagePropertiesAccessor>();
        services.AddScoped<ICorrelationContextAccessor, CorrelationContextAccessor>();

        await using ServiceProvider rootProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true
        });

        var service = new RabbitMqBackgroundService(rootProvider);
        await service.StartAsync(CancellationToken.None);

        await subscribersChannel.Writer.WriteAsync(MessageSubscriber.Subscribe(
            typeof(TestMessage),
            (_, _, _) => throw new InvalidOperationException("expected test failure")));

        IAsyncBasicConsumer registeredConsumer = await consumerReady.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await registeredConsumer.HandleBasicDeliverAsync(
            "consumer-tag",
            1,
            false,
            "exchange.test",
            "messages.test",
            CreateBasicProperties("message-1", "corr-1"),
            new ReadOnlyMemory<byte>(new byte[] { 1 }));

        await channel.Received(1).BasicNackAsync(1UL, false, false, Arg.Any<CancellationToken>());
        await channel.DidNotReceive().BasicAckAsync(Arg.Any<ulong>(), false, Arg.Any<CancellationToken>());

        await service.StopAsync(CancellationToken.None);
        service.Dispose();
    }

    private static BasicProperties CreateBasicProperties(string messageId, string correlationId)
        => new()
        {
            MessageId = messageId,
            CorrelationId = correlationId,
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            Headers = new Dictionary<string, object?>()
        };

    private static async Task WaitUntilAsync(Func<bool> predicate, TimeSpan timeout)
    {
        DateTime deadline = DateTime.UtcNow.Add(timeout);
        while (DateTime.UtcNow <= deadline)
        {
            if (predicate())
            {
                return;
            }

            await Task.Delay(25);
        }

        throw new TimeoutException("Condition was not met within the expected timeout.");
    }

    private sealed class ScopedProbe
    {
        public Guid Id { get; } = Guid.NewGuid();
    }

    private sealed record TestMessage(string Value = "payload");

    private sealed class NoOpSerializer : IRabbitMQSerializer
    {
        public ReadOnlySpan<byte> Serialize(object value) => Array.Empty<byte>();

        public object? Deserialize(ReadOnlySpan<byte> value, Type type) => new TestMessage();

        public object? Deserialize(ReadOnlySpan<byte> value) => new TestMessage();
    }

    private sealed class StaticConventionsProvider : IConventionsProvider
    {
        private readonly IConventions _conventions;

        public StaticConventionsProvider(IConventions conventions)
        {
            _conventions = conventions;
        }

        public IConventions Get<T>() => _conventions;

        public IConventions Get(Type type) => _conventions;
    }

    private sealed class StaticContextProvider : IContextProvider
    {
        public string HeaderName => "message_context";

        public object Get(IDictionary<string, object> headers) => new { Correlation = "ctx" };
    }

    private sealed class PassthroughPluginsExecutor : IRabbitMqPluginsExecutor
    {
        public Task ExecuteAsync(
            Func<object, object, BasicDeliverEventArgs, Task> successor,
            object message,
            object correlationContext,
            BasicDeliverEventArgs args)
            => successor(message, correlationContext, args);
    }

    private sealed class NoOpBusPublisher : IBusPublisher
    {
        public Task PublishAsync<T>(
            T message,
            string? messageId = null,
            string? correlationId = null,
            string? spanContext = null,
            object? messageContext = null,
            IDictionary<string, object>? headers = null,
            CancellationToken cancellationToken = default)
            where T : class
            => Task.CompletedTask;
    }
}
