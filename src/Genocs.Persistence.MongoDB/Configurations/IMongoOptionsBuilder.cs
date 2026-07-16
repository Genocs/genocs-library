namespace Genocs.Persistence.MongoDB.Configurations;

/// <summary>
/// The MongoDB Options Builder.
/// </summary>
public interface IMongoOptionsBuilder
{
    /// <summary>
    /// Setup the Connection string.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>The current instance of <see cref="IMongoOptionsBuilder"/>.</returns>
    IMongoOptionsBuilder WithConnectionString(string connectionString);

    /// <summary>
    /// Setup the Database name.
    /// </summary>
    /// <param name="database">The database name.</param>
    /// <returns>The current instance of <see cref="IMongoOptionsBuilder"/>.</returns>
    IMongoOptionsBuilder WithDatabase(string database);

    /// <summary>
    /// Setup the database Seed.
    /// </summary>
    /// <param name="seed">Indicates whether to seed the database.</param>
    /// <returns>The current instance of <see cref="IMongoOptionsBuilder"/>.</returns>
    IMongoOptionsBuilder WithSeed(bool seed);

    /// <summary>
    /// Get the settings.
    /// </summary>
    /// <returns>MongoDbSettings instance.</returns>
    MongoOptions Build();
}