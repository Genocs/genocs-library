namespace Genocs.MessageBrokers.Outbox;

/// <summary>
/// The Message Outbox interface definition.
/// </summary>
public interface IMessageOutbox
{
    bool Enabled { get; }

    Task HandleAsync(string messageId, Func<Task> handler);

    Task SendAsync<T>(
                        T message,
                        string? originatedMessageId = null,
                        string? messageId = null,
                        string? correlationId = null,
                        string? spanContext = null,
                        object? messageContext = null,
                        IDictionary<string, object>? headers = null)
        where T : class;
}