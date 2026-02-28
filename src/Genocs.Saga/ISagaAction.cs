namespace Genocs.Saga;

/// <summary>
/// Interface for defining a saga action.
/// A saga action is a step in a saga that can be executed and compensated if necessary.
/// </summary>
/// <typeparam name="TMessage">The type of message that the saga action handles.</typeparam>
public interface ISagaAction<in TMessage>
{
    /// <summary>
    /// The HandleAsync method is responsible for executing the saga action. It takes a message of type TMessage and a saga context as parameters.
    /// The saga context provides information about the current state of the saga and allows the action to access and modify it.
    /// If the action fails, the CompensateAsync method will be called to undo any changes made by the HandleAsync method.
    /// </summary>
    /// <param name="message">The message that triggers the saga action.</param>
    /// <param name="context">The context of the saga, providing access to its state and allowing modifications.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleAsync(TMessage message, ISagaContext context);

    /// <summary>
    /// Asynchronously compensates for the specified message within the context of the saga.
    /// </summary>
    /// <remarks>This method is typically used to revert changes made by a previous operation in the saga.
    /// Ensure that the message and context are valid before invoking this method.</remarks>
    /// <param name="message">The message that triggers the compensation process. Must contain the necessary data for the operation.</param>
    /// <param name="context">The saga context that provides the current state and information required for the compensation operation.</param>
    /// <returns>A task that represents the asynchronous compensation operation. The task completes when the compensation process
    /// is finished.</returns>
    Task CompensateAsync(TMessage message, ISagaContext context);
}
