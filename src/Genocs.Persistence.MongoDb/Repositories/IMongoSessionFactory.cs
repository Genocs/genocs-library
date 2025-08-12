using MongoDB.Driver;

namespace Genocs.Persistence.MongoDb.Repositories;

/// <summary>
/// The MongoDb Session factory.
/// This Session factory allows you to create a new session that in return you can use to handle transactions.
/// </summary>
public interface IMongoSessionFactory
{
    /// <summary>
    /// Create a new session.
    /// </summary>
    /// <returns>The async ClientSessionHandle.</returns>
    Task<IClientSessionHandle> CreateAsync();
}