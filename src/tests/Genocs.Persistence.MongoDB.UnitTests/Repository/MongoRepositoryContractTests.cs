using Genocs.Common.Domain.Entities;
using Genocs.Persistence.MongoDB.Domain.Repositories;
using MongoDB.Driver;
using NSubstitute;
using Xunit;

namespace Genocs.Persistence.MongoDB.UnitTests.Repository;

public class MongoRepositoryContractTests
{
    [Fact]
    public void Update_ById_AppliesAction_AndPersistsEntity()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "before" };
        var repository = CreateMongoBaseRepository(entity, out var collection);

        // Act
        var updated = repository.Update(entity.Id, e => e.Name = "after");

        // Assert
        Assert.Equal("after", updated.Name);
        collection.Received(1).ReplaceOne(
            Arg.Any<FilterDefinition<TestEntity>>(),
            updated,
            Arg.Any<ReplaceOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ById_AppliesAction_AndPersistsEntity()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "before" };
        var repository = CreateMongoBaseRepository(entity, out var collection);

        // Act
        var updated = await repository.UpdateAsync(entity.Id, e =>
        {
            e.Name = "after";
            return Task.CompletedTask;
        });

        // Assert
        Assert.Equal("after", updated.Name);
        await collection.Received(1).ReplaceOneAsync(
            Arg.Any<FilterDefinition<TestEntity>>(),
            updated,
            Arg.Any<ReplaceOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAsync_ForRepositoryOfType_UsesInsertOneAsyncWithCancellationToken()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "insert-async" };
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var repository = CreateMongoBaseRepositoryOfType(entity, out var collection);

        // Act
        await repository.AddAsync(entity, cancellationToken);

        // Assert
        await collection.Received(1).InsertOneAsync(entity, null, cancellationToken);
    }

    [Fact]
    public async Task GetByIdAsync_ForRepositoryOfType_ReturnsEntityWhenFound()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "sample" };
        var repository = CreateMongoBaseRepositoryOfType(entity);

        // Act
        var result = await repository.GetByIdAsync(entity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
    }

    [Fact]
    public void Update_ForRepositoryOfType_UsesSynchronousReplaceOne()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "sync-update" };
        var repository = CreateMongoBaseRepositoryOfType(entity, out var collection);

        // Act
        var updated = repository.Update(entity);

        // Assert
        Assert.Equal(entity.Id, updated.Id);
        collection.Received(1).ReplaceOne(
            Arg.Any<FilterDefinition<TestEntity>>(),
            updated,
            Arg.Any<ReplaceOptions>(),
            Arg.Any<CancellationToken>());
        collection.DidNotReceive().ReplaceOneAsync(
            Arg.Any<FilterDefinition<TestEntity>>(),
            Arg.Any<TestEntity>(),
            Arg.Any<ReplaceOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Delete_ForRepositoryOfType_UsesSynchronousDeleteOne()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "sync-delete" };
        var repository = CreateMongoBaseRepositoryOfType(entity, out var collection);

        // Act
        repository.Delete(entity.Id);

        // Assert
        collection.Received(1).DeleteOne(
            Arg.Any<FilterDefinition<TestEntity>>(),
            Arg.Any<CancellationToken>());
        collection.DidNotReceive().DeleteOneAsync(
            Arg.Any<FilterDefinition<TestEntity>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_ById_UsesDeleteOneAsync()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "delete-async" };
        var repository = CreateMongoBaseRepository(entity, out var collection);

        // Act
        await repository.DeleteAsync(entity.Id);

        // Assert
        await collection.Received(1).DeleteOneAsync(
            Arg.Any<FilterDefinition<TestEntity>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ById_ReturnsEntityWhenFound()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "query" };
        var repository = CreateMongoBaseRepository(entity, out _);

        // Act
        var result = await repository.FirstOrDefaultAsync(entity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
    }

    private static MongoBaseRepository<TestEntity, Guid> CreateMongoBaseRepository(TestEntity entity, out IMongoCollection<TestEntity> collection)
    {
        var database = Substitute.For<IMongoDatabase>();
        collection = Substitute.For<IMongoCollection<TestEntity>>();
        var cursor = CreateSingleEntityCursor(entity);

        collection.FindSync(
            Arg.Any<FilterDefinition<TestEntity>>(),
            Arg.Any<FindOptions<TestEntity, TestEntity>>(),
            Arg.Any<CancellationToken>())
            .Returns(cursor);

        collection.FindAsync(
            Arg.Any<FilterDefinition<TestEntity>>(),
            Arg.Any<FindOptions<TestEntity, TestEntity>>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(cursor));

        collection.ReplaceOneAsync(
            Arg.Any<FilterDefinition<TestEntity>>(),
            Arg.Any<TestEntity>(),
            Arg.Any<ReplaceOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Substitute.For<ReplaceOneResult>()));

        collection.DeleteOneAsync(
            Arg.Any<FilterDefinition<TestEntity>>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Substitute.For<DeleteResult>()));

        database.GetCollection<TestEntity>(Arg.Any<string>(), Arg.Any<MongoCollectionSettings>())
            .Returns(collection);

        return new MongoBaseRepository<TestEntity, Guid>(database, "test-collection");
    }

    private static MongoBaseRepositoryOfType<TestEntity, Guid> CreateMongoBaseRepositoryOfType(TestEntity entity)
        => CreateMongoBaseRepositoryOfType(entity, out _);

    private static MongoBaseRepositoryOfType<TestEntity, Guid> CreateMongoBaseRepositoryOfType(TestEntity entity, out IMongoCollection<TestEntity> collection)
    {
        var database = Substitute.For<IMongoDatabase>();
        var provider = Substitute.For<IMongoDatabaseProvider>();
        collection = Substitute.For<IMongoCollection<TestEntity>>();
        var cursor = CreateSingleEntityCursor(entity);

        collection.FindSync(
            Arg.Any<FilterDefinition<TestEntity>>(),
            Arg.Any<FindOptions<TestEntity, TestEntity>>(),
            Arg.Any<CancellationToken>())
            .Returns(cursor);

        collection.FindAsync(
            Arg.Any<FilterDefinition<TestEntity>>(),
            Arg.Any<FindOptions<TestEntity, TestEntity>>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(cursor));

        collection.ReplaceOne(
            Arg.Any<FilterDefinition<TestEntity>>(),
            Arg.Any<TestEntity>(),
            Arg.Any<ReplaceOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Substitute.For<ReplaceOneResult>());

        collection.DeleteOne(
            Arg.Any<FilterDefinition<TestEntity>>(),
            Arg.Any<CancellationToken>())
            .Returns(Substitute.For<DeleteResult>());

        collection.InsertOneAsync(
            Arg.Any<TestEntity>(),
            Arg.Any<InsertOneOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        database.GetCollection<TestEntity>(Arg.Any<string>(), Arg.Any<MongoCollectionSettings>())
            .Returns(collection);

        provider.Database.Returns(database);

        return new MongoBaseRepositoryOfType<TestEntity, Guid>(provider);
    }

    private static IAsyncCursor<TestEntity> CreateSingleEntityCursor(TestEntity entity)
    {
        var cursor = Substitute.For<IAsyncCursor<TestEntity>>();
        cursor.Current.Returns(new[] { entity });
        cursor.MoveNext(Arg.Any<CancellationToken>()).Returns(true, false);
        cursor.MoveNextAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(true), Task.FromResult(false));
        return cursor;
    }

    public sealed class TestEntity : IEntity<Guid>
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsTransient() => Id == Guid.Empty;
    }
}
