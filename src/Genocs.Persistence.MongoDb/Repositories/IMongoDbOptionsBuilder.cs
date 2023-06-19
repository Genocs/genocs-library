using Genocs.Persistence.MongoDb.Options;

namespace Genocs.Persistence.MongoDb.Repositories;

/// <summary>
/// The MongoDB Options Builder 
/// </summary>
public interface IMongoDbOptionsBuilder
{
    /// <summary>
    /// Setup the Connection string
    /// </summary>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    IMongoDbOptionsBuilder WithConnectionString(string connectionString);

    /// <summary>
    /// Setup the Database name
    /// </summary>
    /// <param name="database"></param>
    /// <returns></returns>
    IMongoDbOptionsBuilder WithDatabase(string database);

    /// <summary>
    /// Setup the database Seed
    /// </summary>
    /// <param name="seed"></param>
    /// <returns></returns>
    IMongoDbOptionsBuilder WithSeed(bool seed);

    /// <summary>
    /// Get the settings
    /// </summary>
    /// <returns>MongoDbSettings instance</returns>
    MongoDbSettings Build();
}