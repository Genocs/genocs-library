using Genocs.Common.CQRS.Commands;
using Genocs.Messaging;
using Genocs.Messaging.Outbox;
using Genocs.Orders.WebApi.Domain;
using Genocs.Orders.WebApi.Events;
using Genocs.Orders.WebApi.Services;
using Genocs.Persistence.MongoDB.Domain.Repositories;

namespace Genocs.Orders.WebApi.Commands.Handlers;

public class CreateOrderHandler(
                            IMongoBaseRepository<Order, Guid> repository,
                            IBusPublisher publisher,
                            IMessageOutbox outbox,
                            IProductServiceClient productServiceClient,
                            ILogger<CreateOrderHandler> logger) : ICommandHandler<CreateOrder>
{
    private readonly IMongoBaseRepository<Order, Guid> _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IBusPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    private readonly IMessageOutbox _outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
    private readonly IProductServiceClient _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
    private readonly ILogger<CreateOrderHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task HandleAsync(CreateOrder command, CancellationToken cancellationToken = default)
    {
        bool exists = await _repository.ExistsAsync(o => o.Id == command.OrderId, cancellationToken: cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"Order with given id: {command.OrderId} already exists!");
        }

        _logger.LogInformation($"Fetching products for order with id: {command.OrderId}...");

        List<OrderItem> productItems = [];

        foreach (var productId in command.Products)
        {
            var productDto = await _productServiceClient.GetAsync(productId) ?? throw new InvalidOperationException($"Product '{productId}' was not found. Requested for order '{command.OrderId}'");
            productItems.Add(new OrderItem(productId, productDto.UnitPrice, 1));
        }

        var order = new Order(command.OrderId, command.CustomerId, productItems);
        await _repository.AddAsync(order, cancellationToken: cancellationToken);

        _logger.LogInformation($"Created order '{command.OrderId}' for customer '{command.CustomerId}'.");

        string? spanContext = System.Diagnostics.Activity.Current?.Id;
        var @event = new OrderCreated(order.Id);
        if (_outbox.Enabled)
        {
            await _outbox.SendAsync(@event, spanContext: spanContext, cancellationToken: cancellationToken);
            return;
        }

        await _publisher.PublishAsync(@event, spanContext: spanContext, cancellationToken: cancellationToken);
    }
}