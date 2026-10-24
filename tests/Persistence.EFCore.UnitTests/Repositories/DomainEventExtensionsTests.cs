using Genocs.Common.CQRS.Events;
using Genocs.Common.Domain.Entities;
using Genocs.Persistence.EFCore.Repositories;
using Shouldly;
using Xunit;

namespace Genocs.Persistence.EFCore.UnitTests.Repositories;

public class DomainEventExtensionsTests
{
    [Fact]
    public void AddDomainEvent_DelegatesToAggregateContract()
    {
        var aggregate = new ContractOnlyAggregate();
        var @event = new TestEvent();

        aggregate.AddDomainEvent(@event);

        aggregate.DomainEvents.ShouldContain(@event);
        aggregate.Invocations.ShouldBe(1);
    }

    [Fact]
    public void AddDomainEvent_ThrowsForNullAggregate()
    {
        ContractOnlyAggregate? aggregate = null;

        Should.Throw<ArgumentNullException>(() => DomainEventExtensions.AddDomainEvent(aggregate!, new TestEvent()));
    }

    private sealed class ContractOnlyAggregate : IGeneratesDomainEvents
    {
        private readonly List<IEvent> _domainEvents = [];

        public int Invocations { get; private set; }

        public IReadOnlyCollection<IEvent> DomainEvents => _domainEvents;

        public void AddDomainEvent(IEvent @event)
        {
            Invocations++;
            _domainEvents.Add(@event);
        }

        public void ClearDomainEvents() => _domainEvents.Clear();
    }

    private sealed record TestEvent : IEvent;
}
