using AutoMapper;
using MediatR;
using OrderFlow.Application.Common;
using OrderFlow.Application.DTOs;
using OrderFlow.Application.Interfaces;

namespace OrderFlow.Application.Queries.GetOrders;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, ApiResponse<PaginatedResult<OrderSummaryDto>>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrdersQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PaginatedResult<OrderSummaryDto>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        int? userId = request.IsAdmin ? null : request.UserId;

        var (orders, totalCount) = await _orderRepository.GetPagedAsync(userId, request.Page, request.PageSize, request.Status, cancellationToken);

        var dtos = _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);

        var result = PaginatedResult<OrderSummaryDto>.Create(dtos, totalCount, request.Page, request.PageSize);

        return ApiResponse<PaginatedResult<OrderSummaryDto>>.Ok(result);
    }
}
