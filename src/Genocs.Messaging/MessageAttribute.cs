namespace Genocs.Messaging;

/// <summary>
/// Message Attribute.
/// </summary>
/// <remarks>
/// ctor.
/// </remarks>
/// <param name="exchange">The Exchange name.</param>
/// <param name="routingKey">The Routing Key.</param>
/// <param name="queue">The Queue name.</param>
/// <param name="external">Indicates if the message is external.</param>
[AttributeUsage(AttributeTargets.Class)]
public class MessageAttribute(string? exchange = null, string? routingKey = null, string? queue = null, bool external = false) : Attribute
{
    /// <summary>
    /// The Exchange name.
    /// </summary>
    public string? Exchange { get; } = exchange;

    /// <summary>
    /// The Routing Key.
    /// </summary>
    public string? RoutingKey { get; } = routingKey;

    /// <summary>
    /// The Queue name.
    /// </summary>
    public string? Queue { get; } = queue;

    /// <summary>
    /// External.
    /// </summary>
    public bool External { get; } = external;
}