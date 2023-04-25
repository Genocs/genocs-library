using Genocs.Core.CQRS.Events;

namespace Genocs.Products.WebApi.Events;

public class ProductCreated : IEvent
{
    public Guid ProductId { get; }

    public ProductCreated(Guid productId)
    {
        ProductId = productId;
    }
}