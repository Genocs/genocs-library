using Genocs.Persistence.EFCore.Providers;
using Shouldly;
using Xunit;

namespace Genocs.Persistence.EFCore.UnitTests.Providers;

public class DbProviderResolverTests
{
    private static readonly IEFCoreDbProvider[] Providers =
    [
        new SqlServerDbProvider(),
        new PostgreSqlDbProvider(),
        new MySqlDbProvider(),
        new SqliteDbProvider(),
        new OracleDbProvider(),
        new MongoDbProvider(),
    ];

    [Theory]
    [InlineData("mssql", typeof(SqlServerDbProvider))]
    [InlineData("postgresql", typeof(PostgreSqlDbProvider))]
    [InlineData("mysql", typeof(MySqlDbProvider))]
    [InlineData("sqlite", typeof(SqliteDbProvider))]
    [InlineData("oracle", typeof(OracleDbProvider))]
    [InlineData("mongodb", typeof(MongoDbProvider))]
    public void Resolve_ReturnsMatchingProvider(string key, Type expectedType)
    {
        var provider = Providers.Resolve(key);

        provider.ShouldBeOfType(expectedType);
    }

    [Theory]
    [InlineData("MSSQL")]
    [InlineData("MongoDB")]
    public void Resolve_IsCaseInsensitive(string key)
    {
        var provider = Providers.Resolve(key);

        provider.ProviderKey.ShouldBe(key.ToLowerInvariant());
    }

    [Fact]
    public void Resolve_UnknownProvider_Throws()
    {
        var exception = Should.Throw<InvalidOperationException>(() => Providers.Resolve("cosmosdb"));

        exception.Message.ShouldBe("DB Provider cosmosdb is not supported.");
    }

    [Fact]
    public void TryResolve_UnknownProvider_ReturnsNull()
    {
        Providers.TryResolve("cosmosdb").ShouldBeNull();
    }
}
