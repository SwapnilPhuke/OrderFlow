using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Events;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Events;

namespace OrderFlow.Application.EventHandlers;

public class OrderPlacedEventHandler : INotificationHandler<OrderPlacedDomainEvent>
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<OrderPlacedEventHandler> _logger;

    public OrderPlacedEventHandler(IEventBus eventBus, ILogger<OrderPlacedEventHandler> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task Handle(OrderPlacedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Domain event received: OrderPlaced — OrderId: {OrderId}, UserId: {UserId}, TotalAmount: {TotalAmount}",
            notification.OrderId, notification.UserId, notification.TotalAmount);

        var integrationEvent = new OrderPlacedIntegrationEvent(
            notification.OrderId,
            notification.UserId,
            notification.TotalAmount,
            notification.CreatedAt);

        await _eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
