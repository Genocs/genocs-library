namespace Genocs.Saga;

public interface ISagaMessageIdentity
{
    string MessageId { get; }
}