namespace Genocs.MessageBrokers.RabbitMQ;

public interface IRabbitMQSerializer
{
    ReadOnlySpan<byte> Serialize(object value);
    object? Deserialize(ReadOnlySpan<byte> value, Type type);
    object? Deserialize(ReadOnlySpan<byte> value);
}