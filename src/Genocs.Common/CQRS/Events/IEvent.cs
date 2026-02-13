using Genocs.Common.CQRS.Commons;

namespace Genocs.Common.CQRS.Events;

/// <summary>
/// The CQRS event interface that defines a generic event used for integration tasks.
/// </summary>
public interface IEvent : IMessage;
