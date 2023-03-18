using Convey.CQRS.Events;

namespace Genocs.Core.Demo.Users.Application.Services;

public interface IMessageBroker
{
    Task PublishAsync(params IEvent[] events);
}