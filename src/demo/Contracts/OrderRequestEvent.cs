using Genocs.Common.Cqrs.Events;

namespace Genocs.Library.Demo.Contracts;

public class OrderRequest : IEvent
{
    public string OrderId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = default!;
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = default!;

    public List<Product> Basket { get; set; } = default!;
}

public class Product(string sKU, int count, decimal price)
{
    public string SKU { get; private set; } = sKU;
    public int Count { get; private set; } = count;
    public decimal Price { get; private set; } = price;
}
