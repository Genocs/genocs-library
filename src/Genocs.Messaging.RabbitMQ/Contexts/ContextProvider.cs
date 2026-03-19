namespace Genocs.Messaging.RabbitMQ.Contexts;

internal sealed class ContextProvider : IContextProvider
{
    private readonly IRabbitMQSerializer _serializer;
    public string HeaderName { get; }

    public ContextProvider(IRabbitMQSerializer serializer, RabbitMQOptions options)
    {
        _serializer = serializer;
        HeaderName = string.IsNullOrWhiteSpace(options.Context?.Header)
            ? "message_context"
            : options.Context.Header;
    }

    public object? Get(IDictionary<string, object>? headers)
    {
        if (headers is null)
        {
            return null;
        }

        if (!headers.TryGetValue(HeaderName, out object? context))
        {
            return null;
        }

        if (context is byte[] bytes)
        {
            return _serializer.Deserialize(bytes);
        }

        return null;
    }
}