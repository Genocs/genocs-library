namespace Genocs.Saga;

internal sealed class SagaExecutionContext : ISagaContext
{
    public SagaId SagaId { get; }
    public string Originator { get; }
    public IReadOnlyCollection<ISagaContextMetadata> Metadata { get; }
    public SagaContextError? SagaContextError { get; set; }

    public SagaExecutionContext(ISagaContext context)
    {
        SagaId = context.SagaId;
        Originator = context.Originator;
        Metadata = context.Metadata.ToList().AsReadOnly();
        SagaContextError = context.SagaContextError;
    }

    public ISagaContextMetadata GetMetadata(string key)
        => Metadata.Single(m => m.Key == key);

    public bool TryGetMetadata(string key, out ISagaContextMetadata? metadata)
    {
        metadata = Metadata.SingleOrDefault(m => m.Key == key);
        return metadata is not null;
    }
}