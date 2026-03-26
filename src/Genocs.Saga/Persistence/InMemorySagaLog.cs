using System.Collections.Concurrent;

namespace Genocs.Saga.Persistence;

internal class InMemorySagaLog : ISagaLog
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<ISagaLogData>> _sagaLog;

    public InMemorySagaLog()
        => _sagaLog = new();

    public Task<IEnumerable<ISagaLogData>> ReadAsync(SagaId id, Type type)
    {
        if (_sagaLog.TryGetValue(GetKey(id, type), out ConcurrentQueue<ISagaLogData>? entries))
        {
            return Task.FromResult<IEnumerable<ISagaLogData>>(entries.ToArray());
        }

        return Task.FromResult<IEnumerable<ISagaLogData>>([]);
    }

    public Task WriteAsync(ISagaLogData message)
    {
        ConcurrentQueue<ISagaLogData> entries = _sagaLog.GetOrAdd(GetKey(message.Id, message.Type), _ => new ConcurrentQueue<ISagaLogData>());
        entries.Enqueue(message);
        return Task.CompletedTask;
    }

    private static string GetKey(SagaId id, Type type)
        => $"{id.Id}:{type.AssemblyQualifiedName}";
}
