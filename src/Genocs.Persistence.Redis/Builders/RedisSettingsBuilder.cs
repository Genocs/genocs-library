using Genocs.Persistence.Redis.Configurations;

namespace Genocs.Persistence.Redis.Builders;

/// <summary>
/// The Redis Options builder. This class is used to build the Redis options.
/// </summary>
internal sealed class RedisSettingsBuilder : IRedisOptionsBuilder
{
    private readonly RedisOptions _options = new();

    /// <summary>
    /// Set the connection string.
    /// </summary>
    /// <param name="connectionString">The redis connection string.</param>
    /// <returns>The builder. You can use it to chain commands.</returns>
    public IRedisOptionsBuilder WithConnectionString(string connectionString)
    {
        _options.ConnectionString = connectionString;
        return this;
    }

    /// <summary>
    /// Set the redis instance.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <returns>The builder. You can use it to chain commands.</returns>
    public IRedisOptionsBuilder WithInstance(string instance)
    {
        _options.Instance = instance;
        return this;
    }

    /// <summary>
    /// Build the redis options.
    /// </summary>
    /// <returns>The created RedisOptions.</returns>
    public RedisOptions Build()
        => _options;
}