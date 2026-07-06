using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context) => _context = context;

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? category = null, string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category.ToLower() == category.ToLower());

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()) ||
                                     p.Description.ToLower().Contains(search.ToLower()));

        var total = await query.CountAsync(cancellationToken);
        var products = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (products, total);
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        product.UpdatedAt = DateTime.UtcNow;
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<bool> DecrementStockAsync(int productId, int quantity, CancellationToken cancellationToken = default)
    {
        var rows = await _context.Products
            .Where(p => p.Id == productId && p.Stock >= quantity)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock - quantity), cancellationToken);
        return rows > 0;
    }

    public async Task<int> GetLowStockCountAsync(int threshold = 10, CancellationToken cancellationToken = default)
        => await _context.Products
            .CountAsync(p => p.IsActive && p.Stock < threshold, cancellationToken);

    public async Task<int> GetTotalActiveCountAsync(CancellationToken cancellationToken = default)
        => await _context.Products.CountAsync(p => p.IsActive, cancellationToken);
}
