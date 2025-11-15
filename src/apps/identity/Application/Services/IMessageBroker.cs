using Genocs.Core.CQRS.Events;

namespace Genocs.Identities.Application.Services;

public interface IMessageBroker
{
    Task PublishAsync(params IEvent[] events);
}