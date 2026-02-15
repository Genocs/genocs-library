using Genocs.Common.CQRS.Events;

namespace Genocs.Library.Demo.WebApi.Models;

public record OrderSubmitted(string OrderId, string UserId) : IEvent;
