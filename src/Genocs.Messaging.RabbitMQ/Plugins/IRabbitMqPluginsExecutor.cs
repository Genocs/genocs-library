using RabbitMQ.Client.Events;

namespace Genocs.Messaging.RabbitMQ.Plugins;

internal interface IRabbitMqPluginsExecutor
{
    Task ExecuteAsync(Func<object, object, BasicDeliverEventArgs, Task> successor,
        object message, object correlationContext, BasicDeliverEventArgs args);
}