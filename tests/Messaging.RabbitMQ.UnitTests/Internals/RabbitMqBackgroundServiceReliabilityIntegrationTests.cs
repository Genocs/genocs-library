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

public class RabbitMqBackgroundServiceReliabilityIntegrationTests
{
    [Fact]
    public async Task SuccessfulHandler_AcksMessage()
    {
        await using var harness = await RabbitMqHarness.CreateAsync();

        await harness.SubscribersChannel.Writer.WriteAsync(MessageSubscriber.Subscribe(
            typeof(TestMessage),
            (_, _, _) => Task.CompletedTask));

        await harness.DeliverAsync(1, "message-ok", "corr-ok");

        await harness.Channel.Received(1).BasicAckAsync(1UL, false, Arg.Any<CancellationToken>());
        await harness.Channel.DidNotReceiveWithAnyArgs().BasicNackAsync(default, default, default, default);
    }

    [Fact]
    public async Task FailedHandler_RetriesThenNacks_WhenRetriesExhausted()
    {
        int handlerAttempts = 0;

        await using var harness = await RabbitMqHarness.CreateAsync(options =>
        {
            options.Retries = 1;
            options.RetryInterval = 1;
            options.DeadLetter = new RabbitMQOptions.DeadLetterOptions
            {
                Enabled = false,
                Prefix = "dlx.",
                Suffix = ".dead",
                Declare = false,
                Durable = true,
                Exclusive = false,
                AutoDelete = false
            };
        });

        await harness.SubscribersChannel.Writer.WriteAsync(MessageSubscriber.Subscribe(
            typeof(TestMessage),
            (_, _, _) =>
            {
                Interlocked.Increment(ref handlerAttempts);
                throw new InvalidOperationException("expected retry failure");
            }));

        await harness.DeliverAsync(1, "message-retry-fail", "corr-retry-fail");

        Assert.Equal(2, handlerAttempts);
        await harness.Channel.Received(1).BasicNackAsync(1UL, false, false, Arg.Any<CancellationToken>());
        await harness.Channel.DidNotReceive().BasicAckAsync(Arg.Any<ulong>(), false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FailedHandler_WithDeadLetterEnabled_NacksToDeadLetterFlow()
    {
        await using var harness = await RabbitMqHarness.CreateAsync(
            options =>
            {
                options.Retries = 0;
                options.DeadLetter = new RabbitMQOptions.DeadLetterOptions
                {
                    Enabled = true,
                    Prefix = "dlx.",
                    Suffix = ".dead",
                    Declare = false,
                    Durable = true,
                    Exclusive = false,
                    AutoDelete = false
                };
            },
            mapper: new DeadLetterFailedMessageMapper());

        await harness.SubscribersChannel.Writer.WriteAsync(MessageSubscriber.Subscribe(
            typeof(TestMessage),
            (_, _, _) => throw new InvalidOperationException("move to dead letter")));

        await harness.DeliverAsync(1, "message-dlx", "corr-dlx");

        await harness.Channel.Received(1).BasicNackAsync(1UL, false, false, Arg.Any<CancellationToken>());
        await harness.Channel.DidNotReceive().BasicAckAsync(Arg.Any<ulong>(), false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandlerExecution_ResolvesNewScopedServices_PerMessage()
    {
        var scopeIds = new ConcurrentQueue<Guid>();

        await using var harness = await RabbitMqHarness.CreateAsync();
        await harness.SubscribersChannel.Writer.WriteAsync(MessageSubscriber.Subscribe(
            typeof(TestMessage),
            (sp, _, _) =>
            {
                scopeIds.Enqueue(sp.GetRequiredService<ScopedProbe>().Id);
                return Task.CompletedTask;
            }));

        await harness.DeliverAsync(1, "message-scope-1", "corr-scope-1");
        await harness.DeliverAsync(2, "message-scope-2", "corr-scope-2");

        await WaitUntilAsync(() => scopeIds.Count >= 2, TimeSpan.FromSeconds(5));
        Guid[] resolved = scopeIds.ToArray();

        Assert.Equal(2, resolved.Length);
        Assert.NotEqual(resolved[0], resolved[1]);
        await harness.Channel.Received(2).BasicAckAsync(Arg.Any<ulong>(), false, Arg.Any<CancellationToken>());
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

    private sealed class RabbitMqHarness : IAsyncDisposable
    {
        private readonly ServiceProvider _rootProvider;
        private readonly RabbitMqBackgroundService _service;
        private readonly Task<IAsyncBasicConsumer> _consumerTask;

        private RabbitMqHarness(
            ServiceProvider rootProvider,
            RabbitMqBackgroundService service,
            IChannel channel,
            MessageSubscribersChannel subscribersChannel,
            Task<IAsyncBasicConsumer> consumerTask)
        {
            _rootProvider = rootProvider;
            _service = service;
            _consumerTask = consumerTask;
            Channel = channel;
            SubscribersChannel = subscribersChannel;
        }

        public IChannel Channel { get; }

        public MessageSubscribersChannel SubscribersChannel { get; }

        public static async Task<RabbitMqHarness> CreateAsync(
            Action<RabbitMQOptions>? configureOptions = null,
            IExceptionToFailedMessageMapper? mapper = null)
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

            channel.QueueDeclareAsync(
                    Arg.Any<string>(),
                    Arg.Any<bool>(),
                    Arg.Any<bool>(),
                    Arg.Any<bool>(),
                    Arg.Any<IDictionary<string, object?>>(),
                    Arg.Any<bool>(),
                    Arg.Any<bool>(),
                    Arg.Any<CancellationToken>())
                .Returns(callInfo =>
                {
                    string queueName = callInfo.ArgAt<string>(0);
                    return Task.FromResult(new QueueDeclareOk(queueName, 0, 0));
                });

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
            services.AddSingleton<ILogger<RabbitMQSubscriber>>(NullLogger<RabbitMQSubscriber>.Instance);
            services.AddScoped<ScopedProbe>();
            services.AddScoped<IMessagePropertiesAccessor, MessagePropertiesAccessor>();
            services.AddScoped<ICorrelationContextAccessor, CorrelationContextAccessor>();

            if (mapper is not null)
            {
                services.AddSingleton<IExceptionToFailedMessageMapper>(mapper);
            }

            var options = new RabbitMQOptions
            {
                Queue = new RabbitMQOptions.QueueOptions
                {
                    Declare = false,
                    Durable = true,
                    Exclusive = false,
                    AutoDelete = false,
                    Template = "{messageName}"
                },
                Exchange = new RabbitMQOptions.ExchangeOptions
                {
                    Name = "exchange.test",
                    Type = "topic",
                    Declare = false,
                    Durable = true,
                    AutoDelete = false
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
            };

            configureOptions?.Invoke(options);
            services.AddSingleton(options);

            ServiceProvider rootProvider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true
            });

            var service = new RabbitMqBackgroundService(rootProvider);
            await service.StartAsync(CancellationToken.None);

            return new RabbitMqHarness(
                rootProvider,
                service,
                channel,
                subscribersChannel,
                consumerReady.Task.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        public async Task DeliverAsync(ulong deliveryTag, string messageId, string correlationId)
        {
            IAsyncBasicConsumer consumer = await _consumerTask;

            await consumer.HandleBasicDeliverAsync(
                "consumer-tag",
                deliveryTag,
                false,
                "exchange.test",
                "messages.test",
                CreateBasicProperties(messageId, correlationId),
                new ReadOnlyMemory<byte>(new byte[] { 1 }));
        }

        public async ValueTask DisposeAsync()
        {
            await _service.StopAsync(CancellationToken.None);
            _service.Dispose();
            await _rootProvider.DisposeAsync();
        }
    }

    private sealed class DeadLetterFailedMessageMapper : IExceptionToFailedMessageMapper
    {
        public FailedMessage? Map(Exception exception, object message) => new(shouldRetry: false, moveToDeadLetter: true);
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