namespace InventoryManager.Core.Interfaces;

public class DashboardSummary
{
    public int TotalProducts { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public int TotalSuppliers { get; set; }
    public decimal InventoryValue { get; set; }
    public decimal TodaysSales { get; set; }
    public decimal TodaysProfit { get; set; }
}

public interface IDashboardService
{
    Task<DashboardSummary> GetSummaryAsync();
}