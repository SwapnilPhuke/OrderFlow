using MediatR;
using OrderFlow.Application.Common;
using OrderFlow.Application.DTOs;

namespace OrderFlow.Application.Commands.PlaceOrder;

public record PlaceOrderCommand(
    int UserId,
    List<PlaceOrderItemDto> Items,
    string? Notes) : IRequest<ApiResponse<OrderResponseDto>>;
