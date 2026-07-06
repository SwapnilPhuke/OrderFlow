using AutoMapper;
using MediatR;
using OrderFlow.Application.Common;
using OrderFlow.Application.DTOs;
using OrderFlow.Application.Interfaces;

namespace OrderFlow.Application.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, ApiResponse<OrderResponseDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<OrderResponseDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            return ApiResponse<OrderResponseDto>.Fail($"Order {request.OrderId} not found.");

        if (!request.IsAdmin && order.UserId != request.UserId)
            return ApiResponse<OrderResponseDto>.Fail("You do not have permission to view this order.");

        var dto = _mapper.Map<OrderResponseDto>(order);
        return ApiResponse<OrderResponseDto>.Ok(dto);
    }
}
