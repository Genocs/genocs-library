using Genocs.Core.CQRS.Events;

namespace Genocs.Template.Application.Services;

public interface IMessageBroker
{
    Task PublishAsync(params IEvent[] events);
}