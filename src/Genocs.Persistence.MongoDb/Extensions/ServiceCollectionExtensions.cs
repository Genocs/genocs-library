using Genocs.Persistence.MongoDb.Options;
using Genocs.Persistence.MongoDb.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using System.Reflection;

namespace Genocs.Persistence.MongoDb.Extensions;

/// <summary>
/// ServiceCollectionExtension for mongoDb Repository setup
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Setup the MongoDatabase
    /// </summary>
    /// <param name="services">Service Collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns></returns>
    public static IServiceCollection AddMongoDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(MongoDbSettings.Position);

        if (!section.Exists())
        {
            return services;
        }

        services.Configure<MongoDbSettings>(section);

        services.AddSingleton<IMongoDatabaseProvider, MongoDatabaseProvider>();
        services.AddScoped(typeof(IMongoDbRepository<>), typeof(MongoDbRepository<>));

        RegisterConventions();

        return services;
    }

    /// <summary>
    /// Register all the MongoDb databases
    /// </summary>
    /// <param name="services">Service Collection</param>
    /// <param name="assembly">Assembly to scan</param>
    /// <param name="lifetime">Kind of ServiceLifetime</param>
    /// <returns></returns>
    public static IServiceCollection RegisterRepositories(this IServiceCollection services, Assembly assembly,
                      ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        services
            .Scan(s => s.FromAssemblyDependencies(assembly)
            .AddClasses(c => c.AssignableTo(typeof(IMongoDbRepository<>)))
            .AsImplementedInterfaces()
            .WithLifetime(lifetime));

        return services;
    }

    internal static void RegisterConventions()
    {
        BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
        BsonSerializer.RegisterSerializer(typeof(decimal?),
            new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));
        ConventionRegistry.Register("genocs", new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new IgnoreExtraElementsConvention(true),
            new EnumRepresentationConvention(BsonType.String),
        }, _ => true);
    }
}
