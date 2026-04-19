using Genocs.Common.Domain.Entities;

namespace Genocs.Messaging.Outbox.Messages;

public sealed class OutboxMessage : IEntity<string>
{
    public string? Id { get; set; }
    public string? OriginatedMessageId { get; set; }
    public string? CorrelationId { get; set; }
    public string? SpanContext { get; set; }
    public Dictionary<string, object?> Headers { get; set; } = [];
    public string? MessageType { get; set; }
    public string? MessageContextType { get; set; }
    public object? Message { get; set; }
    public object? MessageContext { get; set; }
    public string? SerializedMessage { get; init; }
    public string? SerializedMessageContext { get; init; }
    public DateTime SentAt { get; init; }
    public DateTime? ProcessedAt { get; private set; }
    public void SetProcessed() => ProcessedAt = DateTime.UtcNow;

    public bool IsTransient() => string.IsNullOrWhiteSpace(Id);
}