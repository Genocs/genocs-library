namespace Genocs.Common.CQRS.Commands;

/// <summary>
/// Command dispatcher interface.
/// </summary>
public interface ICommandDispatcher
{
    /// <summary>
    /// SendAsync.
    /// </summary>
    /// <typeparam name="T">The type of command.</typeparam>
    /// <param name="command">The command object.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task.</returns>
    Task SendAsync<T>(T command, CancellationToken cancellationToken = default)
        where T : class, ICommand;

    /// <summary>
    /// SendAsync for result-returning commands.
    /// </summary>
    /// <typeparam name="TCommand">The type of command.</typeparam>
    /// <typeparam name="TResult">The type of result returned by the command.</typeparam>
    /// <param name="command">The command object.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task with result.</returns>
    Task<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class, ICommand<TResult>;
}
