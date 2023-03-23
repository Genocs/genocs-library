using Genocs.Core.CQRS.Events;

namespace Genocs.Core.Demo.Contracts;

public class DemoEvent : IEvent
{
    public string Name { get; set; }
    public string Address { get; set; }

    public DemoEvent(string name, string address) => (Name, Address) = (name, address);
}
