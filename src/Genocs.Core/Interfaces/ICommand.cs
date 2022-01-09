using System.Threading.Tasks;

namespace Genocs.Core.Interfaces
{
    /// <summary>
    /// Command definition
    /// </summary>
    public interface ICommand : IMessage
    {
    }

    public interface ICommandHandler<T> where T : ICommand
    {
        Task HandleCommand(T @command);
    }
}
