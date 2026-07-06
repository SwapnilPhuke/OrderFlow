using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedAsync(int page, int pageSize, string? category = null, string? search = null, CancellationToken cancellationToken = default);
    Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default);
    Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task<bool> DecrementStockAsync(int productId, int quantity, CancellationToken cancellationToken = default);
    Task<int> GetTotalActiveCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetLowStockCountAsync(int threshold = 10, CancellationToken cancellationToken = default);
}
