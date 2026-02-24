using Genocs.Common.Cqrs.Events;

namespace Genocs.Identities.Application.Services;

public interface IMessageBroker
{
    Task PublishAsync(params IEvent[] events);
}