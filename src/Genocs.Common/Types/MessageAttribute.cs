namespace Genocs.Common.Types;

/// <summary>
/// MessageAttribute class.
/// </summary>
/// <remarks>
/// Standard constructor.
/// </remarks>
/// <param name="exchange"></param>
/// <param name="topic"></param>
/// <param name="queue"></param>
/// <param name="queueType"></param>
/// <param name="errorQueue"></param>
/// <param name="subscriptionId"></param>
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