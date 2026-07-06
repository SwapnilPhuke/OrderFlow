namespace OrderFlow.Infrastructure.Messaging.Events;

public record OrderPlacedIntegrationEvent(
    int     OrderId,
    int     UserId,
    decimal TotalAmount,
    int     ItemCount,
    DateTime CreatedAt);
