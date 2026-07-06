using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enums;

namespace OrderFlow.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context) => _context = context;

    public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedAsync(
        int? userId, int page, int pageSize, OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .AsQueryable();

        if (userId.HasValue)
            query = query.Where(o => o.UserId == userId.Value);

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        var total = await query.CountAsync(cancellationToken);
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (orders, total);
    }

    public async Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        order.UpdatedAt = DateTime.UtcNow;
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task<int> GetUserOrderCountAsync(int userId, CancellationToken cancellationToken = default)
        => await _context.Orders.CountAsync(o => o.UserId == userId, cancellationToken);

    public async Task<Dictionary<OrderStatus, int>> GetOrderCountsByStatusAsync(
        int? userId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders.AsQueryable();
        if (userId.HasValue)
            query = query.Where(o => o.UserId == userId.Value);

        return await query
            .GroupBy(o => o.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }

    public async Task<decimal> GetTotalRevenueAsync(int? userId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .Where(o => o.Status == OrderStatus.Delivered);

        if (userId.HasValue)
            query = query.Where(o => o.UserId == userId.Value);

        return await query.SumAsync(o => o.TotalAmount, cancellationToken);
    }
}
