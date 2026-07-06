namespace OrderFlow.Application.Events;

public record OrderPlacedIntegrationEvent(
    int OrderId,
    int UserId,
    decimal TotalAmount,
    DateTime CreatedAt);
