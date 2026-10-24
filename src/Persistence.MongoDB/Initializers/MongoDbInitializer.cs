using System.Collections.Concurrent;
using Genocs.Persistence.MongoDB.Configurations;
using Genocs.Persistence.MongoDB.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Genocs.Persistence.MongoDB.Initializers;

/// <summary>
/// The MongoDbInitializer implementation.
/// </summary>
/// <param name="database">The mongoDb database reference.</param>
/// <param name="seeder">The database seeder. The seeder is useful to setup database with custom constraint at setup stage.</param>
/// <param name="options">The mongoDb option instance.</param>
/// <param name="logger">Logger used for initializer diagnostics.</param>
internal sealed class MongoInitializer(IMongoDatabase database, IMongoSeeder seeder, MongoOptions options, ILogger<MongoInitializer> logger)
    : IMongoInitializer
{
    private static readonly ConcurrentDictionary<string, byte> InitializedByDatabase = new(StringComparer.Ordinal);

    private readonly string _databaseName = database.DatabaseNamespace.DatabaseName;
    private readonly string _seedKey = CreateSeedKey(options, database);
    private readonly bool _seed = options.Seed;
    private readonly IMongoDatabase _database = database;
    private readonly IMongoSeeder _seeder = seeder;
    private readonly ILogger<MongoInitializer> _logger = logger;

    internal static void ResetInitializationStateForTests()
    {
        InitializedByDatabase.Clear();
    }

    /// <summary>
    /// Initialize the database.
    /// </summary>
    /// <returns>The Task.</returns>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (!_seed)
        {
            _logger.LogInformation("Mongo seeding is disabled for database {DatabaseName}.", _databaseName);
            return;
        }

        if (!InitializedByDatabase.TryAdd(_seedKey, 1))
        {
            _logger.LogInformation("Mongo seeding skipped for database {DatabaseName} because it was already initialized in this process.", _databaseName);
            return;
        }

        _logger.LogInformation("Mongo seeding started for database {DatabaseName}.", _databaseName);

        try
        {
            await _seeder.SeedAsync(_database, cancellationToken);
            _logger.LogInformation("Mongo seeding completed for database {DatabaseName}.", _databaseName);
        }
        catch (Exception ex)
        {
            InitializedByDatabase.TryRemove(_seedKey, out _);
            _logger.LogError(ex, "Mongo seeding failed for database {DatabaseName}; initialization guard was cleared to allow retry.", _databaseName);
            throw;
        }
    }

    private static string CreateSeedKey(MongoOptions options, IMongoDatabase database)
    {
        return $"{options.ConnectionString}|{database.DatabaseNamespace.DatabaseName}";
    }
}