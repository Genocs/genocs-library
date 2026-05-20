using Genocs.Core.Builders;
using Genocs.Persistence.MongoDB.Configurations;
using Genocs.Persistence.MongoDB.Domain.Repositories;
using Genocs.Persistence.MongoDB.Extensions;
using Genocs.Persistence.MongoDB.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xunit;

namespace Genocs.Persistence.MongoDB.UnitTests;

public class MongoRegistrationExtensionsTests
{
    [Fact]
    public void AddMongo_WithValidOptions_RegistersMongoCoreServices()
    {
        var services = new ServiceCollection();
        IGenocsBuilder builder = services.AddGenocs(new ConfigurationBuilder().Build());
        var options = CreateValidOptions();

        builder.AddMongo(options);

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(MongoOptions));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IMongoClient));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IMongoDatabase));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IMongoInitializer));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IMongoSessionFactory));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IMongoDatabaseProvider));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IMongoSeeder));
    }

    [Fact]
    public void AddMongo_WithInvalidOptions_DoesNotRegisterMongoCoreServices()
    {
        var services = new ServiceCollection();
        IGenocsBuilder builder = services.AddGenocs(new ConfigurationBuilder().Build());
        var options = new MongoOptions
        {
            ConnectionString = string.Empty,
            Database = "genocs_test",
        };

        builder.AddMongo(options);

        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(IMongoClient));
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(IMongoDatabase));
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(IMongoInitializer));
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(IMongoDatabaseProvider));
    }

    [Fact]
    public void AddMongoWithRegistration_WithConfiguredSection_RegistersRepositoryOpenGeneric()
    {
        var services = new ServiceCollection();
        IGenocsBuilder builder = services.AddGenocs(CreateMongoConfiguration());

        builder.AddMongoWithRegistration();

        Assert.Contains(services, descriptor =>
            descriptor.ServiceType.IsGenericType
            && descriptor.ServiceType.GetGenericTypeDefinition() == typeof(IMongoRepository<>)
            && descriptor.ImplementationType?.IsGenericType == true
            && descriptor.ImplementationType.GetGenericTypeDefinition() == typeof(MongoRepository<>));
    }

    [Fact]
    public void AddMongoRepository_RegistersTypedRepositoryBinding()
    {
        var services = new ServiceCollection();
        IGenocsBuilder builder = services.AddGenocs(new ConfigurationBuilder().Build());

        builder.AddMongoRepository<TestEntity, Guid>("tests");

        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(IMongoBaseRepository<TestEntity, Guid>)
            && descriptor.Lifetime == ServiceLifetime.Transient);
    }

    private static MongoOptions CreateValidOptions()
        => new()
        {
            ConnectionString = "mongodb://localhost:27017",
            Database = "genocs_test",
            Seed = false,
        };

    private static IConfiguration CreateMongoConfiguration()
        => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["mongoDb:connectionString"] = "mongodb://localhost:27017",
                ["mongoDb:database"] = "genocs_test",
            })
            .Build();

    private sealed class TestEntity : Genocs.Common.Domain.Entities.IEntity<Guid>
    {
        public Guid Id { get; set; }

        public bool IsTransient() => Id == Guid.Empty;
    }
}