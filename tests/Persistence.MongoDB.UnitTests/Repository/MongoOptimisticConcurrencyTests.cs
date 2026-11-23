using System.Reflection;
using Genocs.Common.Domain.Entities;
using Genocs.Persistence.MongoDB.Domain.Repositories;
using Genocs.Persistence.MongoDB.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Servers;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Genocs.Persistence.MongoDB.UnitTests.Repository;

public class MongoOptimisticConcurrencyTests
{
    [Fact]
    public async Task UpdateAsync_IncrementsVersion_WhenStoredVersionMatches()
    {
        var entity = VersionedOrder.Create(Guid.NewGuid(), "draft", 4);
        var repository = CreateRepository(out var collection);
        var accepted = AcceptedReplace();
        FilterDefinition<VersionedOrder>? filter = null;
        collection.ReplaceOneAsync(
                Arg.Do<FilterDefinition<VersionedOrder>>(candidate => filter = candidate),
                Arg.Any<VersionedOrder>(),
                Arg.Any<ReplaceOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(accepted));

        var updated = await repository.UpdateAsync(entity);

        updated.Version.ShouldBe(5);
        filter.ShouldNotBeNull();
        FilterContainsVersion(filter!, 4).ShouldBeTrue();
        await collection.DidNotReceive().InsertOneAsync(
            Arg.Any<VersionedOrder>(),
            Arg.Any<InsertOneOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ThrowsAndRestoresVersion_WhenStoredVersionIsNewer()
    {
        var entity = VersionedOrder.Create(Guid.NewGuid(), "stale", 4);
        var repository = CreateRepository(out var collection);
        var missed = MissedReplace();
        collection.ReplaceOneAsync(
                Arg.Any<FilterDefinition<VersionedOrder>>(),
                Arg.Any<VersionedOrder>(),
                Arg.Any<ReplaceOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(missed));

        var exception = await Should.ThrowAsync<OptimisticConcurrencyException>(() => repository.UpdateAsync(entity));

        exception.EntityType.ShouldBe(typeof(VersionedOrder));
        exception.Key.ShouldBe(entity.Id);
        exception.ExpectedVersion.ShouldBe(4);
        entity.Version.ShouldBe(4);
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenStoredVersionDoesNotMatch()
    {
        var entity = VersionedOrder.Create(Guid.NewGuid(), "kept", 4);
        var repository = CreateRepository(out var collection);
        var missed = DeleteResult(0);
        collection.DeleteOneAsync(Arg.Any<FilterDefinition<VersionedOrder>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(missed));

        var exception = await Should.ThrowAsync<OptimisticConcurrencyException>(() => repository.DeleteAsync(entity));

        exception.ExpectedVersion.ShouldBe(4);
        entity.Version.ShouldBe(4);
    }

    [Fact]
    public async Task DeleteAsync_RemovesDocument_WhenStoredVersionMatches()
    {
        var entity = VersionedOrder.Create(Guid.NewGuid(), "doomed", 4);
        var repository = CreateRepository(out var collection);
        var deleted = DeleteResult(1);
        FilterDefinition<VersionedOrder>? filter = null;
        collection.DeleteOneAsync(
                Arg.Do<FilterDefinition<VersionedOrder>>(candidate => filter = candidate),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(deleted));

        await repository.DeleteAsync(entity);

        filter.ShouldNotBeNull();
        FilterContainsVersion(filter!, 4).ShouldBeTrue();
        entity.Version.ShouldBe(4);
    }

    [Fact]
    public async Task InsertOrUpdateAsync_InsertsOriginalVersion_WhenDocumentIsMissing()
    {
        var entity = VersionedOrder.Create(Guid.NewGuid(), "new", 0);
        var repository = CreateRepository(out var collection);
        var missed = MissedReplace();
        collection.ReplaceOneAsync(
                Arg.Any<FilterDefinition<VersionedOrder>>(),
                Arg.Any<VersionedOrder>(),
                Arg.Any<ReplaceOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(missed));

        var saved = await repository.InsertOrUpdateAsync(entity);

        saved.Version.ShouldBe(0);
        await collection.Received(1).InsertOneAsync(entity, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InsertOrUpdateAsync_Throws_WhenInsertHitsAnExistingDocument()
    {
        var entity = VersionedOrder.Create(Guid.NewGuid(), "race", 0);
        var repository = CreateRepository(out var collection);
        var missed = MissedReplace();
        collection.ReplaceOneAsync(
                Arg.Any<FilterDefinition<VersionedOrder>>(),
                Arg.Any<VersionedOrder>(),
                Arg.Any<ReplaceOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(missed));
        collection.InsertOneAsync(entity, null, Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw DuplicateKey());

        var exception = await Should.ThrowAsync<OptimisticConcurrencyException>(() => repository.InsertOrUpdateAsync(entity));

        exception.ExpectedVersion.ShouldBe(0);
        exception.InnerException.ShouldBeOfType<MongoWriteException>();
        entity.Version.ShouldBe(0);
    }

    [Fact]
    public void Update_OnObjectIdRepository_IncrementsVersion_WhenStoredVersionMatches()
    {
        var entity = VersionedOrder.Create(Guid.NewGuid(), "sync", 2);
        var repository = CreateRepositoryOfType(out var collection);
        var accepted = AcceptedReplace();
        collection.ReplaceOne(
                Arg.Any<FilterDefinition<VersionedOrder>>(),
                Arg.Any<VersionedOrder>(),
                Arg.Any<ReplaceOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(accepted);

        var updated = repository.Update(entity);

        updated.Version.ShouldBe(3);
    }

    [Fact]
    public async Task UpdateAsync_LeavesNonVersionedDocumentOnLastWriteWins()
    {
        var entity = new PlainOrder { Id = Guid.NewGuid(), Name = "plain" };
        var database = Substitute.For<IMongoDatabase>();
        var collection = Substitute.For<IMongoCollection<PlainOrder>>();
        database.GetCollection<PlainOrder>(Arg.Any<string>(), Arg.Any<MongoCollectionSettings>()).Returns(collection);
        var repository = new MongoBaseRepository<PlainOrder, Guid>(database, "orders");

        await repository.UpdateAsync(entity);

        await collection.Received(1).ReplaceOneAsync(
            Arg.Any<FilterDefinition<PlainOrder>>(),
            entity,
            Arg.Is<ReplaceOptions>(options => !options.IsUpsert),
            Arg.Any<CancellationToken>());
    }

    private static MongoBaseRepository<VersionedOrder, Guid> CreateRepository(out IMongoCollection<VersionedOrder> collection)
    {
        var database = Substitute.For<IMongoDatabase>();
        collection = Substitute.For<IMongoCollection<VersionedOrder>>();
        database.GetCollection<VersionedOrder>(Arg.Any<string>(), Arg.Any<MongoCollectionSettings>()).Returns(collection);
        return new MongoBaseRepository<VersionedOrder, Guid>(database, "orders");
    }

    private static MongoBaseRepositoryOfType<VersionedOrder, Guid> CreateRepositoryOfType(out IMongoCollection<VersionedOrder> collection)
    {
        var database = Substitute.For<IMongoDatabase>();
        var provider = Substitute.For<IMongoDatabaseProvider>();
        collection = Substitute.For<IMongoCollection<VersionedOrder>>();
        database.GetCollection<VersionedOrder>(Arg.Any<string>(), Arg.Any<MongoCollectionSettings>()).Returns(collection);
        provider.Database.Returns(database);
        return new MongoBaseRepositoryOfType<VersionedOrder, Guid>(provider);
    }

    private static ReplaceOneResult AcceptedReplace()
    {
        var result = Substitute.For<ReplaceOneResult>();
        result.IsAcknowledged.Returns(true);
        result.MatchedCount.Returns(1);
        return result;
    }

    private static ReplaceOneResult MissedReplace()
    {
        var result = Substitute.For<ReplaceOneResult>();
        result.IsAcknowledged.Returns(true);
        result.MatchedCount.Returns(0);
        return result;
    }

    private static DeleteResult DeleteResult(long deletedCount)
    {
        var result = Substitute.For<DeleteResult>();
        result.IsAcknowledged.Returns(true);
        result.DeletedCount.Returns(deletedCount);
        return result;
    }

    private static MongoWriteException DuplicateKey()
    {
        var connectionId = new ConnectionId(new ServerId(new ClusterId(), new System.Net.DnsEndPoint("localhost", 27017)), 1);
        var error = (WriteError)typeof(WriteError)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(constructor => constructor.GetParameters().Length == 4)
            .Invoke([ServerErrorCategory.DuplicateKey, 11000, "duplicate key", new BsonDocument()]);
        return new MongoWriteException(connectionId, error, writeConcernError: null, innerException: null);
    }

    private static bool FilterContainsVersion(object? node, long expectedVersion)
        => FilterContainsVersion(node, expectedVersion, []);

    private static bool FilterContainsVersion(object? node, long expectedVersion, HashSet<object> visited)
    {
        if (node is null || node is string || node is Type)
        {
            return false;
        }

        if (node is long number)
        {
            return number == expectedVersion;
        }

        if (node is int integer)
        {
            return integer == expectedVersion;
        }

        if (node is System.Linq.Expressions.Expression expression)
        {
            return ExpressionContainsVersion(expression, expectedVersion);
        }

        if (!node.GetType().IsClass || !visited.Add(node))
        {
            return false;
        }

        foreach (var field in node.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (FilterContainsVersion(field.GetValue(node), expectedVersion, visited))
            {
                return true;
            }
        }

        if (node is System.Collections.IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                if (FilterContainsVersion(item, expectedVersion, visited))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool ExpressionContainsVersion(System.Linq.Expressions.Expression expression, long expectedVersion)
    {
        if (expression is System.Linq.Expressions.ConstantExpression constant)
        {
            return constant.Value is long number && number == expectedVersion
                || constant.Value is int integer && integer == expectedVersion;
        }

        return expression switch
        {
            System.Linq.Expressions.UnaryExpression unary => ExpressionContainsVersion(unary.Operand, expectedVersion),
            System.Linq.Expressions.BinaryExpression binary => ExpressionContainsVersion(binary.Left, expectedVersion) || ExpressionContainsVersion(binary.Right, expectedVersion),
            System.Linq.Expressions.MemberExpression member => member.Expression is not null && ExpressionContainsVersion(member.Expression, expectedVersion),
            _ => false
        };
    }

    public sealed class VersionedOrder : IEntity<Guid>, IVersioned
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public long Version { get; private set; }

        public static VersionedOrder Create(Guid id, string name, long version)
            => new()
            {
                Id = id,
                Name = name,
                Version = version
            };

        public bool IsTransient() => Id == Guid.Empty;
    }

    public sealed class PlainOrder : IEntity<Guid>
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsTransient() => Id == Guid.Empty;
    }
}
