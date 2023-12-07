using Genocs.Common.Types;
using Genocs.Core.Builders;
using Genocs.Persistence.MongoDb.Builders;
using Genocs.Persistence.MongoDb.Factories;
using Genocs.Persistence.MongoDb.Initializers;
using Genocs.Persistence.MongoDb.Options;
using Genocs.Persistence.MongoDb.Repositories;
using Genocs.Persistence.MongoDb.Repositories.Clean;
using Genocs.Persistence.MongoDb.Repositories.Mentor;
using Genocs.Persistence.MongoDb.Seeders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using System.Reflection;

namespace Genocs.Persistence.MongoDb.Extensions;

/// <summary>
/// The MongoDb Extensions.
/// </summary>
public static class MongoDbExtensions
{
    // Helpful when dealing with integration testing
    private static bool _conventionsRegistered;

    /// <summary>
    /// It allows to add support for MongoDb.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <param name="sectionName">The section name.</param>
    /// <param name="seederType">The seeder name.</param>
    /// <param name="registerConventions">Defines if setup the MongoDB Conventions.</param>
    /// <returns>The Genocs builder.</returns>
    public static IGenocsBuilder AddMongo(
                                          this IGenocsBuilder builder,
                                          string sectionName = MongoDbSettings.Position,
                                          Type? seederType = null,
                                          bool registerConventions = true)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = MongoDbSettings.Position;
        }

        var mongoOptions = builder.GetOptions<MongoDbSettings>(sectionName);
        return builder.AddMongo(mongoOptions, seederType, registerConventions);
    }

    /// <summary>
    /// It allows to add support for MongoDb.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <param name="buildOptions">The Genocs builder.</param>
    /// <param name="seederType">The seeder name.</param>
    /// <param name="registerConventions">Defines if setup the MongoDB Conventions.</param>
    /// <returns>The Genocs builder.</returns>
    public static IGenocsBuilder AddMongo(
                                          this IGenocsBuilder builder,
                                          Func<IMongoDbOptionsBuilder,
                                          IMongoDbOptionsBuilder> buildOptions,
                                          Type? seederType = null,
                                          bool registerConventions = true)
    {
        var mongoOptions = buildOptions(new MongoDbOptionsBuilder()).Build();
        return builder.AddMongo(mongoOptions, seederType, registerConventions);
    }

    /// <summary>
    /// Setup MongoDb support.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <param name="mongoOptions">The settings.</param>
    /// <param name="seederType"></param>
    /// <param name="registerConventions"></param>
    /// <returns>The Genocs builder.</returns>
    public static IGenocsBuilder AddMongo(
                                          this IGenocsBuilder builder,
                                          MongoDbSettings mongoOptions,
                                          Type? seederType = null,
                                          bool registerConventions = true)
    {
        if (!builder.TryRegister(MongoDbSettings.Position))
        {
            return builder;
        }

        if (mongoOptions.SetRandomDatabaseSuffix)
        {
            string suffix = $"{Guid.NewGuid():N}";
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

        // Setup conventions
        if (registerConventions && !_conventionsRegistered)
        {
            ServiceCollectionExtensions.RegisterConventions();
        }

        return builder;
    }

    /// <summary>
    /// Adds a MongoDb repository to the DI container. Using Genocs builder support.
    /// </summary>
    /// <typeparam name="TEntity">The name of the entity.</typeparam>
    /// <typeparam name="TIdentifiable">The kind of identifier.</typeparam>
    /// <param name="builder">The Genocs builder.</param>
    /// <param name="collectionName">The collection name where to store data.</param>
    /// <returns>The Genocs builder.</returns>
    public static IGenocsBuilder AddMongoRepository<TEntity, TIdentifiable>(
                                                                            this IGenocsBuilder builder,
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

    /// <summary>
    /// Add MongoDb support.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <param name="sectionName">The Genocs builder.</param>
    /// <param name="registerConventions">The Genocs builder.</param>
    /// <returns>The Genocs builder.</returns>
    public static IGenocsBuilder AddMongoFast(
                                              this IGenocsBuilder builder,
                                              string sectionName = MongoDbSettings.Position,
                                              bool registerConventions = true)
    {

        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = MongoDbSettings.Position;
        }

        var section = builder.Configuration.GetSection(sectionName);

        if (!section.Exists())
        {
            return builder;
        }

        builder.Services.Configure<MongoDbSettings>(section);

        builder.Services.AddSingleton<IMongoDatabaseProvider, MongoDatabaseProvider>();
        builder.Services.AddScoped(typeof(IMongoDbRepository<>), typeof(MongoDbRepository<>));

        if (registerConventions && !_conventionsRegistered)
        {
            ServiceCollectionExtensions.RegisterConventions();
        }

        return builder;
    }

    /// <summary>
    /// Register all the default MongoDb repository.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <param name="assembly">Assembly to scan.</param>
    /// <param name="lifetime">Kind of ServiceLifetime.</param>
    /// <returns>The Genocs builder.</returns>
    public static IGenocsBuilder RegisterMongoRepositories(
                                                           this IGenocsBuilder builder,
                                                           Assembly assembly,
                                                           ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        builder.Services
            .Scan(s => s.FromAssemblyDependencies(assembly)
            .AddClasses(c => c.AssignableTo(typeof(IMongoDbRepository<>)))
            .AsImplementedInterfaces()
            .WithLifetime(lifetime));

        return builder;
    }
}