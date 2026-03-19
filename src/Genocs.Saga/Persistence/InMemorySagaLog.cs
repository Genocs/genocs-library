namespace Genocs.Saga.Persistence;

internal class InMemorySagaLog : ISagaLog
{
    private readonly List<ISagaLogData> _sagaLog;

    public InMemorySagaLog()
        => _sagaLog = [];

    public Task<IEnumerable<ISagaLogData>> ReadAsync(SagaId id, Type type)
        => Task.FromResult(_sagaLog.Where(sld => sld.Id == id && sld.Type == type));

    public async Task WriteAsync(ISagaLogData message)
    {
        _sagaLog.Add(message);
        await Task.CompletedTask;
    }
}
