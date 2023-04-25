using Genocs.Core.Builders;
using Genocs.MessageBrokers.Outbox.Options;

namespace Genocs.MessageBrokers.Outbox;

public interface IMessageOutboxConfigurator
{
    IGenocsBuilder Builder { get; }
    OutboxSettings Options { get; }
}