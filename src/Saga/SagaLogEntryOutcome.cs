namespace Genocs.Saga;

public enum SagaLogEntryOutcome : byte
{
    Completed = 1,
    Failed = 2,
    Compensated = 3,
    CompensationFailed = 4,
}