using Genocs.Common.Cqrs.Events;

namespace Genocs.Library.Demo.Contracts;

public class DemoEvent : IEvent
{
    public string? Name { get; set; }
    public string? Address { get; set; }
}
