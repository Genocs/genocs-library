namespace Genocs.Messaging.Outbox.Configurations;

/// <summary>
/// Represents the options for configuring the Outbox pattern in Genocs.Messaging.
/// </summary>
public class OutboxOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "outbox";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The expiry time for outbox messages.
    /// </summary>
    public int Expiry { get; set; }

    /// <summary>
    /// The interval in milliseconds for processing outbox messages.
    /// </summary>
    public double IntervalMilliseconds { get; set; }

    /// <summary>
    /// The name of the inbox collection.
    /// </summary>
    public string InboxCollection { get; set; }

    /// <summary>
    /// The name of the outbox collection.
    /// </summary>
    public string OutboxCollection { get; set; }

    /// <summary>
    /// The type of the outbox.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Indicates whether transactions are disabled.
    /// </summary>
    public bool DisableTransactions { get; set; }
}