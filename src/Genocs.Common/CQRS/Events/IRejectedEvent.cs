using Genocs.Common.Types;

namespace Genocs.Common.CQRS.Events;

/// <summary>
/// Interface for rejected events in CQRS pattern.
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

    /// <summary>
    /// Structured error representation aligned with the shared Result/Error primitives.
    /// </summary>
    Error Error => new(Code, Reason);
}
