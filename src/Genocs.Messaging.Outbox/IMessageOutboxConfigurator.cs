using Genocs.Core.Builders;
using Genocs.Messaging.Outbox.Configurations;

namespace Genocs.Messaging.Outbox;

public interface IMessageOutboxConfigurator
{
    IGenocsBuilder Builder { get; }
    OutboxOptions Options { get; }
}