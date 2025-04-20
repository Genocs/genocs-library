using Genocs.Core.Domain.Entities;

namespace Genocs.Core.Domain.Repositories;

/// <summary>
/// This interface is used to identify a repository so can be used to be registered by convention.
/// Implement generic version instead of this one.
/// </summary>
public interface IRepository<TEntity, in TKey>
       where TEntity : IEntity<TKey>;



// The Repository for the Application Db
// I(Read)RepositoryBase<T> is from Ardalis.Specification

/// <summary>
/// The regular read/write repository for an aggregate root.
/// </summary>
public interface IRepository<T> : Ardalis.Specification.IRepositoryBase<T>
    where T : class, IAggregateRoot;

/// <summary>
/// The read-only repository for an aggregate root.
/// </summary>
public interface IReadRepository<T> : Ardalis.Specification.IReadRepositoryBase<T>
    where T : class, IAggregateRoot;

/// <summary>
/// A special (read/write) repository for an aggregate root,
/// that also adds EntityCreated, EntityUpdated or EntityDeleted
/// events to the DomainEvents of the entities before adding,
/// updating or deleting them.
/// </summary>
public interface IRepositoryWithEvents<T> : Ardalis.Specification.IRepositoryBase<T>
    where T : class, IAggregateRoot;
