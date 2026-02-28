using Genocs.Common.CQRS.Events;

namespace Genocs.Library.Demo.Masstransit.WebApi.Models;

public record OrderSubmitted(string OrderId, string UserId) : IEvent;
