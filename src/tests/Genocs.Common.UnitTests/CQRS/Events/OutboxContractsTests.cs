using Genocs.Common.CQRS.Events;
using Xunit;

namespace Genocs.Common.UnitTests.CQRS.Events;

public class OutboxContractsTests
{
    [Fact]
    public void TransactionalEvent_ShouldExtendIntegrationEvent()
    {
        Assert.Contains(typeof(IIntegrationEvent), typeof(ITransactionalEvent).GetInterfaces());
    }

    [Fact]
    public void NonGenericOutboxMessage_ShouldExtendGenericOutboxMessage()
    {
        Assert.Contains(typeof(IOutboxMessage<IIntegrationEvent>), typeof(IOutboxMessage).GetInterfaces());
    }

    [Fact]
    public void GenericOutboxMessage_ShouldExposeExpectedEnvelopeProperties()
    {
        var properties = typeof(IOutboxMessage<IIntegrationEvent>).GetProperties();

        Assert.Contains(properties, property => property.Name == "MessageId" && property.PropertyType == typeof(string));
        Assert.Contains(properties, property => property.Name == "CorrelationId" && property.PropertyType == typeof(string));
        Assert.Contains(properties, property => property.Name == "OccurredAt" && property.PropertyType == typeof(DateTime));
        Assert.Contains(properties, property => property.Name == "IntegrationEvent" && property.PropertyType == typeof(IIntegrationEvent));
    }

    [Fact]
    public void OutboxDispatcher_EnqueueAsync_ShouldRequireIntegrationEventConstraint()
    {
        var method = typeof(IOutboxDispatcher)
            .GetMethods()
            .Single(m => m.Name == "EnqueueAsync");

        var genericArgument = method.GetGenericArguments().Single();
        var genericConstraints = genericArgument.GetGenericParameterConstraints();

        Assert.Contains(typeof(IIntegrationEvent), genericConstraints);
    }
}
