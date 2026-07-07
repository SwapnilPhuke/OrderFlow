using OrderFlow.Domain.Enums;

namespace OrderFlow.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    // ── Domain behaviour ────────────────────────────────────────────────────

    private static readonly IReadOnlyDictionary<OrderStatus, HashSet<OrderStatus>> ValidTransitions =
        new Dictionary<OrderStatus, HashSet<OrderStatus>>
        {
            [OrderStatus.Pending]    = [OrderStatus.Processing, OrderStatus.Cancelled],
            [OrderStatus.Processing] = [OrderStatus.Shipped,    OrderStatus.Cancelled],
            [OrderStatus.Shipped]    = [OrderStatus.Delivered],
            [OrderStatus.Delivered]  = [],
            [OrderStatus.Cancelled]  = []
        };

    /// <summary>
    /// Transitions the order to <paramref name="newStatus"/>.
    /// Throws <see cref="InvalidOperationException"/> when the transition is not allowed.
    /// </summary>
    public void UpdateStatus(OrderStatus newStatus)
    {
        if (!ValidTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
            throw new InvalidOperationException(
                $"Cannot transition order from '{Status}' to '{newStatus}'.");

        Status    = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels the order. Only <c>Pending</c> and <c>Processing</c> orders may be cancelled.
    /// Throws <see cref="InvalidOperationException"/> otherwise.
    /// </summary>
    public void Cancel()
    {
        if (Status is not (OrderStatus.Pending or OrderStatus.Processing))
            throw new InvalidOperationException(
                $"Cannot cancel an order with status '{Status}'. Only Pending or Processing orders can be cancelled.");

        Status    = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
