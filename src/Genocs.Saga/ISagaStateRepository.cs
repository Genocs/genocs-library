namespace Genocs.Saga;

public interface ISagaStateRepository
{
    Task<ISagaState> ReadAsync(SagaId id, Type type);
    Task WriteAsync(ISagaState state);
}
