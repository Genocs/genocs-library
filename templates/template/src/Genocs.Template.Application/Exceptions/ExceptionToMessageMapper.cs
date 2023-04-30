using Genocs.MessageBrokers.RabbitMQ;

namespace Genocs.Template.Application.Exceptions;

internal sealed class ExceptionToMessageMapper : IExceptionToMessageMapper
{
    public object Map(Exception exception, object message) => null;
}

