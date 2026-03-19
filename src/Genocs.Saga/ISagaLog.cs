namespace Genocs.Saga;

public interface ISagaLog
{
    Task<IEnumerable<ISagaLogData>> ReadAsync(SagaId id, Type type);
    Task WriteAsync(ISagaLogData message);
}
