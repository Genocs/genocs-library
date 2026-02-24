using System.Text.Json.Serialization;

namespace Genocs.Common.Cqrs.Events;

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

    [JsonConstructor]
    public RejectedEvent(string reason, string code)
    {
        Reason = reason;
        Code = code;
    }

    public static IRejectedEvent For(string name)
        => new RejectedEvent($"There was an error when executing: {name}", $"{name}_error");
}
