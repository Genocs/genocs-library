using Genocs.Common.CQRS.Commands;

namespace Genocs.Library.Demo.Contracts;

public class SubmitOrder : ICommand
{
    public Guid Id { get; set; }
    public string? OrderId { get; set; }
    public string UserId { get; set; } = default!;
}
