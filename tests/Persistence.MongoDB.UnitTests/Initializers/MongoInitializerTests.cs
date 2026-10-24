using Genocs.Persistence.MongoDB.Configurations;
using Genocs.Persistence.MongoDB.Initializers;
using Genocs.Persistence.MongoDB.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;
using NSubstitute;
using Xunit;

namespace Genocs.Persistence.MongoDB.UnitTests.Initializers;

public class MongoInitializerTests
{
    [Fact]
    public async Task InitializeAsync_WhenSeedDisabled_DoesNotInvokeSeeder()
    {
        MongoInitializer.ResetInitializationStateForTests();

        var database = CreateDatabase("orders");
        var seeder = Substitute.For<IMongoSeeder>();
        var logger = NullLogger<MongoInitializer>.Instance;
        var options = CreateOptions(seed: false);
        var initializer = new MongoInitializer(database, seeder, options, logger);
        var cancellationToken = TestContext.Current.CancellationToken;

        await initializer.InitializeAsync(cancellationToken);

        await seeder.DidNotReceive().SeedAsync(Arg.Any<IMongoDatabase>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InitializeAsync_ForSameDatabase_SeedsOnlyOncePerProcess()
    {
        MongoInitializer.ResetInitializationStateForTests();

        var database = CreateDatabase("orders");
        var seeder = Substitute.For<IMongoSeeder>();
        var logger = NullLogger<MongoInitializer>.Instance;
        var options = CreateOptions(seed: true);

        var firstInitializer = new MongoInitializer(database, seeder, options, logger);
        var secondInitializer = new MongoInitializer(database, seeder, options, logger);
        var cancellationToken = TestContext.Current.CancellationToken;

        await firstInitializer.InitializeAsync(cancellationToken);
        await secondInitializer.InitializeAsync(cancellationToken);

        await seeder.Received(1).SeedAsync(database, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InitializeAsync_ForDifferentDatabases_SeedsEachDatabase()
    {
        MongoInitializer.ResetInitializationStateForTests();

        var firstDatabase = CreateDatabase("orders");
        var secondDatabase = CreateDatabase("billing");
        var seeder = Substitute.For<IMongoSeeder>();
        var logger = NullLogger<MongoInitializer>.Instance;
        var options = CreateOptions(seed: true);

        var firstInitializer = new MongoInitializer(firstDatabase, seeder, options, logger);
        var secondInitializer = new MongoInitializer(secondDatabase, seeder, options, logger);
        var cancellationToken = TestContext.Current.CancellationToken;

        await firstInitializer.InitializeAsync(cancellationToken);
        await secondInitializer.InitializeAsync(cancellationToken);

        await seeder.Received(1).SeedAsync(firstDatabase, Arg.Any<CancellationToken>());
        await seeder.Received(1).SeedAsync(secondDatabase, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InitializeAsync_WhenSeedFails_AllowsRetryForSameDatabase()
    {
        MongoInitializer.ResetInitializationStateForTests();

        var database = CreateDatabase("orders");
        var seeder = Substitute.For<IMongoSeeder>();
        var logger = NullLogger<MongoInitializer>.Instance;
        var options = CreateOptions(seed: true);

        seeder.SeedAsync(database, Arg.Any<CancellationToken>())
            .Returns(
                _ => Task.FromException(new InvalidOperationException("seed failed")),
                _ => Task.CompletedTask);

        var firstInitializer = new MongoInitializer(database, seeder, options, logger);
        var secondInitializer = new MongoInitializer(database, seeder, options, logger);
        var cancellationToken = TestContext.Current.CancellationToken;

        await Assert.ThrowsAsync<InvalidOperationException>(() => firstInitializer.InitializeAsync(cancellationToken));
        await secondInitializer.InitializeAsync(cancellationToken);

        await seeder.Received(2).SeedAsync(database, Arg.Any<CancellationToken>());
    }

    private static MongoOptions CreateOptions(bool seed)
        => new()
        {
            ConnectionString = "mongodb://localhost:27017",
            Database = "orders",
            Seed = seed,
        };

    private static IMongoDatabase CreateDatabase(string databaseName)
    {
        var database = Substitute.For<IMongoDatabase>();
        database.DatabaseNamespace.Returns(new DatabaseNamespace(databaseName));
        return database;
    }
}
