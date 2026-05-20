using Genocs.Common.CQRS.Commons;

namespace Genocs.Common.CQRS.Commands;

/// <summary>
/// CQRS command interface.
/// </summary>
public interface ICommand : IMessage;

/// <summary>
/// CQRS command interface with result.
/// </summary>
/// <typeparam name="TResult">The type of the result returned by the command.</typeparam>
public interface ICommand<TResult> : ICommand;
