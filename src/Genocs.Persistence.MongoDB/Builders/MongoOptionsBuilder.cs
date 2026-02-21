using Genocs.Persistence.MongoDB.Configurations;
using Genocs.Persistence.MongoDB.Configurations;

namespace Genocs.Persistence.MongoDB.Builders;

internal sealed class MongoOptionsBuilder : IMongoOptionsBuilder
{
    private readonly MongoOptions _options = new();

    public IMongoOptionsBuilder WithConnectionString(string connectionString)
    {
        _options.ConnectionString = connectionString;
        return this;
    }

    public IMongoOptionsBuilder WithDatabase(string database)
    {
        _options.Database = database;
        return this;
    }

    public IMongoOptionsBuilder WithSeed(bool seed)
    {
        _options.Seed = seed;
        return this;
    }

    public MongoOptions Build()
        => _options;
}