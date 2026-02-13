using Genocs.Common.CQRS.Commands;

namespace Genocs.Library.Demo.Contracts;

public record SubmitOrder(Guid Id, string OrderId, string UserId) : ICommand;
