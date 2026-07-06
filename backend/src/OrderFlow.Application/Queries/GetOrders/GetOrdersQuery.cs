using MediatR;
using OrderFlow.Application.Common;
using OrderFlow.Application.DTOs;
using OrderFlow.Domain.Enums;

namespace OrderFlow.Application.Queries.GetOrders;

public record GetOrdersQuery(
    int UserId,
    bool IsAdmin,
    int Page = 1,
    int PageSize = 10,
    OrderStatus? Status = null) : IRequest<ApiResponse<PaginatedResult<OrderSummaryDto>>>;
