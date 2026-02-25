using Genocs.Common.CQRS.Commands;

namespace Genocs.Library.Demo.Contracts;

public class DemoCommand : ICommand
{
    public string? Payload { get; set; }
}
