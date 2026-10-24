namespace Genocs.Messaging;

/// <summary>
/// Message Attribute.
/// </summary>
/// <remarks>
/// ctor.
/// </remarks>
/// <param name="exchange">The Exchange name.</param>
/// <param name="topic">The Topic name.</param>
/// <param name="routingKey">The Routing Key.</param>
/// <param name="queue">The Queue name.</param>
/// <param name="queueType">The Queue type.</param>
/// <param name="errorQueue">The Error Queue name.</param>
/// <param name="subscriptionId">The SubscriptionId.</param>
/// <param name="external">Indicates if the message is external.</param>
[AttributeUsage(AttributeTargets.Class)]
public class MessageAttribute(
                        string? exchange = null,
                        string? topic = null,
                        string? routingKey = null,
                        string? queue = null,
                        string? queueType = null,
                        string? errorQueue = null,
                        string? subscriptionId = null,
                        bool external = false) : Attribute
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
    /// Gets the topic associated with the current instance.
    /// </summary>
    public string? Topic { get; } = topic;

    /// <summary>
    /// Gets the queue type.
    /// </summary>
    public string? QueueType { get; } = queueType;

    /// <summary>
    /// Gets the topic associated with the current instance.
    /// </summary>
    public string? ErrorQueue { get; } = errorQueue;

    /// <summary>
    /// Gets the queue type.
    /// </summary>
    public string? SubscriptionId { get; } = subscriptionId;

    /// <summary>
    /// The Queue name.
    /// </summary>
    public string? Queue { get; } = queue;

    /// <summary>
    /// External.
    /// </summary>
    public bool External { get; } = external;
}