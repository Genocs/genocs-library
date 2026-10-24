namespace Genocs.Saga;

public interface ISagaContextMetadata
{
    string Key { get; }
    object Value { get; }
}
