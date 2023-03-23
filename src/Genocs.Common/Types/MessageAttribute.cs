namespace Genocs.Common.Types;

/// <summary>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class MessageAttribute : Attribute
{
    /// <summary>
    /// The Exchange used by the system
    /// </summary>
    public string Exchange { get; }

    /// <summary>
    /// The Topic used to send/receive message 
    /// </summary>
    public string Topic { get; }
    public string Queue { get; }
    public string QueueType { get; }
    public string ErrorQueue { get; }
    public string SubscriptionId { get; }

    /// <summary>
    /// Standard constructor
    /// </summary>
    /// <param name="exchange"></param>
    /// <param name="topic"></param>
    /// <param name="queue"></param>
    /// <param name="queueType"></param>
    /// <param name="errorQueue"></param>
    /// <param name="subscriptionId"></param>
    public MessageAttribute(string exchange = null, string topic = null, string queue = null,
        string queueType = null, string errorQueue = null, string subscriptionId = null)
    {
        Exchange = exchange ?? string.Empty;
        Topic = topic ?? string.Empty;
        Queue = queue ?? string.Empty;
        QueueType = queueType ?? string.Empty;
        ErrorQueue = errorQueue ?? string.Empty;
        SubscriptionId = subscriptionId ?? string.Empty;
    }
}