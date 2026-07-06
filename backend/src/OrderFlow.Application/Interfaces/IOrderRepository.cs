using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enums;

namespace OrderFlow.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedAsync(int? userId, int page, int pageSize, OrderStatus? status = null, CancellationToken cancellationToken = default);
    Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task<int> GetUserOrderCountAsync(int userId, CancellationToken cancellationToken = default);
    Task<Dictionary<OrderStatus, int>> GetOrderCountsByStatusAsync(int? userId = null, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRevenueAsync(int? userId = null, CancellationToken cancellationToken = default);
}
