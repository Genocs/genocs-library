using Genocs.Common.Types;
using Genocs.Core.Builders;
using Genocs.Persistence.MongoDb.Builders;
using Genocs.Persistence.MongoDb.Legacy.Factories;
using Genocs.Persistence.MongoDb.Legacy.Initializers;
using Genocs.Persistence.MongoDb.Legacy.Repositories;
using Genocs.Persistence.MongoDb.Legacy.Seeders;
using Genocs.Persistence.MongoDb.Options;
using Genocs.Persistence.MongoDb.Repositories;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;

namespace Genocs.Persistence.MongoDb.Legacy;

public static class Extensions
{
    // Helpful when dealing with integration testing
    private static bool _conventionsRegistered;
    private const string RegistryName = "persistence.mongoDb";

    public static IGenocsBuilder AddMongo(this IGenocsBuilder builder, string sectionName = MongoDbSettings.Position,
        Type? seederType = null, bool registerConventions = true)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = MongoDbSettings.Position;
        }

        var mongoOptions = builder.GetOptions<MongoDbSettings>(sectionName);
        return builder.AddMongo(mongoOptions, seederType, registerConventions);
    }

    public static IGenocsBuilder AddMongo(this IGenocsBuilder builder, Func<IMongoDbOptionsBuilder,
        IMongoDbOptionsBuilder> buildOptions, Type? seederType = null, bool registerConventions = true)
    {
        var mongoOptions = buildOptions(new MongoDbOptionsBuilder()).Build();
        return builder.AddMongo(mongoOptions, seederType, registerConventions);
    }

    public static IGenocsBuilder AddMongo(this IGenocsBuilder builder, MongoDbSettings mongoOptions,
        Type? seederType = null, bool registerConventions = true)
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
            var options = sp.GetRequiredService<MongoDbSettings>();

            MongoClientSettings clientSettings = MongoClientSettings.FromConnectionString(options.ConnectionString);

            if (options.EnableTracing)
            {
                clientSettings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());
            }

            return new MongoClient(clientSettings);
        });

        builder.Services.AddTransient(sp =>
        {
            var options = sp.GetRequiredService<MongoDbSettings>();
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
            MongoDb.Extensions.ServiceCollectionExtensions.RegisterConventions();
        }

        return builder;
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