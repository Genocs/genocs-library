using Genocs.Core.CQRS.Commands;
using Genocs.MessageBrokers;
using Genocs.MessageBrokers.Outbox;
using Genocs.Orders.WebApi.Domain;
using Genocs.Orders.WebApi.Events;
using Genocs.Orders.WebApi.Services;
using Genocs.Persistence.MongoDb.Domain.Repositories;

namespace Genocs.Orders.WebApi.Commands.Handlers;

public class CreateOrderHandler : ICommandHandler<CreateOrder>
{
    private readonly IMongoDbBaseRepository<Order, Guid> _repository;
    private readonly IBusPublisher _publisher;
    private readonly IMessageOutbox _outbox;
    private readonly IProductServiceClient _productServiceClient;
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(
                                IMongoDbBaseRepository<Order, Guid> repository,
                                IBusPublisher publisher,
                                IMessageOutbox outbox,
                                IProductServiceClient productServiceClient,
                                ILogger<CreateOrderHandler> logger)
    {
        _repository = repository;
        _publisher = publisher;
        _outbox = outbox;
        _productServiceClient = productServiceClient;
        _logger = logger;
    }

    public async Task HandleAsync(CreateOrder command, CancellationToken cancellationToken = default)
    {
        bool exists = await _repository.ExistsAsync(o => o.Id == command.OrderId);
        if (exists)
        {
            throw new InvalidOperationException($"Order with given id: {command.OrderId} already exists!");
        }

        _logger.LogInformation($"Fetching the product for order with id: {command.OrderId}...");
        var productDto = await _productServiceClient.GetAsync(command.ProductId) ?? throw new InvalidOperationException($"Product '{command.ProductId}' was not found. Requested for order '{command.OrderId}'");
        _logger.LogInformation($"Order '{command.OrderId}' will cost '{productDto.UnitPrice}'$.");

        var order = new Order(command.OrderId, command.CustomerId, productDto.UnitPrice);
        await _repository.AddAsync(order);

        _logger.LogInformation($"Created order '{command.OrderId}' for customer '{command.CustomerId}'.");

        string? spanContext = $"Genocs: CreateOrder-{command.OrderId}";
        var @event = new OrderCreated(order.Id);
        if (_outbox.Enabled)
        {
            await _outbox.SendAsync(@event, spanContext: spanContext);
            return;
        }

        await _publisher.PublishAsync(@event, spanContext: spanContext);
    }
}