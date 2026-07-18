using Genocs.Persistence.EFCore.Configurations;
using Genocs.Persistence.EFCore.Providers;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Genocs.Persistence.EFCore.UnitTests.Configurations;

public class ConnectionStringSecurerTests
{
    [Fact]
    public void MakeSecure_MasksMongoCredentials_ForSingleHostUri()
    {
        var sut = CreateSut("mongodb");

        string? secured = sut.MakeSecure("mongodb://user:pass@localhost:27017/bookstore", "mongodb");

        secured.ShouldBe("mongodb://*******:*******@localhost:27017/bookstore");
    }

    [Fact]
    public void MakeSecure_MasksMongoCredentials_ForMultiHostConnectionString()
    {
        var sut = CreateSut("mongodb");

        string? secured = sut.MakeSecure("mongodb://user:pass@host1:27017,host2:27017/bookstore?replicaSet=rs0", "mongodb");

        secured.ShouldBe("mongodb://*******:*******@host1:27017,host2:27017/bookstore?replicaSet=rs0");
    }

    [Fact]
    public void MakeSecure_LeavesMongoConnectionStringWithoutCredentialsUnchanged()
    {
        var sut = CreateSut("mongodb");
        const string connectionString = "mongodb://localhost:27017/bookstore";

        string? secured = sut.MakeSecure(connectionString, "mongodb");

        secured.ShouldBe(connectionString);
    }

    private static ConnectionStringSecurer CreateSut(string dbProvider)
    {
        var options = Options.Create(new DatabaseOptions
        {
            DBProvider = dbProvider,
            ConnectionString = string.Empty
        });

        return new ConnectionStringSecurer([new MongoDbProvider()], options);
    }
}
