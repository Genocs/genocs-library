using Genocs.Common.CQRS.Events;
using Genocs.Common.Domain.Entities;

namespace Genocs.Persistence.EFCore.UnitTests.Repositories;

/// <summary>
/// Minimal aggregate root that implements only the non-generic IAggregateRoot marker.
/// Represents the most common consumer pattern: an entity with a typed key declared
/// separately (e.g. via a base class), not via the self-referential IAggregateRoot{T} form.
/// </summary>
public sealed class ProductAggregate : IAggregateRoot
{
    public bool IsTransient() => true;
}

/// <summary>
/// Aggregate root with a Guid primary key — the standard domain model pattern.
/// Satisfies IAggregateRoot{Guid}, which is NOT the self-referential IAggregateRoot{T}
/// constraint required by EventAddingRepositoryDecorator{T}. IRepositoryWithEvents{T}
/// is therefore intentionally not registered for this type until EFCORE-017.
/// </summary>
public sealed class OrderAggregate : IAggregateRoot<Guid>
{
    public Guid Id { get; init; } = Guid.NewGuid();

    private readonly List<IEvent> _domainEvents = [];

    public IReadOnlyCollection<IEvent> DomainEvents => _domainEvents;

    public void AddDomainEvent(IEvent @event)
    {
        _domainEvents.Add(@event);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    public bool IsTransient() => Id == Guid.Empty;
}

/// <summary>
/// Aggregate root that satisfies the self-referential IAggregateRoot{T} constraint
/// used by EventAddingRepositoryDecorator{T}. Verifies that IRepositoryWithEvents{T}
/// IS registered when the constraint is met.
/// </summary>
public sealed class InvoiceAggregate : IAggregateRoot<InvoiceAggregate>
{
    public InvoiceAggregate Id => this;

    private readonly List<IEvent> _domainEvents = [];

    public IReadOnlyCollection<IEvent> DomainEvents => _domainEvents;

    public void AddDomainEvent(IEvent @event)
    {
        _domainEvents.Add(@event);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    public bool IsTransient() => false;
}

/// <summary>
/// A plain class that does NOT implement IAggregateRoot.
/// Repository registration must ignore it entirely.
/// </summary>
public sealed class NotAnAggregate
{
    public int Value { get; init; }
}
