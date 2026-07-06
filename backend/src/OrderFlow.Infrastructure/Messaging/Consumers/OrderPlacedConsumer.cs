using MassTransit;
using Microsoft.Extensions.Logging;
using OrderFlow.Infrastructure.Messaging.Events;

namespace OrderFlow.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes OrderPlacedIntegrationEvent messages from RabbitMQ.
/// Simulates sending an order-confirmation email to the customer.
/// In production this would call an email service (SendGrid / SES).
/// </summary>
public class OrderPlacedConsumer : IConsumer<OrderPlacedIntegrationEvent>
{
    private readonly ILogger<OrderPlacedConsumer> _logger;

    public OrderPlacedConsumer(ILogger<OrderPlacedConsumer> logger) => _logger = logger;

    public Task Consume(ConsumeContext<OrderPlacedIntegrationEvent> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "[OrderPlacedConsumer] Processing order confirmation — " +
            "OrderId: {OrderId}, UserId: {UserId}, Total: {Total:C}, Items: {Items}. " +
            "[Simulated] Order confirmation email sent to user {UserId}.",
            msg.OrderId, msg.UserId, msg.TotalAmount, msg.ItemCount, msg.UserId);

        return Task.CompletedTask;
    }
}
