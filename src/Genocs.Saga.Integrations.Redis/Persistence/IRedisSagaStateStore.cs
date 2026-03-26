namespace Genocs.Saga.Integrations.Redis.Persistence;

internal interface IRedisSagaStateStore
{
    Task<string?> GetStringAsync(string key);
    Task<bool> CompareAndSetAsync(string key, string? expectedValue, string newValue);
    Task RemoveAsync(string key);
}