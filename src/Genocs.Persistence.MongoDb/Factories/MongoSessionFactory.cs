using Genocs.Persistence.MongoDb.Repositories;
using MongoDB.Driver;

namespace Genocs.Persistence.MongoDb.Factories;

/// <summary>
/// The MongoDb Session factory.
/// </summary>
/// <param name="client"></param>
internal sealed class MongoSessionFactory(IMongoClient client) : IMongoSessionFactory
{
    private readonly IMongoClient _client = client;

    /// <summary>
    /// Create a new session.
    /// </summary>
    /// <returns>The mongoDb client session instance.</returns>
    public Task<IClientSessionHandle> CreateAsync()
        => _client.StartSessionAsync();
}