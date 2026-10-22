namespace Genocs.Persistence.Redis.Configurations;

/// <summary>
/// The Redis Options builder. This interface is used to build the Redis options.
/// </summary>
public interface IRedisOptionsBuilder
{
    /// <summary>
    /// Set the connection string.
    /// </summary>
    /// <param name="connectionString">The redis connection string.</param>
    /// <returns>The builder. You can use it to chain commands.</returns>
    IRedisOptionsBuilder WithConnectionString(string connectionString);

    /// <summary>
    /// Set the redis instance.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <returns>The builder. You can use it to chain commands.</returns>
    IRedisOptionsBuilder WithInstance(string instance);

    /// <summary>
    /// Build the redis options.
    /// </summary>
    /// <returns>The created RedisOptions.</returns>
    RedisOptions Build();
}