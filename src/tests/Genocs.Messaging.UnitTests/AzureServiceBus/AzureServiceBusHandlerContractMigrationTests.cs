using System.Reflection;
using Genocs.Messaging.AzureServiceBus.Queues.Interfaces;
using Genocs.Messaging.AzureServiceBus.Topics.Interfaces;
using Xunit;

namespace Genocs.Messaging.UnitTests.AzureServiceBus;

public class AzureServiceBusHandlerContractMigrationTests
{
    [Fact]
    public void QueueInterface_ExposesModernRegistrationAndMarksLegacyAsObsolete()
    {
        MethodInfo modernMethod = typeof(IAzureServiceBusQueue).GetMethod("ConsumeModern")
            ?? throw new InvalidOperationException("ConsumeModern method was not found.");
        MethodInfo legacyMethod = typeof(IAzureServiceBusQueue).GetMethod("Consume")
            ?? throw new InvalidOperationException("Consume method was not found.");

        Assert.NotNull(modernMethod);
        Assert.NotNull(legacyMethod.GetCustomAttribute<ObsoleteAttribute>());
    }

    [Fact]
    public void TopicInterface_ExposesModernRegistrationAndMarksLegacyAsObsolete()
    {
        MethodInfo modernMethod = typeof(IAzureServiceBusTopic).GetMethod("SubscribeModern")
            ?? throw new InvalidOperationException("SubscribeModern method was not found.");
        MethodInfo legacyMethod = typeof(IAzureServiceBusTopic).GetMethod("Subscribe")
            ?? throw new InvalidOperationException("Subscribe method was not found.");

        Assert.NotNull(modernMethod);
        Assert.NotNull(legacyMethod.GetCustomAttribute<ObsoleteAttribute>());
    }
}