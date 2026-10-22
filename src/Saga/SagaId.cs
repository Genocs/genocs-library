namespace Genocs.Saga;

public readonly struct SagaId(string id)
{
    public string Id { get; } = id;

    public static implicit operator string(SagaId sagaId)
        => sagaId.Id;

    public static implicit operator SagaId(string sagaId)
        => new(sagaId);

    public static SagaId NewSagaId()
        => new(Guid.NewGuid().ToString());

    public override readonly string ToString()
        => Id;
}
