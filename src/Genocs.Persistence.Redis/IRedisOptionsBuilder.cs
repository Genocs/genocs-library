using Genocs.Persistence.Redis.Options;

namespace Genocs.Persistence.Redis;

public interface IRedisOptionsBuilder
{
    IRedisOptionsBuilder WithConnectionString(string connectionString);
    IRedisOptionsBuilder WithInstance(string instance);
    RedisSettings Build();
}