using System;
using Convey.MessageBrokers.RabbitMQ;

namespace Trill.Services.Users.Core.Exceptions
{
    internal sealed class ExceptionToMessageMapper : IExceptionToMessageMapper
    {
        public object Map(Exception exception, object message) => null;
    }
}

