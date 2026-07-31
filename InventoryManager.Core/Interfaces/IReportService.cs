using InventoryManager.Core.Entities;

namespace InventoryManager.Core.Interfaces;

public class InventoryReportRow
{
    public string SKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public decimal CostPrice { get; set; }
    public decimal InventoryValue { get; set; }
}

public class SalesReportRow
{
    public DateTime SaleDate { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Revenue { get; set; }
    public decimal Profit { get; set; }
}

public class LowStockReportRow
{
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ReorderLevel { get; set; }
    public string? SupplierName { get; set; }
}

public class DailySalesPoint
{
    public DateTime Date { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class TopProductPoint
{
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
}

public interface IReportService
{
    Task<List<InventoryReportRow>> GetInventoryReportAsync();
    Task<List<SalesReportRow>> GetSalesReportAsync(DateTime? from, DateTime? to);
    Task<List<LowStockReportRow>> GetLowStockReportAsync();
    Task<List<StockMovement>> GetStockMovementReportAsync(DateTime? from, DateTime? to);
    Task<List<DailySalesPoint>> GetLast7DaysSalesAsync();
    Task<List<TopProductPoint>> GetTopSellingProductsAsync(int count);
}