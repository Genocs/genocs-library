namespace Genocs.Saga;

/// <summary>
/// The ISaga interface represents a long-running process that can be completed or rejected.
/// It provides methods for managing the state of the saga and resolving its identifier based on incoming messages and context.
/// The ISaga interface is designed to be implemented by classes that represent specific sagas in a distributed system,
/// allowing for coordination of complex workflows across multiple services or components.
/// </summary>
public interface ISaga
{

    /// <summary>
    /// The Id property represents the unique identifier of the saga instance.
    /// It is used to track and manage the state of the saga throughout its lifecycle.
    /// </summary>
    SagaId Id { get; }

    /// <summary>
    /// The State property represents the current state of the saga instance.
    /// It is used to determine the progress and outcome of the saga.
    /// </summary>
    SagaProcessState State { get; }

    /// <summary>
    /// Completes the saga instance, marking it as successfully finished.
    /// </summary>
    void Complete();

    /// <summary>
    /// Asynchronously completes the saga instance, marking it as successfully finished.
    /// </summary>
    Task CompleteAsync();

    /// <summary>
    /// Rejects the saga instance, marking it as failed.
    /// </summary>
    /// <param name="innerException">The exception that caused the rejection, if any.</param>
    void Reject(Exception? innerException = null);

    /// <summary>
    /// Asynchronously rejects the saga instance, marking it as failed.
    /// </summary>
    /// <param name="innerException">The exception that caused the rejection, if any.</param>
    Task RejectAsync(Exception? innerException = null);

    /// <summary>
    /// Initializes the saga instance with the specified identifier and state.
    /// </summary>
    /// <param name="id">The unique identifier of the saga instance.</param>
    /// <param name="state">The initial state of the saga instance.</param>
    void Initialize(SagaId id, SagaProcessState state);

    /// <summary>
    /// Resolves the unique identifier of the saga instance based on the incoming message and context.
    /// </summary>
    /// <param name="message">The incoming message.</param>
    /// <param name="context">The saga context.</param>
    /// <returns>The resolved saga identifier.</returns>
    SagaId ResolveId(object message, ISagaContext context);
}

/// <summary>
/// Defines a contract for a saga that manages state and associated data for a specific process.
/// </summary>
/// <remarks>Implementations of this interface should provide logic for initializing the saga with a unique
/// identifier, its states, and associated data. The Data property exposes the current data for the saga
/// instance.</remarks>
/// <typeparam name="TData">The type of data maintained by the saga. Must be a reference type.</typeparam>
public interface ISaga<TData> : ISaga
    where TData : class
{
    /// <summary>
    /// Gets the primary data associated with this instance.
    /// </summary>
    /// <remarks>Accessing this property before the data has been initialized may result in a null reference.
    /// Ensure that the data is available before use to avoid exceptions.</remarks>
    TData Data { get; }

    /// <summary>
    /// Initializes the saga with the specified identifier, states, and associated data.
    /// </summary>
    /// <remarks>This method must be called before the saga can be processed. Ensure that the provided states
    /// are valid for the given saga identifier.</remarks>
    /// <param name="id">The unique identifier for the saga instance. Used to track the saga's state and progress throughout its
    /// lifecycle.</param>
    /// <param name="state">The current states of the saga, representing its stages and possible transitions.</param>
    /// <param name="data">The data associated with the saga, containing relevant information required for processing.</param>
    void Initialize(SagaId id, SagaProcessState state, TData data);
}
