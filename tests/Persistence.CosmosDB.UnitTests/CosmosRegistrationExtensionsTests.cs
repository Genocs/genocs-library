using Genocs.Core.Builders;
using Genocs.Persistence.CosmosDB.Configurations;
using Genocs.Persistence.CosmosDB.Domain.Entities;
using Genocs.Persistence.CosmosDB.Domain.Repositories;
using Genocs.Persistence.CosmosDB.Extensions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Genocs.Persistence.CosmosDB.UnitTests;

public class CosmosRegistrationExtensionsTests
{
    [Fact]
    public void AddCosmos_WithValidOptions_RegistersClientAndDatabase()
    {
        var services = new ServiceCollection();
        IGenocsBuilder builder = services.AddGenocs(new ConfigurationBuilder().Build());

        builder.AddCosmos(CreateValidOptions());

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(CosmosOptions));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(CosmosClient));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(Database));
    }

    [Fact]
    public void AddCosmos_WithInvalidOptions_DoesNotRegisterClient()
    {
        var services = new ServiceCollection();
        IGenocsBuilder builder = services.AddGenocs(new ConfigurationBuilder().Build());

        builder.AddCosmos(new CosmosOptions
        {
            ConnectionString = string.Empty,
            Database = "orders",
        });

        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(CosmosClient));
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(Database));
    }

    [Fact]
    public void AddCosmos_WithConfigurationSection_RegistersClient()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["cosmosDb:connectionString"] = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;",
                ["cosmosDb:database"] = "orders",
            })
            .Build();

        IGenocsBuilder builder = services.AddGenocs(configuration);
        builder.AddCosmos();

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(CosmosClient));
    }

    [Fact]
    public void AddCosmosRepository_RegistersTypedRepository()
    {
        var services = new ServiceCollection();
        IGenocsBuilder builder = services.AddGenocs(new ConfigurationBuilder().Build());

        builder.AddCosmosRepository<OrderDocument, Guid>("orders");

        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(ICosmosRepository<OrderDocument, Guid>)
            && descriptor.Lifetime == ServiceLifetime.Transient);
    }

    [Fact]
    public void AddCosmosRepository_RejectsUnsupportedKey()
    {
        var services = new ServiceCollection();
        IGenocsBuilder builder = services.AddGenocs(new ConfigurationBuilder().Build());

        Assert.Throws<NotSupportedException>(() => builder.AddCosmosRepository<IntOrderDocument, int>("orders"));
    }

    private static CosmosOptions CreateValidOptions()
        => new()
        {
            ConnectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;",
            Database = "orders",
        };

    private sealed class OrderDocument : ICosmosEntity<Guid>
    {
        public Guid Id { get; set; }

        public string PartitionKey { get; set; } = string.Empty;

        public bool IsTransient() => Id == Guid.Empty;
    }

    private sealed class IntOrderDocument : ICosmosEntity<int>
    {
        public int Id { get; set; }

        public string PartitionKey { get; set; } = string.Empty;

        public bool IsTransient() => Id == 0;
    }
}
