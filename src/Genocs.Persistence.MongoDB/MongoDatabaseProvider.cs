using Genocs.Persistence.MongoDB.Configurations;
using MongoDB.Driver;

namespace Genocs.Persistence.MongoDB;

/// <summary>
/// The MongoDatabaseProvider.
/// </summary>
public class MongoDatabaseProvider : IMongoDatabaseProvider
{
    /// <summary>
    /// Reference to MongoClient.
    /// </summary>
    public IMongoClient MongoClient { get; private set; }

    /// <summary>
    /// Reference to Database.
    /// </summary>
    public IMongoDatabase Database { get; private set; }

    /// <summary>
    /// Default Constructor.
    /// </summary>
    /// <param name="mongoClient">DI-registered Mongo client.</param>
    /// <param name="options">Mongo database options.</param>
    /// <exception cref="NullReferenceException">This exception happens in case mandatory data is missing.</exception>
    public MongoDatabaseProvider(IMongoClient mongoClient, MongoOptions options)
    {
        ArgumentNullException.ThrowIfNull(mongoClient);
        ArgumentNullException.ThrowIfNull(options);

        if (!MongoOptions.IsValid(options))
        {
            throw new InvalidOperationException($"{nameof(options)} is invalid");
        }

        this.MongoClient = mongoClient;
        this.Database = this.MongoClient.GetDatabase(options.Database);
    }
}
