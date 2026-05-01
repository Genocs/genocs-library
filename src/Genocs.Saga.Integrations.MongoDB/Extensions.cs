using Genocs.Common.Configurations;
using Genocs.Core.Builders;
using Genocs.Persistence.MongoDB.Configurations;
using Genocs.Persistence.MongoDB.Extensions;
using Genocs.Saga.Integrations.MongoDB.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Genocs.Saga.Integrations.MongoDB;

/// <summary>
/// Extension methods that wire saga persistence to MongoDB.
/// MongoDB client/database registration is delegated to <c>Genocs.Persistence.MongoDB</c>
/// so the saga integration shares the same <see cref="IMongoClient"/> pipeline used across the host.
/// </summary>
public static class Extensions
{
    private static readonly object RegistrationLock = new();
    private static bool _conventionsRegistered;

    private static string DeserializationError =>
        $"Could not deserialize given appsettings. Ensure '{MongoOptions.Position}' contains non-empty ConnectionString and Database values.";

    /// <summary>
    /// Configures saga state and saga log persistence to use an <see cref="IMongoDatabase"/>
    /// already registered in the service collection (typically via <c>AddMongo</c> from
    /// <c>Genocs.Persistence.MongoDB</c>). Use this overload to share a single MongoDB client
    /// across the application.
    /// </summary>
    /// <param name="builder">The saga builder.</param>
    /// <returns>The saga builder so calls can be chained.</returns>
    public static ISagaBuilder UseMongoPersistence(this ISagaBuilder builder)
    {
        builder.UseSagaLog<MongoSagaLog>();
        builder.UseSagaStateRepository<MongoSagaStateRepository>();

        RegisterConventions();

        return builder;
    }

    /// <summary>
    /// Configures saga state and saga log persistence to use MongoDB. Reads connection
    /// settings from the supplied <paramref name="configuration"/> and registers the
    /// shared MongoDB client through <c>Genocs.Persistence.MongoDB</c>.
    /// </summary>
    /// <param name="builder">The saga builder.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="sectionName">The configuration section name. Defaults to <see cref="MongoOptions.Position"/>.</param>
    /// <returns>The saga builder so calls can be chained.</returns>
    public static ISagaBuilder UseMongoPersistence(this ISagaBuilder builder, IConfiguration configuration, string sectionName = MongoOptions.Position)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = MongoOptions.Position;
        }

        MongoOptions settings = ResolveSettings(configuration, sectionName);

        return builder.UseMongoPersistence(settings);
    }

    /// <summary>
    /// Configures saga state and saga log persistence to use MongoDB using the supplied
    /// <see cref="MongoOptions"/>. Registers the shared MongoDB client through
    /// <c>Genocs.Persistence.MongoDB</c>.
    /// </summary>
    /// <param name="builder">The saga builder.</param>
    /// <param name="settings">The MongoDB connection options.</param>
    /// <returns>The saga builder so calls can be chained.</returns>
    public static ISagaBuilder UseMongoPersistence(this ISagaBuilder builder, MongoOptions settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (!MongoOptions.IsValid(settings))
        {
            throw new InvalidConfigurationException(DeserializationError);
        }

        builder.Services.AddMongoClient(settings);

        return builder.UseMongoPersistence();
    }

    private static MongoOptions ResolveSettings(IConfiguration configuration, string sectionName)
    {
        var settings = configuration.GetOptions<MongoOptions>(sectionName);
        if (MongoOptions.IsValid(settings))
        {
            return settings;
        }

        throw new InvalidConfigurationException(DeserializationError);
    }

    private static void RegisterConventions()
    {
        lock (RegistrationLock)
        {
            if (_conventionsRegistered)
            {
                return;
            }

            var pack = new ConventionPack
            {
                new ObjectSerializerAllowedTypesConvention(type =>
                    ObjectSerializer.DefaultAllowedTypes(type) || IsApplicationType(type)),
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String),
            };

            ConventionRegistry.Register("SagaAllowedTypes", pack, _ => true);
            _conventionsRegistered = true;
        }
    }

    private static bool IsApplicationType(Type type)
    {
        if (type.IsGenericType)
        {
            return type.GetGenericArguments().All(IsApplicationType);
        }

        if (type.IsArray)
        {
            return type.GetElementType() is Type elementType && IsApplicationType(elementType);
        }

        string assemblyName = type.Assembly.GetName().Name;
        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            return false;
        }

        return !assemblyName.Equals("mscorlib", StringComparison.Ordinal)
            && !assemblyName.Equals("netstandard", StringComparison.Ordinal)
            && !assemblyName.Equals("System.Private.CoreLib", StringComparison.Ordinal)
            && !assemblyName.StartsWith("System.", StringComparison.Ordinal)
            && !assemblyName.StartsWith("Microsoft.", StringComparison.Ordinal);
    }
}
