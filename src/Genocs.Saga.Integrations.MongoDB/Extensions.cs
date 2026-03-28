using Genocs.Common.Configurations;
using Genocs.Core.Builders;
using Genocs.Saga.Integrations.MongoDB.Configurations;
using Genocs.Saga.Integrations.MongoDB.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Genocs.Saga.Integrations.MongoDB;

public static class Extensions
{
    private static readonly object RegistrationLock = new();
    private static bool _conventionsRegistered;

    private static string DeserializationError =>
        $"Could not deserialize given appsettings. Ensure either '{MongoOptions.Position}' contains non-empty ConnectionString and Database values.";

    public static ISagaBuilder UseMongoPersistence(this ISagaBuilder builder, IConfiguration configuration, string sectionName = MongoOptions.Position)
    {
        return builder.UseMongoPersistence(GetDatabase);

        IMongoDatabase GetDatabase(IServiceProvider serviceProvider)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sectionName))
                {
                    sectionName = MongoOptions.Position;
                }

                var settings = ResolveSettings(configuration, sectionName);
                if (!MongoOptions.IsValid(settings))
                {
                    throw new InvalidConfigurationException(DeserializationError);
                }

                return new MongoClient(settings.ConnectionString).GetDatabase(settings.Database);
            }
            catch
            {
                throw new SagaException(DeserializationError);
            }
        }
    }

    public static ISagaBuilder UseMongoPersistence(this ISagaBuilder builder, MongoOptions settings)
    {
        return builder.UseMongoPersistence(GetDatabase);

        IMongoDatabase GetDatabase(IServiceProvider serviceProvider)
        {
            if (!MongoOptions.IsValid(settings))
            {
                throw new InvalidConfigurationException(DeserializationError);
            }

            return new MongoClient(settings.ConnectionString).GetDatabase(settings.Database);
        }
    }

    private static ISagaBuilder UseMongoPersistence(this ISagaBuilder builder, Func<IServiceProvider, IMongoDatabase> getDatabase)
    {
        builder.Services.AddTransient(getDatabase);
        builder.UseSagaLog<MongoSagaLog>();
        builder.UseSagaStateRepository<MongoSagaStateRepository>();

        RegisterConventions();

        return builder;
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

        string? assemblyName = type.Assembly.GetName().Name;
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
