using System.Linq.Expressions;
using System.Reflection;
using Genocs.Common.Domain.Entities;
using Genocs.Persistence.MongoDB.Exceptions;
using MongoDB.Driver;

namespace Genocs.Persistence.MongoDB.Domain.Repositories;

/// <summary>
/// Compare-and-swap writes for documents that implement <see cref="IVersioned"/>.
/// The version on the document is the expected token. It increases only when a replacement is accepted.
/// </summary>
internal static class MongoOptimisticConcurrency
{
    public static void Replace<TEntity, TKey>(
        IMongoCollection<TEntity> collection,
        TEntity entity,
        FilterDefinition<TEntity> filter,
        bool insertWhenMissing,
        CancellationToken cancellationToken)
        where TEntity : IEntity<TKey>
    {
        if (entity is not IVersioned versioned)
        {
            collection.ReplaceOne(filter, entity, new ReplaceOptions { IsUpsert = insertWhenMissing }, cancellationToken);
            return;
        }

        long expected = versioned.Version;
        SetVersion(entity, expected + 1);
        ReplaceOneResult result;
        try
        {
            result = collection.ReplaceOne(VersionedFilter(filter, expected), entity, new ReplaceOptions { IsUpsert = false }, cancellationToken);
        }
        catch
        {
            SetVersion(entity, expected);
            throw;
        }

        if (TryAccept(result, entity, expected))
        {
            return;
        }

        if (!insertWhenMissing)
        {
            throw OptimisticConcurrencyException.For(entity.GetType(), entity.Id, expected);
        }

        InsertOrThrow<TEntity, TKey>(collection, entity, expected, cancellationToken);
    }

    public static async Task ReplaceAsync<TEntity, TKey>(
        IMongoCollection<TEntity> collection,
        TEntity entity,
        FilterDefinition<TEntity> filter,
        bool insertWhenMissing,
        CancellationToken cancellationToken)
        where TEntity : IEntity<TKey>
    {
        if (entity is not IVersioned versioned)
        {
            await collection.ReplaceOneAsync(filter, entity, new ReplaceOptions { IsUpsert = insertWhenMissing }, cancellationToken);
            return;
        }

        long expected = versioned.Version;
        SetVersion(entity, expected + 1);
        ReplaceOneResult result;
        try
        {
            result = await collection.ReplaceOneAsync(VersionedFilter(filter, expected), entity, new ReplaceOptions { IsUpsert = false }, cancellationToken);
        }
        catch
        {
            SetVersion(entity, expected);
            throw;
        }

        if (TryAccept(result, entity, expected))
        {
            return;
        }

        if (!insertWhenMissing)
        {
            throw OptimisticConcurrencyException.For(entity.GetType(), entity.Id, expected);
        }

        await InsertOrThrowAsync<TEntity, TKey>(collection, entity, expected, cancellationToken);
    }

    public static void Delete<TEntity, TKey>(
        IMongoCollection<TEntity> collection,
        TEntity entity,
        CancellationToken cancellationToken)
        where TEntity : IEntity<TKey>
    {
        if (entity is not IVersioned versioned)
        {
            collection.DeleteOne(Builders<TEntity>.Filter.Eq(candidate => candidate.Id, entity.Id), cancellationToken);
            return;
        }

        DeleteResult result = collection.DeleteOne(VersionedFilter(IdFilter<TEntity, TKey>(entity), versioned.Version), cancellationToken);
        EnsureDeleted(entity.GetType(), entity.Id, versioned.Version, result);
    }

    public static async Task DeleteAsync<TEntity, TKey>(
        IMongoCollection<TEntity> collection,
        TEntity entity,
        CancellationToken cancellationToken)
        where TEntity : IEntity<TKey>
    {
        if (entity is not IVersioned versioned)
        {
            await collection.DeleteOneAsync(Builders<TEntity>.Filter.Eq(candidate => candidate.Id, entity.Id), cancellationToken);
            return;
        }

        DeleteResult result = await collection.DeleteOneAsync(VersionedFilter(IdFilter<TEntity, TKey>(entity), versioned.Version), cancellationToken);
        EnsureDeleted(entity.GetType(), entity.Id, versioned.Version, result);
    }

    private static void InsertOrThrow<TEntity, TKey>(
        IMongoCollection<TEntity> collection,
        TEntity entity,
        long expected,
        CancellationToken cancellationToken)
        where TEntity : IEntity<TKey>
    {
        try
        {
            collection.InsertOne(entity, cancellationToken: cancellationToken);
        }
        catch (MongoWriteException exception) when (exception.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            throw OptimisticConcurrencyException.For(entity.GetType(), entity.Id, expected, exception);
        }
    }

    private static async Task InsertOrThrowAsync<TEntity, TKey>(
        IMongoCollection<TEntity> collection,
        TEntity entity,
        long expected,
        CancellationToken cancellationToken)
        where TEntity : IEntity<TKey>
    {
        try
        {
            await collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        }
        catch (MongoWriteException exception) when (exception.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            throw OptimisticConcurrencyException.For(entity.GetType(), entity.Id, expected, exception);
        }
    }

    private static FilterDefinition<TEntity> IdFilter<TEntity, TKey>(TEntity entity)
        where TEntity : IEntity<TKey>
        => Builders<TEntity>.Filter.Eq(candidate => candidate.Id, entity.Id);

    private static FilterDefinition<TEntity> VersionedFilter<TEntity>(FilterDefinition<TEntity> filter, long expected)
        => Builders<TEntity>.Filter.And(filter, VersionEquals<TEntity>(expected));

    private static FilterDefinition<TEntity> VersionEquals<TEntity>(long expected)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        Expression property;
        try
        {
            property = Expression.Property(parameter, nameof(IVersioned.Version));
        }
        catch (ArgumentException exception)
        {
            throw new InvalidOperationException(
                $"'{typeof(TEntity).Name}' implements '{nameof(IVersioned)}', but '{nameof(IVersioned.Version)}' is not a public property. Optimistic concurrency needs a mapped long version.",
                exception);
        }

        var version = Expression.Lambda<Func<TEntity, long>>(property, parameter);
        return Builders<TEntity>.Filter.Eq(version, expected);
    }

    private static bool TryAccept<TEntity>(ReplaceOneResult result, TEntity entity, long expected)
    {
        try
        {
            if (!result.IsAcknowledged)
            {
                throw new InvalidOperationException("Optimistic concurrency requires an acknowledged MongoDB write concern.");
            }

            if (result.MatchedCount > 0)
            {
                return true;
            }
        }
        catch
        {
            SetVersion(entity!, expected);
            throw;
        }

        SetVersion(entity!, expected);
        return false;
    }

    private static void EnsureDeleted(Type entityType, object? key, long expected, DeleteResult result)
    {
        if (!result.IsAcknowledged)
        {
            throw new InvalidOperationException("Optimistic concurrency requires an acknowledged MongoDB write concern.");
        }

        if (result.DeletedCount == 0)
        {
            throw OptimisticConcurrencyException.For(entityType, key, expected);
        }
    }

    private static void SetVersion(object entity, long version)
    {
        var property = entity.GetType().GetProperty(
            nameof(IVersioned.Version),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var setter = property?.SetMethod;
        if (setter is null)
        {
            throw new InvalidOperationException(
                $"'{entity.GetType().Name}' implements '{nameof(IVersioned)}', but '{nameof(IVersioned.Version)}' has no setter. Optimistic concurrency needs to store the next version on the document.");
        }

        setter.Invoke(entity, [version]);
    }
}
