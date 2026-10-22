namespace Genocs.Saga;

public interface ISagaState
{
    SagaId? Id { get; }
    Type? Type { get; }
    SagaProcessState State { get; }
    object? Data { get; }
    long Version { get; }
    void Update(SagaProcessState state, object? data = null);
    void UpdateVersion(long version);
}
