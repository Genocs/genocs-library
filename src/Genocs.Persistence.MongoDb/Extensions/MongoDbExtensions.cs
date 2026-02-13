using System.Reflection;
using Genocs.Common.Domain.Entities;
using Genocs.Core.Builders;
using Genocs.Persistence.MongoDb.Configurations;
using Genocs.Persistence.MongoDb.Domain.Repositories;
using Genocs.Persistence.MongoDb.Factories;
using Genocs.Persistence.MongoDb.Initializers;
using Genocs.Persistence.MongoDb.Repositories;
using Genocs.Persistence.MongoDb.Seeders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;

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
    /// <param name="registerConventions">Defines if setup the MongoDB standard Conventions.</param>
    /// <returns>The Genocs builder.</returns>
    public static IGenocsBuilder AddMongo(
                                          this IGenocsBuilder builder,
                                          string sectionName = MongoDbOptions.Position,
                                          Type? seederType = null,
                                          bool registerConventions = true)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = MongoDbOptions.Position;
        }

        var section = builder.Configuration?.GetSection(sectionName);

        if (!section.Exists())
        {
            return builder;
        }

        builder.Services.Configure<MongoDbOptions>(section);

        var mongoOptions = builder.GetOptions<MongoDbOptions>(sectionName);
        return builder.AddMongo(mongoOptions, seederType, registerConventions);
    }

    /// <summary>
    /// Setup MongoDb support.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <param name="options">The settings.</param>
    /// <param name="seederType">The sender type.</param>
    /// <param name="registerConventions">Enable default mongoDB Conventions. Default value is 'true'.</param>
    /// <returns>The Genocs builder.</returns>
    public static IGenocsBuilder AddMongo(
                                          this IGenocsBuilder builder,
                                          MongoDbOptions options,
                                          Type? seederType = null,
                                          bool registerConventions = true)
    {
        if (!builder.TryRegister(MongoDbOptions.Position))
        {
            Console.WriteLine($"Try to register {nameof(MongoDbOptions)} failed!");
            return builder;
        }

        if (!MongoDbOptions.IsValid(options))
        {
            Console.WriteLine($"MongoDbOptions is not valid! {nameof(options.ConnectionString)} or {nameof(options.Database)} is empty.");
            return builder;
        }

        if (options.SetRandomDatabaseSuffix)
        {
            string suffix = $"{Guid.NewGuid():N}";
            Console.WriteLine($"Setting a random MongoDB database suffix: '{suffix}'.");
            options.Database = $"{options.Database}_{suffix}";
        }

        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<IMongoClient>(sp =>
        {
            var options = sp.GetRequiredService<MongoDbOptions>();

            MongoClientSettings clientSettings = MongoClientSettings.FromConnectionString(options.ConnectionString);

            if (options.EnableTracing)
            {
                clientSettings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());
            }

            return new MongoClient(clientSettings);
        });

        builder.Services.AddTransient(sp =>
        {
            var options = sp.GetRequiredService<MongoDbOptions>();
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(options.Database);
        });

        builder.Services.AddTransient<IMongoDbInitializer, MongoDbInitializer>();
        builder.Services.AddTransient<IMongoSessionFactory, MongoSessionFactory>();

        builder.Services.AddSingleton<IMongoDatabaseProvider, MongoDatabaseProvider>();

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
            _conventionsRegistered = true;
            ServiceCollectionExtensions.RegisterConventions();
        }

        return builder;
    }

    /// <summary>
    /// Adds a MongoDb repository to the DI container. Using Genocs builder support.
    /// </summary>
    /// <typeparam name="TEntity">The name of the entity.</typeparam>
    /// <typeparam name="TKey">The kind of identifier.</typeparam>
    /// <param name="builder">The Genocs builder.</param>
    /// <param name="collectionName">The collection name where to store data.</param>
    /// <returns>The Genocs builder.</returns>
    public static IGenocsBuilder AddMongoRepository<TEntity, TKey>(
                                                                    this IGenocsBuilder builder,
                                                                    string collectionName)
        where TEntity : IEntity<TKey>
    {
        builder.Services.AddTransient<IMongoDbBaseRepository<TEntity, TKey>>(sp =>
        {
            var database = sp.GetRequiredService<IMongoDatabase>();
            return new MongoDbBaseRepository<TEntity, TKey>(database, collectionName);
        });

        return builder;
    }

    /// <summary>
    /// Add MongoDb support along with the default MongoDb repository registration.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <param name="sectionName">The Genocs builder.</param>
    /// <param name="registerConventions">The Genocs builder.</param>
    /// <returns>The Genocs builder.</returns>
    public static IGenocsBuilder AddMongoWithRegistration(
                                              this IGenocsBuilder builder,
                                              string sectionName = MongoDbOptions.Position,
                                              bool registerConventions = true)
    {
        AddMongo(builder, sectionName, null, registerConventions);

        var section = builder.Configuration?.GetSection(sectionName);

        if (!section.Exists())
        {
            return builder;
        }

        builder.Services.Configure<MongoDbOptions>(section);

        builder.Services.AddScoped(typeof(IMongoDbRepository<>), typeof(MongoDbRepository<>));

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