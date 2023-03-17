using RabbitMQ.Client;

namespace Genocs.MessageBrokers.RabbitMQ;

public sealed class ConsumerConnection
{
    public IConnection Connection { get; }

    public ConsumerConnection(IConnection connection)
    {
        Connection = connection;
    }
}