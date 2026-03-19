using Genocs.Persistence.MongoDB.Configurations;

namespace Genocs.Persistence.MongoDB.Configurations;

/// <summary>
/// The MongoDB Options Builder.
/// </summary>
public interface IMongoOptionsBuilder
{
    /// <summary>
    /// Setup the Connection string.
    /// </summary>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    IMongoOptionsBuilder WithConnectionString(string connectionString);

    /// <summary>
    /// Setup the Database name.
    /// </summary>
    /// <param name="database"></param>
    /// <returns></returns>
    IMongoOptionsBuilder WithDatabase(string database);

    /// <summary>
    /// Setup the database Seed.
    /// </summary>
    /// <param name="seed"></param>
    /// <returns></returns>
    IMongoOptionsBuilder WithSeed(bool seed);

    /// <summary>
    /// Get the settings.
    /// </summary>
    /// <returns>MongoDbSettings instance.</returns>
    MongoOptions Build();
}