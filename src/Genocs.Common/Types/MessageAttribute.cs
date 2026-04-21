namespace Genocs.Common.Types;

/// <summary>
/// MessageAttribute class. This attribute is used to define the message properties for the message bus system.
/// </summary>
/// <remarks>
/// Standard constructor.
/// </remarks>
/// <param name="exchange">The exchange used by the system.</param>
/// <param name="topic">The topic used to send/receive message.</param>
/// <param name="queue">The queue.</param>
/// <param name="queueType">The type of the queue.</param>
/// <param name="errorQueue">The error queue.</param>
/// <param name="subscriptionId">The subscription ID.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class MessageAttribute(
                        string? exchange = null,
                        string? topic = null,
                        string? queue = null,
                        string? queueType = null,
                        string? errorQueue = null,
                        string? subscriptionId = null) : Attribute
{
    /// <summary>
    /// The Exchange used by the system.
    /// </summary>
    public string Exchange { get; } = exchange ?? string.Empty;

    /// <summary>
    /// The Topic used to send/receive message.
    /// </summary>
    public string Topic { get; } = topic ?? string.Empty;

    /// <summary>
    /// The queue.
    /// </summary>
    public string Queue { get; } = queue ?? string.Empty;

    /// <summary>
    /// The type of the queue.
    /// </summary>
    public string QueueType { get; } = queueType ?? string.Empty;

    /// <summary>
    /// The error.
    /// </summary>
    public string ErrorQueue { get; } = errorQueue ?? string.Empty;

    /// <summary>
    /// The subscriptionId.
    /// </summary>
    public string SubscriptionId { get; } = subscriptionId ?? string.Empty;
}