namespace Genocs.Saga;

public interface ISagaLogData
{
    string EntryId { get; }
    SagaId Id { get; }
    Type Type { get; }
    long CreatedAt { get; }
    object Message { get; }
    string MessageId { get; }
    SagaLogEntryOutcome Outcome { get; }
}
