namespace Genocs.Saga;

/// <summary>
/// The saga action that starts the saga.
/// A saga can only have one start action, and it will be the first action executed when the saga is triggered.
/// The saga will be created when the start action is executed,
/// and the saga data will be initialized with the default values.
/// The saga will be completed when the start action is completed,
/// and the saga data will be updated with the values from the start action.
/// The saga will be compensated if any of the actions in the saga fail,
/// and the compensation logic will be executed in reverse order of the actions.
/// </summary>
/// <typeparam name="TMessage">The type of message that the saga start action handles.</typeparam>
public interface ISagaStartAction<in TMessage> : ISagaAction<TMessage>;

