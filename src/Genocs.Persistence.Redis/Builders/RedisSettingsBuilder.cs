using Genocs.Persistence.Redis.Configurations;

namespace Genocs.Persistence.Redis.Builders;

internal sealed class RedisSettingsBuilder : IRedisSettingsBuilder
{
    private readonly RedisSettings _options = new();

    public IRedisSettingsBuilder WithConnectionString(string connectionString)
    {
        _options.ConnectionString = connectionString;
        return this;
    }

    public IRedisSettingsBuilder WithInstance(string instance)
    {
        _options.Instance = instance;
        return this;
    }

    public RedisSettings Build()
        => _options;
}