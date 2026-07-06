using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Common;
using OrderFlow.Application.DTOs;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Events;

namespace OrderFlow.Application.Commands.PlaceOrder;

public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, ApiResponse<OrderResponseDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPublisher _publisher;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly ILogger<PlaceOrderCommandHandler> _logger;

    public PlaceOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IPublisher publisher,
        ICacheService cacheService,
        IMapper mapper,
        ILogger<PlaceOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _publisher = publisher;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<OrderResponseDto>> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.Items == null || request.Items.Count == 0)
            return ApiResponse<OrderResponseDto>.Fail("Order must contain at least one item.");

        var orderItems = new List<OrderItem>();
        decimal totalAmount = 0;

        foreach (var item in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product == null)
                return ApiResponse<OrderResponseDto>.Fail($"Product with ID {item.ProductId} was not found.");

            if (!product.IsActive)
                return ApiResponse<OrderResponseDto>.Fail($"Product '{product.Name}' is not available.");

            if (product.Stock < item.Quantity)
                return ApiResponse<OrderResponseDto>.Fail($"Insufficient stock for '{product.Name}'. Available: {product.Stock}, Requested: {item.Quantity}.");

            orderItems.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            });

            totalAmount += product.Price * item.Quantity;
        }

        foreach (var item in request.Items)
        {
            await _productRepository.DecrementStockAsync(item.ProductId, item.Quantity, cancellationToken);
        }

        var order = new Order
        {
            UserId = request.UserId,
            TotalAmount = totalAmount,
            Notes = request.Notes,
            OrderItems = orderItems
        };

        var createdOrder = await _orderRepository.CreateAsync(order, cancellationToken);

        await _publisher.Publish(
            new OrderPlacedDomainEvent(createdOrder.Id, createdOrder.UserId, createdOrder.TotalAmount, createdOrder.CreatedAt),
            cancellationToken);

        await _cacheService.RemoveAsync($"dashboard:stats:{request.UserId}");
        await _cacheService.RemoveAsync("dashboard:stats:admin");

        var fetchedOrder = await _orderRepository.GetByIdAsync(createdOrder.Id, cancellationToken);
        var dto = _mapper.Map<OrderResponseDto>(fetchedOrder ?? createdOrder);

        _logger.LogInformation("Order {OrderId} placed by user {UserId} for total {TotalAmount}", createdOrder.Id, request.UserId, totalAmount);

        return ApiResponse<OrderResponseDto>.Ok(dto, "Order placed successfully.");
    }
}
