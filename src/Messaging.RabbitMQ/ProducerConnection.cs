using RabbitMQ.Client;

namespace Genocs.Messaging.RabbitMQ;

public sealed class ProducerConnection
{
    public IConnection Connection { get; }

    public ProducerConnection(IConnection connection)
    {
        Connection = connection;
    }
}