using Genocs.MessageBrokers.Outbox.Messages;

namespace Genocs.MessageBrokers.Outbox;

public interface IMessageOutboxAccessor
{
    Task<IReadOnlyList<OutboxMessage>> GetUnsentAsync();
    Task ProcessAsync(OutboxMessage message);
    Task ProcessAsync(IEnumerable<OutboxMessage> outboxMessages);
}