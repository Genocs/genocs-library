namespace Genocs.Saga;

public interface ISagaLogData
{
    SagaId Id { get; }
    Type Type { get; }
    long CreatedAt { get; }
    object Message { get; }
}
