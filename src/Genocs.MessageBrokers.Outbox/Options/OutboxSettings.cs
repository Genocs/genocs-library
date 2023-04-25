namespace Genocs.MessageBrokers.Outbox.Options;

public class OutboxSettings
{
    public bool Enabled { get; set; }
    public int Expiry { get; set; }
    public double IntervalMilliseconds { get; set; }
    public string InboxCollection { get; set; }
    public string OutboxCollection { get; set; }
    public string Type { get; set; }
    public bool DisableTransactions { get; set; }
}