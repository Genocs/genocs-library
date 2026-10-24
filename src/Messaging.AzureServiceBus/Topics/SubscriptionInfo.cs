namespace Genocs.Messaging.AzureServiceBus.Topics;

public sealed class SubscriptionInfo
{
    public readonly bool IsDynamic;
    public readonly Type EventType;
    public readonly Type HandlerType;
    public readonly bool UsesLegacyContract;

    private SubscriptionInfo(bool isDynamic, Type eventType, Type handlerType, bool usesLegacyContract)
    {
        IsDynamic = isDynamic;
        EventType = eventType;
        HandlerType = handlerType;
        UsesLegacyContract = usesLegacyContract;
    }

    public static SubscriptionInfo Dynamic(Type eventType, Type handlerType, bool usesLegacyContract)
    {
        return new SubscriptionInfo(true, eventType, handlerType, usesLegacyContract);
    }

    public static SubscriptionInfo Typed(Type eventType, Type handlerType, bool usesLegacyContract)
    {
        return new SubscriptionInfo(false, eventType, handlerType, usesLegacyContract);
    }
}
