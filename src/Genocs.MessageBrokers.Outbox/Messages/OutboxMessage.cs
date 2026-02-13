using Genocs.Common.Domain.Entities;

namespace Genocs.MessageBrokers.Outbox.Messages;

public sealed class OutboxMessage : IEntity<string>
{
    public string Id { get; set; }
    public string? OriginatedMessageId { get; set; }
    public string? CorrelationId { get; set; }
    public string? SpanContext { get; set; }
    public Dictionary<string, object> Headers { get; set; } = new();
    public string? MessageType { get; set; }
    public string? MessageContextType { get; set; }
    public object? Message { get; set; }
    public object? MessageContext { get; set; }
    public string? SerializedMessage { get; set; }
    public string? SerializedMessageContext { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public bool IsTransient()
    {
        throw new NotImplementedException();
    }
}