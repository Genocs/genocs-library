using Genocs.Core.Interfaces;
using Genocs.ServiceBusAzure.Options;
using Genocs.ServiceBusAzure.Queues.Interfaces;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genocs.ServiceBusAzure.Queues
{
    /// <summary>
    /// Todo
    /// </summary>
    public class AzureServiceBusQueue : IAzureServiceBusQueue
    {
        private readonly IQueueClient _queueClient;
        private readonly AzureServiceBusQueueSettings _options;
        private readonly ILogger<AzureServiceBusQueue> _logger;
        private Dictionary<string, KeyValuePair<Type, Type>> _handlers = new Dictionary<string, KeyValuePair<Type, Type>>();
        private const string COMMAND_SUFFIX = "Command";
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Todo
        /// </summary>
        /// <param name="options"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AzureServiceBusQueue(IOptions<AzureServiceBusQueueSettings> options,
                                    IServiceProvider serviceProvider, 
                                    ILogger<AzureServiceBusQueue> logger)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            _options = options.Value;

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            ServiceBusConnectionStringBuilder serviceBusConnectionStringBuilder = new ServiceBusConnectionStringBuilder(_options.ConnectionString);
            serviceBusConnectionStringBuilder.EntityPath = _options.QueueName;
            _queueClient = new QueueClient(serviceBusConnectionStringBuilder, _options.ReceiveMode, _options.RetryPolicy)
            {
                PrefetchCount = _options.PrefetchCount
            };

            RegisterQueueMessageHandlerAndProcess();
        }

        /// <summary>
        /// Todo
        /// </summary>
        /// <param name="options"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AzureServiceBusQueue(AzureServiceBusQueueSettings options,
                                       IServiceProvider serviceProvider, 
                                       ILogger<AzureServiceBusQueue> logger)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            _options = options;

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            ServiceBusConnectionStringBuilder serviceBusConnectionStringBuilder = new ServiceBusConnectionStringBuilder(_options.ConnectionString);
            serviceBusConnectionStringBuilder.EntityPath = _options.QueueName;
            _queueClient = new QueueClient(serviceBusConnectionStringBuilder, _options.ReceiveMode, _options.RetryPolicy)
            {
                PrefetchCount = _options.PrefetchCount
            };

            RegisterQueueMessageHandlerAndProcess();
        }

        /// <summary>
        /// Todo
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task SendAsync(ICommand command)
        {
            var jsonMessage = JsonConvert.SerializeObject(command);
            var body = Encoding.UTF8.GetBytes(jsonMessage);
            var commandName = command.GetType().Name.Replace(COMMAND_SUFFIX, "");
            var message = new Message
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = body,
                Label = commandName
            };

            await _queueClient.SendAsync(message);
        }

        /// <summary>
        /// Todo
        /// </summary>
        /// <param name="command"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public async Task ScheduleAsync(ICommand command, DateTimeOffset offset)
        {
            var jsonMessage = JsonConvert.SerializeObject(command);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            var message = new Message
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = body
            };

            await _queueClient.ScheduleMessageAsync(message, offset);
        }

        /// <summary>
        /// Todo
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        public void Consume<T, TH>() where T : ICommand where TH : ICommandHandler<T>
        {
            var eventName = typeof(T).Name;
            if (!_handlers.ContainsKey(eventName))
            {
                _handlers.Add(eventName, new KeyValuePair<Type, Type>(typeof(T), typeof(TH)));
            }
        }

        private void RegisterQueueMessageHandlerAndProcess()
        {
            _queueClient.RegisterMessageHandler(
                async (message, token) =>
                {
                    var eventName = $"{message.Label}{COMMAND_SUFFIX}";
                    var messageData = Encoding.UTF8.GetString(message.Body);
                    // Complete the message so that it is not received again.
                    if (await ProcessQueueMessages(eventName, messageData))
                    {
                        await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
                    }
                },
                new MessageHandlerOptions(ExceptionReceivedHandler) { MaxConcurrentCalls = _options.MaxConcurrentCalls, AutoComplete = false });
        }

        private async Task<bool> ProcessQueueMessages(string eventName, string message)
        {
            var processed = false;
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
                            var command = JsonConvert.DeserializeObject(message, eventType);
                            var concreteType = typeof(ICommandHandler<>).MakeGenericType(eventType);
                            await (Task)concreteType.GetMethod("HandleCommand").Invoke(handler, new object[] { command });
                        }

                    }
                }
                processed = true;
            }
            return processed;
        }


        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var ex = exceptionReceivedEventArgs.Exception;
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            _logger.LogError($"ERROR handling message: {ex.Message} ", ex);
            return Task.CompletedTask;
        }

        //public void Register(Func<IQueueClient, Message, CancellationToken, Task> callback, Func<ExceptionReceivedEventArgs, Task> exceptionHandler, MessageHandlerOptions handlerOptions = null)
        //{
        //    if (handlerOptions == null)
        //        handlerOptions = new MessageHandlerOptions(exceptionHandler) { MaxConcurrentCalls = 10, AutoComplete = true };

        //    _client.RegisterMessageHandler((msg, ct) => callback(_client, msg, ct), handlerOptions);
        //}

    }
}
