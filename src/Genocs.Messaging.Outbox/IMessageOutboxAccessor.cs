using Genocs.Messaging.Outbox.Messages;

namespace Genocs.Messaging.Outbox;

public interface IMessageOutboxAccessor
{
    Task<IReadOnlyList<OutboxMessage>> GetUnsentAsync();
    Task ProcessAsync(OutboxMessage message);
    Task ProcessAsync(IEnumerable<OutboxMessage> outboxMessages);
}