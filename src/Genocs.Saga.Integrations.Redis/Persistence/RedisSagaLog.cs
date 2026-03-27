using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Genocs.Saga.Integrations.Redis.Persistence;

internal sealed class RedisSagaLog : ISagaLog
{
    private readonly IDistributedCache _cache;

    public RedisSagaLog(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<IEnumerable<ISagaLogData>> ReadAsync(SagaId id, Type sagaType)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new SagaException($"{nameof(id)} was null.");
        }

        if (sagaType is null)
        {
            throw new SagaException($"{nameof(sagaType)} was null.");
        }

        var sagaLogDatas = new List<RedisSagaLogData>();
        var deserializedSagaLogDatas = new List<RedisSagaLogData>();
        var cachedSagaLogDatas = await _cache.GetStringAsync(LogId(id, sagaType));

        if (!string.IsNullOrWhiteSpace(cachedSagaLogDatas))
        {
            sagaLogDatas = JsonConvert.DeserializeObject<List<RedisSagaLogData>>(cachedSagaLogDatas);
            sagaLogDatas.ForEach(sld =>
            {
                {
                    var message = (sld.Message as JObject)?.ToObject(sld.MessageType);
                    deserializedSagaLogDatas.Add(new RedisSagaLogData(sld.EntryId, sld.Id, sld.Type, sld.CreatedAt, message ?? sld.Message, sld.MessageType, sld.MessageId, sld.Outcome));
                }
            });
        }

        return deserializedSagaLogDatas;
    }

    public async Task WriteAsync(ISagaLogData logData)
    {
        if (logData is null)
        {
            throw new SagaException($"{nameof(logData)} was null.");
        }

        var sagaLogDatas = (await ReadAsync(logData.Id, logData.Type)).ToList();

        var sagaLogData = new RedisSagaLogData(logData.EntryId, logData.Id, logData.Type, logData.CreatedAt, logData.Message, logData.Message.GetType(), logData.MessageId, logData.Outcome);

        sagaLogDatas.Add(sagaLogData);

        string serializedSagaLogDatas = JsonConvert.SerializeObject(sagaLogDatas);

        await _cache.SetStringAsync(LogId(logData.Id, logData.Type), serializedSagaLogDatas);
    }

    public async Task UpdateOutcomeAsync(SagaId id, Type sagaType, string entryId, SagaLogEntryOutcome outcome)
    {
        var sagaLogDatas = (await ReadAsync(id, sagaType)).Cast<RedisSagaLogData>().ToList();
        int index = sagaLogDatas.FindIndex(entry => string.Equals(entry.EntryId, entryId, StringComparison.Ordinal));

        if (index < 0)
        {
            throw new SagaException($"Saga log entry '{entryId}' was not found for '{sagaType.FullName}' and id '{id.Id}'.");
        }

        RedisSagaLogData existing = sagaLogDatas[index];
        sagaLogDatas[index] = new RedisSagaLogData(existing.EntryId, existing.Id, existing.Type, existing.CreatedAt, existing.Message, existing.MessageType, existing.MessageId, outcome);

        string serializedSagaLogDatas = JsonConvert.SerializeObject(sagaLogDatas);
        await _cache.SetStringAsync(LogId(id, sagaType), serializedSagaLogDatas);
    }

    public async Task DeleteAsync(SagaId sagaId, Type sagaType)
    {
        if (string.IsNullOrWhiteSpace(sagaId))
        {
            throw new SagaException($"{nameof(sagaId)} was null or whitespace.");
        }

        if (sagaType is null)
        {
            throw new SagaException($"{nameof(sagaType)} was null.");
        }

        await _cache.RemoveAsync(LogId(sagaId, sagaType));
    }

    private string LogId(string id, Type type) => $"_log_{id}_{type.GetHashCode()}";
}
