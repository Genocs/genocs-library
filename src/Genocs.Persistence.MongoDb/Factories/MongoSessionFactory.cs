using Genocs.Persistence.MongoDb.Repositories;
using MongoDB.Driver;

namespace Genocs.Persistence.MongoDb.Factories;

internal sealed class MongoSessionFactory(IMongoClient client) : IMongoSessionFactory
{
    private readonly IMongoClient _client = client;

    public Task<IClientSessionHandle> CreateAsync()
        => _client.StartSessionAsync();
}