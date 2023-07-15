using MongoDB.Driver;

namespace Genocs.Persistence.MongoDb.Repositories;

/// <summary>
/// The MongoDb Session Factory
/// </summary>
public interface IMongoSessionFactory
{
    /// <summary>
    /// Create a new session
    /// </summary>
    /// <returns></returns>
    Task<IClientSessionHandle> CreateAsync();
}