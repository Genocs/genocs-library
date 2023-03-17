using Genocs.Core.Builders;

namespace Genocs.MessageBrokers.Outbox;

public interface IMessageOutboxConfigurator
{
    IGenocsBuilder Builder { get; }
    OutboxOptions Options { get; }
}