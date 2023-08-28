using Genocs.Persistence.Redis.Options;

namespace Genocs.Persistence.Redis.Builders;

internal sealed class RedisOptionsBuilder : IRedisOptionsBuilder
{
    private readonly RedisSettings _options = new();

    public IRedisOptionsBuilder WithConnectionString(string connectionString)
    {
        _options.ConnectionString = connectionString;
        return this;
    }

    public IRedisOptionsBuilder WithInstance(string instance)
    {
        _options.Instance = instance;
        return this;
    }

    public RedisSettings Build()
        => _options;
}