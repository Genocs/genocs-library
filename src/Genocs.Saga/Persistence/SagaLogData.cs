using Genocs.Saga.Utils;

namespace Genocs.Saga.Persistence;

internal sealed class SagaLogData : ISagaLogData
{
    public string EntryId { get; }
    public SagaId Id { get; }
    public Type Type { get; }
    public long CreatedAt { get; }
    public object Message { get; }
    public string MessageId { get; }
    public SagaLogEntryOutcome Outcome { get; }

    private SagaLogData(string entryId, SagaId sagaId, Type sagaType, long createdAt, object message, string messageId, SagaLogEntryOutcome outcome)
        => (EntryId, Id, Type, CreatedAt, Message, MessageId, Outcome) = (entryId, sagaId, sagaType, createdAt, message, messageId, outcome);

    public static SagaLogData Create(SagaId sagaId, Type sagaType, object message, SagaLogEntryOutcome outcome, string messageId = null)
        => new(Guid.NewGuid().ToString("N"), sagaId, sagaType, DateTimeOffset.Now.GetTimeStamp(), message, messageId, outcome);

    public static SagaLogData CopyOf(ISagaLogData data, SagaLogEntryOutcome? outcome = null)
        => new(data.EntryId, data.Id, data.Type, data.CreatedAt, data.Message, data.MessageId, outcome ?? data.Outcome);
}
