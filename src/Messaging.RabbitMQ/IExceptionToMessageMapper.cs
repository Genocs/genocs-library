namespace Genocs.Messaging.RabbitMQ;

public interface IExceptionToMessageMapper
{
    object? Map(Exception exception, object message);
}