using Genocs.Persistence.MongoDB.Repositories;
using MongoDB.Driver;

namespace Genocs.Persistence.MongoDB.Factories;

/// <summary>
/// The MongoDB Session factory.
/// </summary>
/// <param name="client">The MongoDB client.</param>
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