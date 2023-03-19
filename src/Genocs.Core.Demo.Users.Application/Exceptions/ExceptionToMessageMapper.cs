using Genocs.MessageBrokers.RabbitMQ;

namespace Genocs.Core.Demo.Users.Application.Exceptions;

internal sealed class ExceptionToMessageMapper : IExceptionToMessageMapper
{
    public object Map(Exception exception, object message) => null;
}

