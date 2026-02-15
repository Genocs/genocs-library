using Azure.Messaging.ServiceBus;
using Genocs.Common.CQRS.Commands;
using Genocs.ServiceBusAzure.Configurations;
using Genocs.ServiceBusAzure.Queues.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Genocs.ServiceBusAzure.Queues;

/// <summary>
/// Azure Service Bus Queue implementation using Azure.Messaging.ServiceBus SDK.
/// </summary>
public class AzureServiceBusQueue : IAzureServiceBusQueue, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ServiceBusProcessor _processor;
    private readonly AzureServiceBusQueueOptions _options;
    private readonly ILogger<AzureServiceBusQueue> _logger;
    private readonly Dictionary<string, KeyValuePair<Type, Type>> _handlers = new();
    private const string COMMAND_SUFFIX = "Command";
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="AzureServiceBusQueue"/> using <see cref="IOptions{T}"/>.
    /// </summary>
    /// <param name="options">The queue configuration options.</param>
    /// <param name="serviceProvider">The service provider for resolving handlers.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public AzureServiceBusQueue(
        IOptions<AzureServiceBusQueueOptions> options,
        IServiceProvider serviceProvider,
        ILogger<AzureServiceBusQueue> logger)
        : this(options?.Value ?? throw new ArgumentNullException(nameof(options)), serviceProvider, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="AzureServiceBusQueue"/> using direct options.
    /// </summary>
    /// <param name="options">The queue configuration options.</param>
    /// <param name="serviceProvider">The service provider for resolving handlers.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public AzureServiceBusQueue(
        AzureServiceBusQueueOptions options,
        IServiceProvider serviceProvider,
        ILogger<AzureServiceBusQueue> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        _client = new ServiceBusClient(_options.ConnectionString);
        _sender = _client.CreateSender(_options.QueueName);
        _processor = _client.CreateProcessor(_options.QueueName, new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = _options.MaxConcurrentCalls,
            PrefetchCount = _options.PrefetchCount,
            ReceiveMode = _options.ReceiveMode,
            AutoCompleteMessages = false
        });

        RegisterQueueMessageHandlerAndProcess();
    }

    /// <summary>
    /// Sends a command to the queue.
    /// </summary>
    /// <param name="command">The command to send.</param>
    public async Task SendAsync(ICommand command)
    {
        string jsonMessage = JsonSerializer.Serialize(command, command.GetType());
        string commandName = command.GetType().Name.Replace(COMMAND_SUFFIX, "");

        var message = new ServiceBusMessage(jsonMessage)
        {
            MessageId = Guid.NewGuid().ToString(),
            Subject = commandName
        };

        await _sender.SendMessageAsync(message);
    }

    /// <summary>
    /// Schedules a command to be sent at a specified time.
    /// </summary>
    /// <param name="command">The command to schedule.</param>
    /// <param name="offset">The time at which the message should be enqueued.</param>
    public async Task ScheduleAsync(ICommand command, DateTimeOffset offset)
    {
        string jsonMessage = JsonSerializer.Serialize(command, command.GetType());

        var message = new ServiceBusMessage(jsonMessage)
        {
            MessageId = Guid.NewGuid().ToString()
        };

        await _sender.ScheduleMessageAsync(message, offset);
    }

    /// <summary>
    /// Registers a command handler for consuming messages from the queue.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <typeparam name="TH">The command handler type.</typeparam>
    public void Consume<T, TH>() where T : ICommand where TH : ICommandHandlerLegacy<T>
    {
        string eventName = typeof(T).Name;
        if (!_handlers.ContainsKey(eventName))
        {
            _handlers.Add(eventName, new KeyValuePair<Type, Type>(typeof(T), typeof(TH)));
        }
    }

    private void RegisterQueueMessageHandlerAndProcess()
    {
        _processor.ProcessMessageAsync += async (args) =>
        {
            string eventName = $"{args.Message.Subject}{COMMAND_SUFFIX}";
            string messageData = args.Message.Body.ToString();

            // Complete the message so that it is not received again.
            if (await ProcessQueueMessages(eventName, messageData))
            {
                await args.CompleteMessageAsync(args.Message);
            }
        };

        _processor.ProcessErrorAsync += ExceptionReceivedHandler;

        _processor.StartProcessingAsync().GetAwaiter().GetResult();
    }

    private async Task<bool> ProcessQueueMessages(string eventName, string message)
    {
        bool processed = false;
        if (_handlers.ContainsKey(eventName))
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var type = _handlers[eventName];
                if (type.Key != null && type.Value != null)
                {
                    var handler = scope.ServiceProvider.GetRequiredService(type.Value);
                    if (handler != null)
                    {
                        var eventType = type.Key;
                        var command = JsonSerializer.Deserialize(message, eventType);
                        var concreteType = typeof(ICommandHandler<>).MakeGenericType(eventType);
                        await (Task)concreteType.GetMethod("HandleCommand")!.Invoke(handler, new object[] { command! })!;
                    }
                }
            }

            processed = true;
        }

        return processed;
    }

    private Task ExceptionReceivedHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "ERROR handling message: {ErrorMessage} - Source: {ErrorSource}",
            args.Exception.Message, args.ErrorSource);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the Service Bus client, sender, and processor.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _processor.StopProcessingAsync();
        await _processor.DisposeAsync();
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
