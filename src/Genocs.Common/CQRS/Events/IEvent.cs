using Genocs.Common.Cqrs.Commons;

namespace Genocs.Common.Cqrs.Events;

/// <summary>
/// The Cqrs event interface that defines a generic event used for integration tasks.
/// </summary>
public interface IEvent : IMessage;
