namespace Genocs.ServiceBusAzure.Topics;

public sealed class SubscriptionInfo
{
    public readonly bool IsDynamic;
    public readonly Type HandlerType;

    private SubscriptionInfo(bool isDynamic, Type handlerType)
    {
        IsDynamic = isDynamic;
        HandlerType = handlerType;
    }

    public static SubscriptionInfo Dynamic(Type handlerType)
    {
        return new SubscriptionInfo(true, handlerType);
    }

    public static SubscriptionInfo Typed(Type handlerType)
    {
        return new SubscriptionInfo(false, handlerType);
    }
}
