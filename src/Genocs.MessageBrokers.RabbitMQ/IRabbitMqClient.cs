namespace Genocs.MessageBrokers.RabbitMQ;

public interface IRabbitMQClient
{
    void Send(
                object? message,
                IConventions conventions,
                string? messageId = null,
                string? correlationId = null,
                string? spanContext = null,
                object? messageContext = null,
                IDictionary<string, object>? headers = null);
}