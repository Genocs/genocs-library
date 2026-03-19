namespace Genocs.Messaging.RabbitMQ;

public interface IContextProvider
{
    string HeaderName { get; }
    object Get(IDictionary<string, object> headers);
}