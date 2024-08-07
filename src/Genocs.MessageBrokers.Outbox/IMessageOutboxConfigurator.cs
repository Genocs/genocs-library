using Genocs.Core.Builders;
using Genocs.MessageBrokers.Outbox.Configurations;

namespace Genocs.MessageBrokers.Outbox;

public interface IMessageOutboxConfigurator
{
    IGenocsBuilder Builder { get; }
    OutboxOptions Options { get; }
}