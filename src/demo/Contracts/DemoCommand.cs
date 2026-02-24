using Genocs.Common.Cqrs.Commands;

namespace Genocs.Library.Demo.Contracts;

public class DemoCommand : ICommand
{
    public string? Payload { get; set; }
}
