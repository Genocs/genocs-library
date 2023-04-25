using Genocs.Core.CQRS.Queries;
using Genocs.Orders.WebApi.DTO;
using System;

namespace Genocs.Orders.WebApi.Queries;

public class GetOrder : IQuery<OrderDto>
{
    public Guid OrderId { get; set; }
}