using Genocs.Common.Interfaces;

namespace Genocs.Orders.WebApi.DTO;

public class ProductDto : IDto
{
    public Guid ProductId { get; set; }
    public decimal UnitPrice { get; set; }
}