using Genocs.Common.Domain.Entities;
using Genocs.Common.Domain.Specifications;

namespace Genocs.Common.Domain.Repositories;

/// <summary>
/// Optional repository contract for providers that support specification-based querying.
/// Prefer this contract in application and domain services when query composition is required.
/// </summary>
/// <typeparam name="TEntity">Main entity type this repository works on.</typeparam>
/// <typeparam name="TKey">Primary key type of the entity.</typeparam>
public interface ISpecificationRepository<TEntity, TKey> : IRepositoryOfEntity<TEntity, TKey>
    where TEntity : IEntity<TKey>
{
    /// <summary>
    /// Lists entities using a provider-agnostic specification.
    /// </summary>
    Task<List<TEntity>> ListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists projected results using a provider-agnostic specification.
    /// </summary>
    Task<List<TResult>> ListAsync<TResult>(IProjectionSpecification<TEntity, TResult> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first matching entity or null using a provider-agnostic specification.
    /// </summary>
    Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets exactly one matching entity using a provider-agnostic specification.
    /// Throws when no entity or more than one entity matches.
    /// </summary>
    Task<TEntity> SingleAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching a provider-agnostic specification.
    /// </summary>
    Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching a provider-agnostic specification with long precision.
    /// </summary>
    Task<long> LongCountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
}
