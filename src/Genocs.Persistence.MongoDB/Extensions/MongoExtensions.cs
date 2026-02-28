using System.Reflection;
using Genocs.Common.Domain.Entities;
using Genocs.Core.Builders;
using Genocs.Persistence.MongoDB.Configurations;
using Genocs.Persistence.MongoDB.Domain.Repositories;
using Genocs.Persistence.MongoDB.Factories;
using Genocs.Persistence.MongoDB.Initializers;
using Genocs.Persistence.MongoDB.Repositories;
using Genocs.Persistence.MongoDB.Seeders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;

namespace Genocs.Persistence.MongoDB.Extensions;

/// <summary>
/// The MongoDB Extensions.
/// </summary>
public static class MongoExtensions
{
    // Helpful when dealing with integration testing
    private static bool _conventionsRegistered;

    /// <summary>
    /// It allows to add support for MongoDB.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <param name="sectionName">The section name.</param>
    /// <param name="seederType">The seeder name.</param>
    /// <param name="registerConventions">Defines if setup the MongoDB standard Conventions.</param>
    /// <returns>The Genocs builder.</returns>
    public static IGenocsBuilder AddMongo(
                                          this IGenocsBuilder builder,
                                          string sectionName = MongoOptions.Position,
                                          Type? seederType = null,
                                          bool registerConventions = true)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = MongoOptions.Position;
        }

        var section = builder.Configuration?.GetSection(sectionName);

        if (!section.Exists())
        {
            return builder;
        }

        builder.Services.Configure<MongoOptions>(section);

        var mongoOptions = builder.GetOptions<MongoOptions>(sectionName);
        return builder.AddMongo(mongoOptions, seederType, registerConventions);
    }

    /// <summary>
    /// Setup MongoDB support.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <param name="options">The settings.</param>
    /// <param name="seederType">The sender type.</param>
    /// <param name="registerConventions">Enable default mongoDB Conventions. Default value is 'true'.</param>
    /// <returns>The Genocs builder.</returns>
    public static IGenocsBuilder AddMongo(
                                          this IGenocsBuilder builder,
                                          MongoOptions options,
                                          Type? seederType = null,
                                          bool registerConventions = true)
    {
        if (!builder.TryRegister(MongoOptions.Position))
        {
            Console.WriteLine($"Try to register {nameof(MongoOptions)} failed!");
            return builder;
        }

        if (!MongoOptions.IsValid(options))
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
            var options = sp.GetRequiredService<MongoOptions>();

            MongoClientSettings clientSettings = MongoClientSettings.FromConnectionString(options.ConnectionString);

            if (options.EnableTracing)
            {
                clientSettings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());
            }

            return new MongoClient(clientSettings);
        });

        builder.Services.AddTransient(sp =>
        {
            var options = sp.GetRequiredService<MongoOptions>();
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(options.Database);
        });

        builder.Services.AddTransient<IMongoInitializer, MongoInitializer>();
        builder.Services.AddTransient<IMongoSessionFactory, MongoSessionFactory>();

        builder.Services.AddSingleton<IMongoDatabaseProvider, MongoDatabaseProvider>();

        if (seederType is null)
        {
            builder.Services.AddTransient<IMongoSeeder, MongoSeeder>();
        }
        else
        {
            builder.Services.AddTransient(typeof(IMongoSeeder), seederType);
        }

        builder.AddInitializer<IMongoInitializer>();

        // Setup conventions
        if (registerConventions && !_conventionsRegistered)
        {
            _conventionsRegistered = true;
            ServiceCollectionExtensions.RegisterConventions();
        }

        return builder;
    }

    /// <summary>
    /// Adds a MongoDB repository to the DI container. Using Genocs builder support.
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
        builder.Services.AddTransient<IMongoBaseRepository<TEntity, TKey>>(sp =>
        {
            var database = sp.GetRequiredService<IMongoDatabase>();
            return new MongoBaseRepository<TEntity, TKey>(database, collectionName);
        });

        return builder;
    }

    /// <summary>
    /// Add MongoDB support along with the default MongoDB repository registration.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <param name="sectionName">The Genocs builder.</param>
    /// <param name="registerConventions">The Genocs builder.</param>
    /// <returns>The Genocs builder.</returns>
    public static IGenocsBuilder AddMongoWithRegistration(
                                              this IGenocsBuilder builder,
                                              string sectionName = MongoOptions.Position,
                                              bool registerConventions = true)
    {
        AddMongo(builder, sectionName, null, registerConventions);

        var section = builder.Configuration?.GetSection(sectionName);

        if (!section.Exists())
        {
            return builder;
        }

        builder.Services.Configure<MongoOptions>(section);

        builder.Services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));

        return builder;
    }

    /// <summary>
    /// Register all the default MongoDB repository.
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
            .AddClasses(c => c.AssignableTo(typeof(IMongoRepository<>)))
            .AsImplementedInterfaces()
            .WithLifetime(lifetime));

        return builder;
    }
}