using Genocs.Common.CQRS.Commands;

namespace Genocs.Library.Demo.Contracts;

public class DemoCommand(string payload) : ICommand
{
    public string Payload { get; } = payload;
}
