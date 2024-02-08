using System.Threading;
using System.Threading.Tasks;

namespace Genocs.Core.CQRS.Commands;

/// <summary>
/// Command dispatcher interface
/// </summary>
public interface ICommandDispatcher
{
    /// <summary>
    /// SendAsync
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendAsync<T>(T command, CancellationToken cancellationToken = default) where T : class, ICommand;
}
