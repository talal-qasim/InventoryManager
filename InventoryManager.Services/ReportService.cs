using InventoryManager.Core.Entities;
using InventoryManager.Core.Interfaces;
using InventoryManager.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<InventoryReportRow>> GetInventoryReportAsync()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .ToListAsync();

        return products.Select(p => new InventoryReportRow
        {
            SKU = p.SKU,
            ProductName = p.Name,
            CategoryName = p.Category.Name,
            CurrentStock = p.CurrentStock,
            CostPrice = p.CostPrice,
            InventoryValue = p.CostPrice * p.CurrentStock
        }).OrderBy(r => r.ProductName).ToList();
    }

    public async Task<List<SalesReportRow>> GetSalesReportAsync(DateTime? from, DateTime? to)
    {
        var query = _context.SaleItems
            .Include(si => si.Sale)
            .Include(si => si.Product)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(si => si.Sale.SaleDate >= from.Value);

        if (to.HasValue)
            query = query.Where(si => si.Sale.SaleDate <= to.Value);

        var items = await query.ToListAsync();

        return items.Select(si => new SalesReportRow
        {
            SaleDate = si.Sale.SaleDate,
            ProductName = si.Product.Name,
            Quantity = si.Quantity,
            Revenue = si.Subtotal,
            Profit = (si.UnitPrice - si.CostPrice) * si.Quantity
        }).OrderByDescending(r => r.SaleDate).ToList();
    }

    public async Task<List<LowStockReportRow>> GetLowStockReportAsync()
    {
        var products = await _context.Products
            .Include(p => p.Supplier)
            .Where(p => p.IsActive && p.CurrentStock <= p.ReorderLevel)
            .ToListAsync();

        return products.Select(p => new LowStockReportRow
        {
            ProductName = p.Name,
            CurrentStock = p.CurrentStock,
            ReorderLevel = p.ReorderLevel,
            SupplierName = p.Supplier != null ? p.Supplier.Name : null
        }).OrderBy(r => r.CurrentStock).ToList();
    }

    public async Task<List<StockMovement>> GetStockMovementReportAsync(DateTime? from, DateTime? to)
    {
        var query = _context.StockMovements
            .Include(m => m.Product)
            .Include(m => m.CreatedByUser)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(m => m.MovementDate >= from.Value);

        if (to.HasValue)
            query = query.Where(m => m.MovementDate <= to.Value);

        return await query.OrderByDescending(m => m.MovementDate).ToListAsync();
    }
    public async Task<List<DailySalesPoint>> GetLast7DaysSalesAsync()
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-6);

        var saleItems = await _context.SaleItems
            .Include(si => si.Sale)
            .Where(si => si.Sale.SaleDate >= startDate)
            .ToListAsync();

        var points = new List<DailySalesPoint>();
        for (var day = startDate; day <= DateTime.UtcNow.Date; day = day.AddDays(1))
        {
            var dayTotal = saleItems
                .Where(si => si.Sale.SaleDate.Date == day)
                .Sum(si => si.Subtotal);

            points.Add(new DailySalesPoint { Date = day, TotalRevenue = dayTotal });
        }

        return points;
    }

    public async Task<List<TopProductPoint>> GetTopSellingProductsAsync(int count)
    {
        var saleItems = await _context.SaleItems
            .Include(si => si.Product)
            .ToListAsync();

        return saleItems
            .GroupBy(si => si.Product.Name)
            .Select(g => new TopProductPoint
            {
                ProductName = g.Key,
                QuantitySold = g.Sum(si => si.Quantity)
            })
            .OrderByDescending(p => p.QuantitySold)
            .Take(count)
            .ToList();
    }
}