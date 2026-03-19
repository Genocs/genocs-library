using Genocs.Core.Demo.Domain.Aggregates;
using Genocs.Library.Demo.Contracts;
using Genocs.Persistence.MongoDB.Domain.Repositories;
using MassTransit;

namespace Genocs.Library.Demo.Masstransit.Worker.Consumers;

public class SubmitOrderConsumer : IConsumer<SubmitOrder>
{
    private readonly ILogger<SubmitOrderConsumer> _logger;

    private readonly IMongoRepository<Order> _orderRepository;

    public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger, IMongoRepository<Order> orderRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    public async Task Consume(ConsumeContext<SubmitOrder> context)
    {
        var order = new Order(context.Message.OrderId, context.Message.UserId, 1, "EUR");
        await _orderRepository.InsertAsync(order);
        _logger.LogInformation($"Order {context.Message.OrderId} processed!");
    }
}