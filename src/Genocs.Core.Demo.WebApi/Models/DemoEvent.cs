using Genocs.Core.Interfaces;

namespace Genocs.Core.Demo.WebApi.Models
{
    public class DemoEvent : IEvent
    {
        public string Name { get; set; }
        public string Address { get; set; }

        public DemoEvent(string name, string address) => (Name, Address) = (name, address);
    }
}
