using Genocs.Persistence.MongoDb.Configurations;
using Genocs.Persistence.MongoDb.Repositories;

namespace Genocs.Persistence.MongoDb.Builders;

internal sealed class MongoDbOptionsBuilder : IMongoDbOptionsBuilder
{
    private readonly MongoDbSettings _options = new();

    public IMongoDbOptionsBuilder WithConnectionString(string connectionString)
    {
        _options.ConnectionString = connectionString;
        return this;
    }

    public IMongoDbOptionsBuilder WithDatabase(string database)
    {
        _options.Database = database;
        return this;
    }

    public IMongoDbOptionsBuilder WithSeed(bool seed)
    {
        _options.Seed = seed;
        return this;
    }

    public MongoDbSettings Build()
        => _options;
}