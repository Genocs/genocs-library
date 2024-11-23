using Genocs.Core.Domain.Repositories;
using MongoDB.Bson;

namespace Genocs.Core.Demo.Domain.Aggregates;

[TableMapping("Orders")]
public class Order : BaseAggregate
{
    public Order(string orderId, string userId, decimal amount, string currency)
    {
        OrderId = orderId;
        UserId = userId;
        Amount = amount;
        Currency = currency;
    }

    public string OrderId { get; set; } = ObjectId.GenerateNewId().ToString();
    public string UserId { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = default!;
}
