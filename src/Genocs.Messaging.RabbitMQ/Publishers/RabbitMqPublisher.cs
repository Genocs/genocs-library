namespace Genocs.Messaging.RabbitMQ.Publishers;

internal sealed class RabbitMQPublisher : IBusPublisher
{
    private readonly IRabbitMQClient _client;
    private readonly IConventionsProvider _conventionsProvider;

    public RabbitMQPublisher(IRabbitMQClient client, IConventionsProvider conventionsProvider)
    {
        _client = client;
        _conventionsProvider = conventionsProvider;
    }

    public Task PublishAsync<T>(
                                T message,
                                string? messageId = null,
                                string? correlationId = null,
                                string? spanContext = null,
                                object? messageContext = null,
                                IDictionary<string, object>? headers = null,
                                CancellationToken cancellationToken = default)
        where T : class
    {
        _client.SendAsync(
                            message,
                            _conventionsProvider.Get(message.GetType()),
                            messageId,
                            correlationId,
                            spanContext,
                            messageContext,
                            headers);

        return Task.CompletedTask;
    }
}