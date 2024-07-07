namespace Genocs.Persistence.Redis.Configurations;

public interface IRedisSettingsBuilder
{
    IRedisSettingsBuilder WithConnectionString(string connectionString);
    IRedisSettingsBuilder WithInstance(string instance);
    RedisSettings Build();
}