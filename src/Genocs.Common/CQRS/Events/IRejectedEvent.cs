namespace Genocs.Common.Cqrs.Events;

/// <summary>
/// Interface for rejected events in Cqrs pattern.
/// </summary>
public interface IRejectedEvent : IEvent
{
    /// <summary>
    /// The reason for the rejection.
    /// </summary>
    string Reason { get; }

    /// <summary>
    /// The code representing the rejection.
    /// </summary>
    string Code { get; }
}
