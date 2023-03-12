namespace Genocs.Core.CQRS.Commands
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// CQRS command handler interface
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public interface ICommandHandler<in TCommand> where TCommand : class, ICommand
    {
        /// <summary>
        /// HandleAsync
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
    }
}