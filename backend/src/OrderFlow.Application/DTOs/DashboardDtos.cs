namespace OrderFlow.Application.DTOs;

public class DashboardStatsDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingOrders { get; set; }
    public int ProcessingOrders { get; set; }
    public int ShippedOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
}
