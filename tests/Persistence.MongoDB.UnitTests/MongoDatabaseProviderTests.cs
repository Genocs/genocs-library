using Genocs.Persistence.MongoDB.Configurations;
using MongoDB.Driver;
using NSubstitute;
using Xunit;

namespace Genocs.Persistence.MongoDB.UnitTests;

public class MongoDatabaseProviderTests
{
    [Fact]
    public void Constructor_UsesInjectedClient_AndConfiguredDatabase()
    {
        var client = Substitute.For<IMongoClient>();
        var database = Substitute.For<IMongoDatabase>();
        var options = new MongoOptions
        {
            ConnectionString = "mongodb://localhost:27017",
            Database = "genocs_test",
        };

        client.GetDatabase(options.Database, Arg.Any<MongoDatabaseSettings?>()).Returns(database);

        var provider = new MongoDatabaseProvider(client, options);

        Assert.Same(client, provider.MongoClient);
        Assert.Same(database, provider.Database);
        client.Received(1).GetDatabase(options.Database, Arg.Any<MongoDatabaseSettings?>());
    }

    [Fact]
    public void Constructor_ThrowsForInvalidOptions()
    {
        var client = Substitute.For<IMongoClient>();
        var options = new MongoOptions
        {
            ConnectionString = string.Empty,
            Database = "genocs_test",
        };

        Assert.Throws<InvalidOperationException>(() => new MongoDatabaseProvider(client, options));
    }
}
