using Genocs.Common.CQRS.Events;
using Genocs.Common.Domain.Entities;
using Genocs.Core.Domain.Entities;
using Xunit;

namespace Genocs.Core.UnitTests.Domain.Entities;

public class AggregateRootTests
{
    [Fact]
    public void DomainEvents_IsInitializedAndNeverNull()
    {
        var aggregate = new TestAggregate();

        Assert.NotNull(aggregate.DomainEvents);
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void ExplicitDomainEventsProjection_ExposesSameCollection()
    {
        var aggregate = new TestAggregate();
        IGeneratesDomainEvents source = aggregate;

        aggregate.DomainEvents.Add(new TestEvent());

        Assert.Single(source.DomainEvents);
        Assert.Same(aggregate.DomainEvents, source.DomainEvents);
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        var aggregate = new TestAggregate();
        aggregate.DomainEvents.Add(new TestEvent());
        aggregate.DomainEvents.Add(new TestEvent());

        aggregate.ClearDomainEvents();

        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void AddDomainEvent_AppendsEventThroughContract()
    {
        var aggregate = new TestAggregate();
        IGeneratesDomainEvents source = aggregate;

        source.AddDomainEvent(new TestEvent());

        Assert.Single(source.DomainEvents);
        Assert.Single(aggregate.DomainEvents);
    }

    private sealed class TestAggregate : AggregateRoot<Guid>;

    private sealed record TestEvent : IEvent;
}
