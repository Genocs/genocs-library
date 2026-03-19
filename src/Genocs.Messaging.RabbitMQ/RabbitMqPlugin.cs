using Genocs.Messaging.RabbitMQ.Plugins;
using RabbitMQ.Client.Events;

namespace Genocs.Messaging.RabbitMQ;

public abstract class RabbitMQPlugin : IRabbitMqPlugin, IRabbitMqPluginAccessor
{
    private Func<object, object, BasicDeliverEventArgs, Task> _successor;

    public abstract Task HandleAsync(object message, object correlationContext, BasicDeliverEventArgs args);

    public Task Next(object message, object correlationContext, BasicDeliverEventArgs args)
        => _successor(message, correlationContext, args);

    void IRabbitMqPluginAccessor.SetSuccessor(Func<object, object, BasicDeliverEventArgs, Task> successor)
        => _successor = successor;
}