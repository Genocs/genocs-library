namespace Genocs.MessageBrokers.RabbitMQ;

public interface IRabbitMqSerializer
{
    ReadOnlySpan<byte> Serialize(object value);
    object? Deserialize(ReadOnlySpan<byte> value, Type type);
    object? Deserialize(ReadOnlySpan<byte> value);
}