using Genocs.Core.Interfaces;

namespace Genocs.Core.Demo.WebApi.Models
{
    public class DemoCommand : ICommand
    {
        public string Payload { get; }
        public DemoCommand(string payload) => Payload = payload;
    }
}
