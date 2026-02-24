namespace Genocs.Common.Cqrs.Commands;

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
}
