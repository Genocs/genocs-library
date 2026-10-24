namespace Genocs.Messaging.RabbitMQ;

public interface IConventionsProvider
{
    IConventions Get<T>();
    IConventions Get(Type type);
}