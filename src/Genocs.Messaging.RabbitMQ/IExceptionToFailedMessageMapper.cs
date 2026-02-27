namespace Genocs.Messaging.RabbitMQ;

public interface IExceptionToFailedMessageMapper
{
    FailedMessage? Map(Exception exception, object message);
}