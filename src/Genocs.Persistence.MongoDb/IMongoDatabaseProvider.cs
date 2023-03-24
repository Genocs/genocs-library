using MongoDB.Driver;

namespace Genocs.Persistence.MongoDb;

/// <summary>
/// Defines interface to obtain a <see cref="IMongoDatabase"/> object.
/// </summary>
public interface IMongoDatabaseProvider
{
    /// <summary>
    /// Gets the MongoClient
    /// </summary>
    IMongoClient MongoClient { get; }


    /// <summary>
    /// Gets the <see cref="IMongoDatabase"/>.
    /// </summary>
    IMongoDatabase Database { get; }
}
