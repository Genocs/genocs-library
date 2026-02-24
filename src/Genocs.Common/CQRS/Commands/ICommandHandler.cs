namespace Genocs.Common.Cqrs.Commands;

/// <summary>
/// Cqrs command handler interface.
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
/// Legacy Cqrs command handler interface.
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