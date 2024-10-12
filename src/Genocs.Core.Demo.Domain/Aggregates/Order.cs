using Genocs.Core.Domain.Entities;
using Genocs.Core.Domain.Entities.Auditing;
using Genocs.Core.Domain.Repositories;
using Genocs.Persistence.MongoDb.Repositories.Clean;
using MongoDB.Bson;

namespace Genocs.Core.Demo.Domain.Aggregates;

[TableMapping("Orders")]
public class Order : AggregateRoot<ObjectId>, IMongoDbEntity, IHasCreationTime
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
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = default!;
}
