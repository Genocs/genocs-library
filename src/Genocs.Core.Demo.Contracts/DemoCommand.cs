using Genocs.Core.Interfaces;

namespace Genocs.Core.Demo.Contracts
{
    public class DemoCommand : ICommand
    {
        public string Payload { get; }

        public DemoCommand(string payload) => Payload = payload;
    }
}
