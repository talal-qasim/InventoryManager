using InventoryManager.Core.Interfaces;
using InventoryManager.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummary> GetSummaryAsync()
    {
        var activeProducts = await _context.Products
            .Where(p => p.IsActive)
            .ToListAsync();

        var totalProducts = activeProducts.Count;
        var outOfStock = activeProducts.Count(p => p.CurrentStock <= 0);
        var lowStock = activeProducts.Count(p => p.CurrentStock > 0 && p.CurrentStock <= p.ReorderLevel);
        var inventoryValue = activeProducts.Sum(p => p.CostPrice * p.CurrentStock);

        var totalSuppliers = await _context.Suppliers.CountAsync(s => s.IsActive);

        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        var todaysSaleItems = await _context.SaleItems
            .Include(si => si.Sale)
            .Where(si => si.Sale.SaleDate >= todayStart && si.Sale.SaleDate < todayEnd)
            .ToListAsync();

        var todaysSales = todaysSaleItems.Sum(si => si.Subtotal);
        var todaysProfit = todaysSaleItems.Sum(si => (si.UnitPrice - si.CostPrice) * si.Quantity);

        return new DashboardSummary
        {
            TotalProducts = totalProducts,
            LowStockCount = lowStock,
            OutOfStockCount = outOfStock,
            TotalSuppliers = totalSuppliers,
            InventoryValue = inventoryValue,
            TodaysSales = todaysSales,
            TodaysProfit = todaysProfit
        };
    }
}