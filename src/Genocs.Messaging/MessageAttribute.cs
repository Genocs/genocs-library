namespace Genocs.Messaging;

/// <summary>
/// Message Attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class MessageAttribute : Attribute
{
    /// <summary>
    /// The Exchange name.
    /// </summary>
    public string? Exchange { get; }

    /// <summary>
    /// The Routing Key.
    /// </summary>
    public string? RoutingKey { get; }

    /// <summary>
    /// The Queue name.
    /// </summary>
    public string? Queue { get; }

    /// <summary>
    /// External.
    /// </summary>
    public bool External { get; }

    /// <summary>
    /// ctor.
    /// </summary>
    /// <param name="exchange">The Exchange name.</param>
    /// <param name="routingKey">The Routing Key.</param>
    /// <param name="queue">The Queue name.</param>
    /// <param name="external">Indicates if the message is external.</param>
    public MessageAttribute(string? exchange = null, string? routingKey = null, string? queue = null, bool external = false)
    {
        Exchange = exchange;
        RoutingKey = routingKey;
        Queue = queue;
        External = external;
    }
}