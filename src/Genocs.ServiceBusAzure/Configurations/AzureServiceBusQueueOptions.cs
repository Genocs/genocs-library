using Microsoft.Azure.ServiceBus;

namespace Genocs.ServiceBusAzure.Configurations;

public class AzureServiceBusQueueOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "azureServiceBusQueue";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    public string? ConnectionString { get; set; }
    public string? QueueName { get; set; }
    public int MaxConcurrentCalls { get; set; } = 20;
    public int PrefetchCount { get; set; } = 100;
    public ReceiveMode ReceiveMode { get; set; } = ReceiveMode.PeekLock;
    public RetryPolicy RetryPolicy { get; set; } = RetryPolicy.Default;
    public bool AutoComplete { get; set; } = true;
}
