using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Common;
using OrderFlow.Application.DTOs;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Enums;

namespace OrderFlow.Application.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, ApiResponse<OrderResponseDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;

    public UpdateOrderStatusCommandHandler(
        IOrderRepository orderRepository,
        ICacheService cacheService,
        IMapper mapper,
        ILogger<UpdateOrderStatusCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<OrderResponseDto>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            return ApiResponse<OrderResponseDto>.Fail($"Order {request.OrderId} not found.");

        if (!request.IsAdmin && order.UserId != request.UserId)
            return ApiResponse<OrderResponseDto>.Fail("You do not have permission to update this order.");

        try { order.UpdateStatus(request.NewStatus); }
        catch (InvalidOperationException ex) { return ApiResponse<OrderResponseDto>.Fail(ex.Message); }

        await _orderRepository.UpdateAsync(order, cancellationToken);

        await _cacheService.RemoveAsync($"dashboard:stats:{request.UserId}");
        await _cacheService.RemoveAsync("dashboard:stats:admin");

        _logger.LogInformation("Order {OrderId} status updated to {NewStatus} by user {UserId}", request.OrderId, request.NewStatus, request.UserId);

        var dto = _mapper.Map<OrderResponseDto>(order);
        return ApiResponse<OrderResponseDto>.Ok(dto, "Order status updated successfully.");
    }
}
