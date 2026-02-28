using Genocs.Common.Interfaces;

namespace Genocs.Orders.WebApi.DTO;

public class OrderDto : IDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
}