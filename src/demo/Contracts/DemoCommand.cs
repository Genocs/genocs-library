using Genocs.Core.CQRS.Commands;

namespace Genocs.Core.Demo.Contracts;

public class DemoCommand(string payload) : ICommand
{
    public string Payload { get; } = payload;
}
