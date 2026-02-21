namespace Genocs.Saga;

/// <summary>
/// This enum represents the state of a saga process.
/// It can be used to track the progress of a saga and determine whether it has completed successfully, failed, or is still pending.
/// </summary>
public enum SagaProcessState : byte
{
    Pending = 0,
    Completed = 1,
    Rejected = 2,
}
