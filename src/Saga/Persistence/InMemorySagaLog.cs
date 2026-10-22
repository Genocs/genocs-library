using System.Collections.Concurrent;

namespace Genocs.Saga.Persistence;

internal class InMemorySagaLog : ISagaLog
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, SagaLogData>> _sagaLog;

    public InMemorySagaLog()
        => _sagaLog = new();

    public Task<IEnumerable<ISagaLogData>> ReadAsync(SagaId id, Type type)
    {
        if (_sagaLog.TryGetValue(GetKey(id, type), out ConcurrentDictionary<string, SagaLogData>? entries))
        {
            return Task.FromResult<IEnumerable<ISagaLogData>>(entries.Values.OrderBy(e => e.CreatedAt).Cast<ISagaLogData>().ToArray());
        }

        return Task.FromResult<IEnumerable<ISagaLogData>>([]);
    }

    public Task WriteAsync(ISagaLogData message)
    {
        ConcurrentDictionary<string, SagaLogData> entries = _sagaLog.GetOrAdd(GetKey(message.Id, message.Type), _ => new ConcurrentDictionary<string, SagaLogData>());
        SagaLogData entry = SagaLogData.CopyOf(message);

        if (!entries.TryAdd(entry.EntryId, entry))
        {
            throw new SagaException($"Saga log entry '{entry.EntryId}' already exists for '{message.Type.FullName}' and id '{message.Id.Id}'.");
        }

        return Task.CompletedTask;
    }

    public Task UpdateOutcomeAsync(SagaId id, Type type, string entryId, SagaLogEntryOutcome outcome)
    {
        if (!_sagaLog.TryGetValue(GetKey(id, type), out ConcurrentDictionary<string, SagaLogData>? entries)
            || !entries.TryGetValue(entryId, out SagaLogData? current))
        {
            throw new SagaException($"Saga log entry '{entryId}' was not found for '{type.FullName}' and id '{id.Id}'.");
        }

        SagaLogData updated = SagaLogData.CopyOf(current, outcome);
        entries[entryId] = updated;
        return Task.CompletedTask;
    }

    private static string GetKey(SagaId id, Type type)
        => $"{id.Id}:{type.AssemblyQualifiedName}";
}
