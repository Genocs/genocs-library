using Genocs.Saga.Utils;

namespace Genocs.Saga.Persistence;

internal class SagaLogData : ISagaLogData
{
    public SagaId Id { get; }
    public Type Type { get; }
    public long CreatedAt { get; }
    public object Message { get; }
    public SagaLogEntryOutcome Outcome { get; }

    private SagaLogData(SagaId sagaId, Type sagaType, long createdAt, object message, SagaLogEntryOutcome outcome)
        => (Id, Type, CreatedAt, Message, Outcome) = (sagaId, sagaType, createdAt, message, outcome);

    public static ISagaLogData Create(SagaId sagaId, Type sagaType, object message, SagaLogEntryOutcome outcome)
        => new SagaLogData(sagaId, sagaType, DateTimeOffset.Now.GetTimeStamp(), message, outcome);
}
