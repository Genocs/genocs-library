using Genocs.Core.Demo.Contracts;
using Genocs.Core.Demo.Domain.Aggregates;
using Genocs.Persistence.MongoDb.Repositories;
using MassTransit;

namespace Genocs.Core.Demo.Worker.Consumers;

public class SubmitOrderConsumer : IConsumer<SubmitOrder>
{
    private readonly ILogger<SubmitOrderConsumer> _logger;

    private readonly IMongoDbRepository<Order> _orderRepository;

    public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger, IMongoDbRepository<Order> orderRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    public async Task Consume(ConsumeContext<SubmitOrder> context)
    {
        Order order = new Order(context.Message.OrderId, context.Message.UserId, 1, "EUR");
        await _orderRepository.InsertAsync(order);
        _logger.LogInformation($"Order {context.Message.OrderId} processed!");
    }
}