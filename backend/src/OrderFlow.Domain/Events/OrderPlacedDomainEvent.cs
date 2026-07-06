using MediatR;

namespace OrderFlow.Domain.Events;

public record OrderPlacedDomainEvent(
    int OrderId,
    int UserId,
    decimal TotalAmount,
    DateTime CreatedAt) : INotification;
