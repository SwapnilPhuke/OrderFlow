using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Common;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Enums;

namespace OrderFlow.Application.Commands.CancelOrder;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, ApiResponse<bool>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CancelOrderCommandHandler> _logger;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        ICacheService cacheService,
        ILogger<CancelOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<ApiResponse<bool>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            return ApiResponse<bool>.Fail($"Order {request.OrderId} not found.");

        if (!request.IsAdmin && order.UserId != request.UserId)
            return ApiResponse<bool>.Fail("You do not have permission to cancel this order.");

        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
            return ApiResponse<bool>.Fail($"Cannot cancel an order with status '{order.Status}'. Only Pending or Processing orders can be cancelled.");

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order, cancellationToken);

        await _cacheService.RemoveAsync($"dashboard:stats:{request.UserId}");
        await _cacheService.RemoveAsync("dashboard:stats:admin");

        _logger.LogInformation("Order {OrderId} cancelled by user {UserId}", request.OrderId, request.UserId);

        return ApiResponse<bool>.Ok(true, "Order cancelled successfully.");
    }
}
