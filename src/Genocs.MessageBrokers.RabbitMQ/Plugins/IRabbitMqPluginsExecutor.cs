using RabbitMQ.Client.Events;

namespace Genocs.MessageBrokers.RabbitMQ.Plugins;

internal interface IRabbitMqPluginsExecutor
{
    Task ExecuteAsync(Func<object, object, BasicDeliverEventArgs, Task> successor,
        object message, object correlationContext, BasicDeliverEventArgs args);
}