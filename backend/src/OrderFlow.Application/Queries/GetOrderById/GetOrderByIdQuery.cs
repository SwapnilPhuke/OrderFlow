using MediatR;
using OrderFlow.Application.Common;
using OrderFlow.Application.DTOs;

namespace OrderFlow.Application.Queries.GetOrderById;

public record GetOrderByIdQuery(int OrderId, int UserId, bool IsAdmin) : IRequest<ApiResponse<OrderResponseDto>>;
