namespace Genocs.Saga.Utils;

internal static class SagaMessageIdentityResolver
{
    public static string Resolve<TMessage>(TMessage message, ISagaContext context)
        where TMessage : class
    {
        if (message is ISagaMessageIdentity identifiedMessage && !string.IsNullOrWhiteSpace(identifiedMessage.MessageId))
        {
            return identifiedMessage.MessageId;
        }

        if (context.TryGetMetadata(SagaContextMetadataKeys.MessageId, out ISagaContextMetadata? metadata)
            && metadata?.Value is string messageId
            && !string.IsNullOrWhiteSpace(messageId))
        {
            return messageId;
        }

        return null;
    }
}