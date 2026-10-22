namespace Genocs.Messaging.RabbitMQ;

public interface IRabbitMQClient
{
    Task SendAsync(
                    object message,
                    IConventions conventions,
                    string? messageId = null,
                    string? correlationId = null,
                    string? spanContext = null,
                    object? messageContext = null,
                    IDictionary<string, object>? headers = null);
}