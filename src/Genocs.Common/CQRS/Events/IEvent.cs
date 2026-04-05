using Genocs.Common.CQRS.Commons;

namespace Genocs.Common.CQRS.Events;

/// <summary>
/// The CQRS event interface that acts as a bridge/base for both domain and integration events.
/// </summary>
public interface IEvent : IMessage
{
}

/// <summary>
/// Marker interface for in-process domain events.
/// </summary>
public interface IDomainEvent : IEvent
{
}

/// <summary>
/// Marker interface for broker-facing integration events.
/// </summary>
public interface IIntegrationEvent : IEvent
{
}
