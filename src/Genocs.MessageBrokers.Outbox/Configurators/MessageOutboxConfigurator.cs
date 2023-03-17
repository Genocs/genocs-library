using Genocs.Core.Builders;

namespace Genocs.MessageBrokers.Outbox.Configurators;

internal sealed class MessageOutboxConfigurator : IMessageOutboxConfigurator
{
    public IGenocsBuilder Builder { get; }
    public OutboxOptions Options { get; }

    public MessageOutboxConfigurator(IGenocsBuilder builder, OutboxOptions options)
    {
        Builder = builder;
        Options = options;
    }
}