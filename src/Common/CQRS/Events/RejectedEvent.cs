using System.Text.Json.Serialization;
using Genocs.Common.Types;

namespace Genocs.Common.CQRS.Events;

/// <summary>
/// Represents an event that indicates a rejection or failure in processing a command or event.
/// </summary>
public class RejectedEvent : IRejectedEvent
{
    /// <summary>
    /// Gets the reason for the rejection or failure.
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// Gets the code associated with the rejection or failure, which can be used for categorization or identification purposes.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the shared error model for this rejection event.
    /// </summary>
    public Error Error => new(Code, Reason);

    [JsonConstructor]
    public RejectedEvent(string reason, string code)
    {
        Reason = reason;
        Code = code;
    }

    /// <summary>
    /// Creates a rejected event from a shared error instance.
    /// </summary>
    public static IRejectedEvent For(Error error)
        => new RejectedEvent(error.Message, error.Code);

    public static IRejectedEvent For(string name)
        => new RejectedEvent($"There was an error when executing: {name}", RejectionCode.Create(name));
}
