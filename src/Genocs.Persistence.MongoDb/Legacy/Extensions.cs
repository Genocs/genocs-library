using Genocs.Core.Builders;
using Genocs.Core.Types;
using Genocs.Persistence.MongoDb.Legacy.Builders;
using Genocs.Persistence.MongoDb.Legacy.Factories;
using Genocs.Persistence.MongoDb.Legacy.Initializers;
using Genocs.Persistence.MongoDb.Legacy.Repositories;
using Genocs.Persistence.MongoDb.Legacy.Seeders;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System;

namespace Genocs.Persistence.MongoDb.Legacy;

public static class Extensions
{
    // Helpful when dealing with integration testing
    private static bool _conventionsRegistered;
    private const string SectionName = "mongo";
    private const string RegistryName = "persistence.mongoDb";

    public static IGenocsBuilder AddMongo(this IGenocsBuilder builder, string sectionName = SectionName,
        Type seederType = null, bool registerConventions = true)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = SectionName;
        }

        var mongoOptions = builder.GetOptions<MongoDbOptions>(sectionName);
        return builder.AddMongo(mongoOptions, seederType, registerConventions);
    }

    public static IGenocsBuilder AddMongo(this IGenocsBuilder builder, Func<IMongoDbOptionsBuilder,
        IMongoDbOptionsBuilder> buildOptions, Type seederType = null, bool registerConventions = true)
    {
        var mongoOptions = buildOptions(new MongoDbOptionsBuilder()).Build();
        return builder.AddMongo(mongoOptions, seederType, registerConventions);
    }

    public static IGenocsBuilder AddMongo(this IGenocsBuilder builder, MongoDbOptions mongoOptions,
        Type seederType = null, bool registerConventions = true)
    {
        if (!builder.TryRegister(RegistryName))
        {
            return builder;
        }

        if (mongoOptions.SetRandomDatabaseSuffix)
        {
            var suffix = $"{Guid.NewGuid():N}";
            Console.WriteLine($"Setting a random MongoDB database suffix: '{suffix}'.");
            mongoOptions.Database = $"{mongoOptions.Database}_{suffix}";
        }

        builder.Services.AddSingleton(mongoOptions);
        builder.Services.AddSingleton<IMongoClient>(sp =>
        {
            var options = sp.GetRequiredService<MongoDbOptions>();
            return new MongoClient(options.ConnectionString);
        });
        builder.Services.AddTransient(sp =>
        {
            var options = sp.GetRequiredService<MongoDbOptions>();
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(options.Database);
        });
        builder.Services.AddTransient<IMongoDbInitializer, MongoDbInitializer>();
        builder.Services.AddTransient<IMongoSessionFactory, MongoSessionFactory>();

        if (seederType is null)
        {
            builder.Services.AddTransient<IMongoDbSeeder, MongoDbSeeder>();
        }
        else
        {
            builder.Services.AddTransient(typeof(IMongoDbSeeder), seederType);
        }

        builder.AddInitializer<IMongoDbInitializer>();
        if (registerConventions && !_conventionsRegistered)
        {
            RegisterConventions();
        }

        return builder;
    }

    private static void RegisterConventions()
    {
        _conventionsRegistered = true;
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

    public static IGenocsBuilder AddMongoRepository<TEntity, TIdentifiable>(this IGenocsBuilder builder,
        string collectionName)
        where TEntity : IIdentifiable<TIdentifiable>
    {
        builder.Services.AddTransient<IMongoRepository<TEntity, TIdentifiable>>(sp =>
        {
            var database = sp.GetRequiredService<IMongoDatabase>();
            return new MongoRepository<TEntity, TIdentifiable>(database, collectionName);
        });

        return builder;
    }
}