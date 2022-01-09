using Genocs.Core.Interfaces;
using Genocs.ServiceBusAzure.Topics.Interfaces;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genocs.ServiceBusAzure.Topics
{
    public class AzureServiceBusTopic : IAzureServiceBusTopic
    {
        private readonly TopicClient _topicClient;
        private readonly TopicOptions _options;
        private readonly ILogger<AzureServiceBusTopic> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const string Event_SUFFIX = "Event";
        private Dictionary<string, List<SubscriptionInfo>> _handlers;
        private SubscriptionClient _subscriptionClient;
        private readonly List<Type> _eventTypes;

        public AzureServiceBusTopic(IOptions<TopicOptions> options,
                        IServiceProvider serviceProvider, ILogger<AzureServiceBusTopic> logger)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            _options = options.Value;

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            ServiceBusConnectionStringBuilder serviceBusConnectionStringBuilder = new ServiceBusConnectionStringBuilder(_options.ConnectionString);
            serviceBusConnectionStringBuilder.EntityPath = _options.TopicName;
            _topicClient = new TopicClient(serviceBusConnectionStringBuilder, _options.RetryPolicy);
            _handlers = new Dictionary<string, List<SubscriptionInfo>>();
            _eventTypes = new List<Type>();
            if (!string.IsNullOrEmpty(_options.SubscriptionName))
            {
                _subscriptionClient = new SubscriptionClient(serviceBusConnectionStringBuilder, _options.SubscriptionName);
                RegisterSubscriptionClientMessageHandler();
            }
        }


        public AzureServiceBusTopic(TopicOptions options,
                        IServiceProvider serviceProvider, ILogger<AzureServiceBusTopic> logger)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            _options = options;

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            ServiceBusConnectionStringBuilder serviceBusConnectionStringBuilder = new ServiceBusConnectionStringBuilder(_options.ConnectionString);
            serviceBusConnectionStringBuilder.EntityPath = _options.TopicName;
            _topicClient = new TopicClient(serviceBusConnectionStringBuilder, _options.RetryPolicy);
            _handlers = new Dictionary<string, List<SubscriptionInfo>>();
            _eventTypes = new List<Type>();
            if (!string.IsNullOrEmpty(_options.SubscriptionName))
            {
                _subscriptionClient = new SubscriptionClient(serviceBusConnectionStringBuilder, _options.SubscriptionName);
                RegisterSubscriptionClientMessageHandler();
            }
        }

        public async Task PublishAsync(IEvent @event)
        {
            var eventName = @event.GetType().Name.Replace(Event_SUFFIX, "");
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            var message = new Message
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = body,
                Label = eventName,
            };

            await _topicClient.SendAsync(message);

        }

        public async Task PublishAsync(IEvent @event, Dictionary<string, object> filters)
        {
            var eventName = @event.GetType().Name.Replace(Event_SUFFIX, "");
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            var message = new Message
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = body,
                Label = eventName,
            };

            foreach (KeyValuePair<string, object> filter in filters)
            {
                message.UserProperties.Add(filter);
            }



            await _topicClient.SendAsync(message);
        }

        public async Task ScheduleAsync(IEvent @event, DateTimeOffset offset)
        {
            var eventName = @event.GetType().Name.Replace(Event_SUFFIX, "");
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            var message = new Message
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = body,
                Label = eventName,
            };
            await _topicClient.ScheduleMessageAsync(message, offset);
        }

        public async Task ScheduleAsync(IEvent @event, DateTimeOffset offset, Dictionary<string, object> filters)
        {
            var eventName = @event.GetType().Name.Replace(Event_SUFFIX, "");
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            var message = new Message
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = body,
                Label = eventName,
            };

            foreach (KeyValuePair<string, object> filter in filters)
            {
                message.UserProperties.Add(filter);
            }

            await _topicClient.ScheduleMessageAsync(message, offset);
        }

        public void Subscribe<T, TH>()
            where T : IEvent
            where TH : IEventHandler<T>
        {
            var eventName = typeof(T).Name.Replace(Event_SUFFIX, "");
            var key = typeof(T).Name;
            if (!_handlers.ContainsKey(key))
            {
                _handlers.Add(key, new List<SubscriptionInfo>());

                //_subscriptionClient.AddRuleAsync(new RuleDescription
                //{
                //    Filter = new CorrelationFilter { Label = eventName.ToLower() },
                //    Name = eventName
                //}).GetAwaiter().GetResult();
            }
            Type handlerType = typeof(TH);

            if (_handlers[key].Any(s => s.HandlerType == handlerType))
            {
                throw new ArgumentException(
                    $"Handler Type {typeof(TH).Name} already registered for '{key}'", nameof(handlerType));
            }
            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }
            _handlers[key].Add(SubscriptionInfo.Typed(handlerType));

        }

        private void RegisterSubscriptionClientMessageHandler()
        {
            _subscriptionClient.RegisterMessageHandler(
                async (message, token) =>
                {
                    var eventName = $"{message.Label}{Event_SUFFIX}";
                    var messageData = Encoding.UTF8.GetString(message.Body);

                    // Complete the message so that it is not received again.
                    if (await ProcessEvent(eventName, messageData))
                    {
                        await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
                    }
                },
                new MessageHandlerOptions(ExceptionReceivedHandler) { MaxConcurrentCalls = _options.MaxConcurrentCalls, AutoComplete = false });
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var ex = exceptionReceivedEventArgs.Exception;
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;

            _logger.LogError(ex, "ERROR handling message: {ExceptionMessage} - Context: {@ExceptionContext}", ex.Message, context);

            return Task.CompletedTask;
        }


        private async Task<bool> ProcessEvent(string eventName, string message)
        {
            var processed = false;
            if (_handlers.ContainsKey(eventName))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var subscriptions = _handlers[eventName];

                    foreach (var subscription in subscriptions)
                    {
                        var handler = scope.ServiceProvider.GetRequiredService(subscription.HandlerType);
                        if (handler != null)
                        {
                            var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);
                            var command = JsonConvert.DeserializeObject(message, eventType);
                            var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                            await (Task)concreteType.GetMethod("HandleEvent").Invoke(handler, new object[] { command });
                        }
                    }
                }
                processed = true;
            }
            else
            {
                _logger.LogError($"Event '{eventName}' do not contains handlers. Check whether Subscribe is set");
            }
            return processed;
        }

    }
}
