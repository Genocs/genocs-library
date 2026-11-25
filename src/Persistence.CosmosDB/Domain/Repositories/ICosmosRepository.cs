using System.Linq.Expressions;
using Genocs.Persistence.CosmosDB.Domain.Entities;

namespace Genocs.Persistence.CosmosDB.Domain.Repositories;

/// <summary>
/// CRUD access to one Cosmos container.
/// Point reads and deletes require the document partition key.
/// Queries use the container LINQ provider and may span partitions.
/// </summary>
/// <typeparam name="TEntity">Document type.</typeparam>
/// <typeparam name="TKey">Document id type. <see cref="string"/> or <see cref="Guid"/>.</typeparam>
public interface ICosmosRepository<TEntity, TKey>
    where TEntity : class, ICosmosEntity<TKey>
{
    /// <summary>
    /// Point-reads a document. Returns <c>null</c> when the item does not exist.
    /// </summary>
    /// <param name="id">Document id.</param>
    /// <param name="partitionKey">Partition key value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The document, or <c>null</c>.</returns>
    Task<TEntity?> GetAsync(TKey id, string partitionKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the single document that matches <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate">Filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The document, or <c>null</c> when none match.</returns>
    /// <exception cref="InvalidOperationException">More than one document matches.</exception>
    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns every document that matches <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate">Filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Matching documents. Empty when none match.</returns>
    Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a document. Fails when an item with the same id already exists in the partition.
    /// </summary>
    /// <param name="entity">Document to insert.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when the item is created.</returns>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces an existing document in its partition.
    /// </summary>
    /// <param name="entity">Document to replace. <see cref="ICosmosEntity{TKey}.PartitionKey"/> selects the partition.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The same instance after the replace is accepted.</returns>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a document by id and partition key.
    /// </summary>
    /// <param name="id">Document id.</param>
    /// <param name="partitionKey">Partition key value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when the item is deleted.</returns>
    Task DeleteAsync(TKey id, string partitionKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns whether a document exists for the id and partition key.
    /// </summary>
    /// <param name="id">Document id.</param>
    /// <param name="partitionKey">Partition key value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> when the item exists.</returns>
    Task<bool> ExistsAsync(TKey id, string partitionKey, CancellationToken cancellationToken = default);
}
