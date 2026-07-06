using MediatR;
using OrderFlow.Application.Common;
using OrderFlow.Application.DTOs;
using OrderFlow.Domain.Enums;

namespace OrderFlow.Application.Commands.UpdateOrderStatus;

public record UpdateOrderStatusCommand(
    int OrderId,
    int UserId,
    OrderStatus NewStatus,
    bool IsAdmin) : IRequest<ApiResponse<OrderResponseDto>>;
