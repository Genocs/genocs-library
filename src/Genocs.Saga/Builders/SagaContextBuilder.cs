using Genocs.Saga.Persistence;

namespace Genocs.Saga.Builders;

internal sealed class SagaContextBuilder : ISagaContextBuilder
{
    private readonly List<ISagaContextMetadata> _metadata;
    private SagaId? _sagaId;
    private string? _originator;

    public SagaContextBuilder()
        => _metadata = [];

    public ISagaContextBuilder WithSagaId(SagaId sagaId)
    {
        _sagaId = sagaId;
        return this;
    }

    public ISagaContextBuilder WithOriginator(string originator)
    {
        _originator = originator;
        return this;
    }

    public ISagaContextBuilder WithMetadata(string key, object value)
    {
        var metadata = new SagaContextMetadata(key, value);
        _metadata.Add(metadata);
        return this;
    }

    public ISagaContextBuilder WithMetadata(ISagaContextMetadata sagaContextMetadata)
    {
        _metadata.Add(sagaContextMetadata);
        return this;
    }

    public ISagaContext Build()
    {
        if (_sagaId is null)
            throw new InvalidOperationException("SagaId must be provided.");

        if (string.IsNullOrEmpty(_originator))
            throw new InvalidOperationException("Originator must be provided.");

        return SagaContext.Create(_sagaId.Value, _originator, _metadata);

    }
}
