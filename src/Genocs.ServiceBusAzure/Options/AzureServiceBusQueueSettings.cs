using Microsoft.Azure.ServiceBus;

namespace Genocs.ServiceBusAzure.Options
{
    public class AzureServiceBusQueueSettings
    {
        public const string Position = "AzureServiceBusQueue";
        public string ConnectionString { get; set; }
        public string QueueName { get; set; }
        public int MaxConcurrentCalls { get; set; } = 20;
        public int PrefetchCount { get; set; } = 100;
        public ReceiveMode ReceiveMode { get; set; } = ReceiveMode.PeekLock;
        public RetryPolicy RetryPolicy { get; set; } = RetryPolicy.Default;
        public bool AutoComplete { get; set; } = true;
    }
}
