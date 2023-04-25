using RabbitMQ.Client.Events;

namespace Genocs.MessageBrokers.RabbitMQ;

public interface IRabbitMqPlugin
{
    Task HandleAsync(object message, object correlationContext, BasicDeliverEventArgs args);
}