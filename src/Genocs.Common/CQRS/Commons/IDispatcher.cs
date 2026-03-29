using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Events;
using Genocs.Common.CQRS.Queries;

namespace Genocs.Common.CQRS.Commons;

/// <summary>
/// Generic dispatcher interface.
/// </summary>
public interface IDispatcher : ICommandDispatcher, IQueryDispatcher, IEventDispatcher;
