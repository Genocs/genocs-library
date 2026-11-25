using System.Linq.Expressions;
using System.Net;
using Genocs.Persistence.CosmosDB.Domain.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace Genocs.Persistence.CosmosDB.Domain.Repositories;

internal sealed class CosmosRepository<TEntity, TKey>(Container container) : ICosmosRepository<TEntity, TKey>
    where TEntity : class, ICosmosEntity<TKey>
{
    static CosmosRepository()
    {
        CosmosItemIdentity.EnsureSupportedKey<TKey>();
    }

    public async Task<TEntity?> GetAsync(TKey id, string partitionKey, CancellationToken cancellationToken = default)
    {
        string itemId = CosmosItemIdentity.ToItemId(id);
        RequirePartitionKey(partitionKey);

        try
        {
            ItemResponse<TEntity> response = await container.ReadItemAsync<TEntity>(
                itemId,
                new PartitionKey(partitionKey),
                cancellationToken: cancellationToken);

            return response.Resource;
        }
        catch (CosmosException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        IReadOnlyList<TEntity> matches = await FindAsync(predicate, cancellationToken);
        return matches.Count switch
        {
            0 => null,
            1 => matches[0],
            _ => throw new InvalidOperationException($"Expected a single {typeof(TEntity).Name} document, but the query returned {matches.Count}."),
        };
    }

    public async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        using FeedIterator<TEntity> iterator = container
            .GetItemLinqQueryable<TEntity>()
            .Where(predicate)
            .ToFeedIterator();

        var matches = new List<TEntity>();
        while (iterator.HasMoreResults)
        {
            FeedResponse<TEntity> page = await iterator.ReadNextAsync(cancellationToken);
            matches.AddRange(page);
        }

        return matches;
    }

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        CosmosItemIdentity.ToItemId(entity.Id);
        RequirePartitionKey(entity.PartitionKey);

        return container.CreateItemAsync(entity, new PartitionKey(entity.PartitionKey), cancellationToken: cancellationToken);
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        string itemId = CosmosItemIdentity.ToItemId(entity.Id);
        RequirePartitionKey(entity.PartitionKey);

        await container.ReplaceItemAsync(
            entity,
            itemId,
            new PartitionKey(entity.PartitionKey),
            cancellationToken: cancellationToken);

        return entity;
    }

    public Task DeleteAsync(TKey id, string partitionKey, CancellationToken cancellationToken = default)
    {
        string itemId = CosmosItemIdentity.ToItemId(id);
        RequirePartitionKey(partitionKey);

        return container.DeleteItemAsync<TEntity>(itemId, new PartitionKey(partitionKey), cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsAsync(TKey id, string partitionKey, CancellationToken cancellationToken = default)
    {
        TEntity? entity = await GetAsync(id, partitionKey, cancellationToken);
        return entity is not null;
    }

    private static void RequirePartitionKey(string partitionKey)
    {
        if (string.IsNullOrWhiteSpace(partitionKey))
        {
            throw new ArgumentException("Cosmos partition key is required.", nameof(partitionKey));
        }
    }
}
