using RabbitMQ.Client.Events;

namespace Genocs.Messaging.RabbitMQ;

public interface IRabbitMqPlugin
{
    Task HandleAsync(object message, object correlationContext, BasicDeliverEventArgs args);
}