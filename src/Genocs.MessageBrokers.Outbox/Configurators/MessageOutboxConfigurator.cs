using Genocs.Core.Builders;
using Genocs.MessageBrokers.Outbox.Configurations;

namespace Genocs.MessageBrokers.Outbox.Configurators;

internal sealed class MessageOutboxConfigurator : IMessageOutboxConfigurator
{
    public IGenocsBuilder Builder { get; }
    public OutboxSettings Options { get; }

    public MessageOutboxConfigurator(IGenocsBuilder builder, OutboxSettings options)
    {
        Builder = builder;
        Options = options;
    }
}