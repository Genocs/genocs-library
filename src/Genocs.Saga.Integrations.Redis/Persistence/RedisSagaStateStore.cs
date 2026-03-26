using StackExchange.Redis;

namespace Genocs.Saga.Integrations.Redis.Persistence;

internal sealed class RedisSagaStateStore : IRedisSagaStateStore
{
    private readonly IDatabase _database;
    private readonly string? _instanceName;

    public RedisSagaStateStore(IDatabase database, string? instanceName)
        => (_database, _instanceName) = (database, instanceName);

    public async Task<string?> GetStringAsync(string key)
    {
        RedisValue value = await _database.StringGetAsync(GetKey(key));
        return value.HasValue ? value.ToString() : null;
    }

    public async Task<bool> CompareAndSetAsync(string key, string? expectedValue, string newValue)
    {
        RedisKey redisKey = GetKey(key);
        ITransaction transaction = _database.CreateTransaction();

        if (expectedValue is null)
        {
            transaction.AddCondition(Condition.KeyNotExists(redisKey));
        }
        else
        {
            transaction.AddCondition(Condition.StringEqual(redisKey, expectedValue));
        }

        Task<bool> writeTask = transaction.StringSetAsync(redisKey, newValue);

        if (!await transaction.ExecuteAsync())
        {
            return false;
        }

        return await writeTask;
    }

    public Task RemoveAsync(string key)
        => _database.KeyDeleteAsync(GetKey(key));

    private RedisKey GetKey(string key)
        => string.IsNullOrWhiteSpace(_instanceName) ? key : $"{_instanceName}{key}";
}