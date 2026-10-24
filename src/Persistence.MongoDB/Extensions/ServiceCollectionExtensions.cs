using Genocs.Persistence.MongoDB.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;

namespace Genocs.Persistence.MongoDB.Extensions;

/// <summary>
/// Service Collection Extension for MongoDB Repository setup.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IMongoClient"/>, <see cref="IMongoDatabase"/>, and the supplied
    /// <see cref="MongoOptions"/> as singletons on the service collection. This is the single
    /// entry point used across Genocs packages (such as the saga integration) to wire up the
    /// MongoDB client pipeline so the same client is reused across the host.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The MongoDB connection options.</param>
    /// <returns>The same service collection so calls can be chained.</returns>
    public static IServiceCollection AddMongoClient(this IServiceCollection services, MongoOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        if (!MongoOptions.IsValid(options))
        {
            throw new InvalidOperationException(
                $"{nameof(MongoOptions)} is invalid. {nameof(options.ConnectionString)} or {nameof(options.Database)} is empty.");
        }

        if (options.SetRandomDatabaseSuffix)
        {
            string suffix = $"{Guid.NewGuid():N}";
            options.Database = $"{options.Database}_{suffix}";
        }

        services.TryAddSingleton(options);

        services.TryAddSingleton<IMongoClient>(sp =>
        {
            var resolvedOptions = sp.GetRequiredService<MongoOptions>();

            MongoClientSettings clientSettings = MongoClientSettings.FromConnectionString(resolvedOptions.ConnectionString);

            if (resolvedOptions.EnableTracing)
            {
                clientSettings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());
            }

            return new MongoClient(clientSettings);
        });

        services.TryAddSingleton<IMongoDatabase>(sp =>
        {
            var resolvedOptions = sp.GetRequiredService<MongoOptions>();
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(resolvedOptions.Database);
        });

        return services;
    }

    internal static void RegisterConventions(MongoGuidRepresentationMode guidRepresentationMode = MongoGuidRepresentationMode.Standard)
    {
        BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));

        BsonSerializer.RegisterSerializer(new GuidSerializer(ToGuidRepresentation(guidRepresentationMode)));

        BsonSerializer.RegisterSerializer(
                                            typeof(decimal?),
                                            new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));

        ConventionRegistry.Register(
            "genocs",
            new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String),
            },
            _ => true);
    }

    internal static GuidRepresentation ToGuidRepresentation(MongoGuidRepresentationMode mode)
    {
        return mode switch
        {
            MongoGuidRepresentationMode.CSharpLegacy => GuidRepresentation.CSharpLegacy,
            _ => GuidRepresentation.Standard,
        };
    }
}
