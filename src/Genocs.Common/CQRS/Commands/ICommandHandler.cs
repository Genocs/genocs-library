namespace Genocs.Common.CQRS.Commands;

/// <summary>
/// CQRS command handler interface.
/// </summary>
/// <typeparam name="TCommand">The type of command.</typeparam>
public interface ICommandHandler<in TCommand>
    where TCommand : class, ICommand
{
    /// <summary>
    /// HandleAsync.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">The Cancellation token.</param>
    /// <returns>Async Task.</returns>
    Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// CQRS command handler interface for result-returning commands.
/// </summary>
/// <typeparam name="TCommand">The type of command.</typeparam>
/// <typeparam name="TResult">The type of result returned by the command.</typeparam>
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : class, ICommand<TResult>
{
    /// <summary>
    /// HandleAsync.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">The Cancellation token.</param>
    /// <returns>Async Task with result.</returns>
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Legacy CQRS command handler interface.
/// </summary>
/// <typeparam name="T">The type of command.</typeparam>
public interface ICommandHandlerLegacy<T>
    where T : ICommand
{
    /// <summary>
    /// Legacy HandleAsync.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <returns>Async Task.</returns>
    Task HandleCommand(T @command);
}