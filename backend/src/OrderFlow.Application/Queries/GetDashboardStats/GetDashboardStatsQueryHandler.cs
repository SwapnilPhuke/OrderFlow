using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Common;
using OrderFlow.Application.DTOs;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Enums;

namespace OrderFlow.Application.Queries.GetDashboardStats;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, ApiResponse<DashboardStatsDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetDashboardStatsQueryHandler> _logger;

    public GetDashboardStatsQueryHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        ICacheService cacheService,
        ILogger<GetDashboardStatsQueryHandler> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<ApiResponse<DashboardStatsDto>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = request.IsAdmin
            ? "dashboard:stats:admin"
            : $"dashboard:stats:{request.UserId}";

        var cached = await _cacheService.GetAsync<DashboardStatsDto>(cacheKey, cancellationToken);
        if (cached != null)
        {
            _logger.LogDebug("Dashboard stats cache hit for key {CacheKey}", cacheKey);
            return ApiResponse<DashboardStatsDto>.Ok(cached);
        }

        int? userId = request.IsAdmin ? null : request.UserId;

        var statusCounts = await _orderRepository.GetOrderCountsByStatusAsync(userId, cancellationToken);
        var totalRevenue = await _orderRepository.GetTotalRevenueAsync(userId, cancellationToken);
        var totalProducts = await _productRepository.GetTotalActiveCountAsync(cancellationToken);
        var lowStockProducts = await _productRepository.GetLowStockCountAsync(10, cancellationToken);

        var totalOrders = statusCounts.Values.Sum();

        var stats = new DashboardStatsDto
        {
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            PendingOrders = statusCounts.GetValueOrDefault(OrderStatus.Pending),
            ProcessingOrders = statusCounts.GetValueOrDefault(OrderStatus.Processing),
            ShippedOrders = statusCounts.GetValueOrDefault(OrderStatus.Shipped),
            DeliveredOrders = statusCounts.GetValueOrDefault(OrderStatus.Delivered),
            CancelledOrders = statusCounts.GetValueOrDefault(OrderStatus.Cancelled),
            TotalProducts = totalProducts,
            LowStockProducts = lowStockProducts
        };

        await _cacheService.SetAsync(cacheKey, stats, TimeSpan.FromMinutes(5), cancellationToken);

        return ApiResponse<DashboardStatsDto>.Ok(stats);
    }
}
