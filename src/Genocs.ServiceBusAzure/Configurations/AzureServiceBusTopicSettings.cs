using Microsoft.Azure.ServiceBus;

namespace Genocs.ServiceBusAzure.Configurations;

public class AzureServiceBusTopicSettings
{
    public const string Position = "AzureServiceBusTopic";
    public string? ConnectionString { get; set; }
    public string? TopicName { get; set; }
    public string? SubscriptionName { get; set; }
    public int MaxConcurrentCalls { get; set; } = 20;
    public int PrefetchCount { get; set; } = 100;
    public ReceiveMode ReceiveMode { get; set; } = ReceiveMode.PeekLock;
    public RetryPolicy RetryPolicy { get; set; } = RetryPolicy.Default;
    public bool AutoComplete { get; set; } = true;
}

