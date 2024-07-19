using Genocs.Persistence.Redis.Configurations;

namespace Genocs.Persistence.Redis.Builders;

internal sealed class RedisSettingsBuilder : IRedisOptionsBuilder
{
    private readonly RedisOptions _options = new();

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

    public RedisOptions Build()
        => _options;
}