using Azure.Messaging.ServiceBus;

namespace Genocs.ServiceBusAzure.Configurations;

public class AzureServiceBusTopicOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "azureServiceBusTopic";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }
    public string? ConnectionString { get; set; }
    public string? TopicName { get; set; }
    public string? SubscriptionName { get; set; }
    public int MaxConcurrentCalls { get; set; } = 20;
    public int PrefetchCount { get; set; } = 100;
    public ServiceBusReceiveMode ReceiveMode { get; set; } = ServiceBusReceiveMode.PeekLock;
    public bool AutoComplete { get; set; } = true;
}

