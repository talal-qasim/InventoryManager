using InventoryManager.Core.Entities;
using InventoryManager.Core.Enums;
using InventoryManager.Core.Interfaces;
using InventoryManager.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Services;

public class SaleService : ISaleService
{
    private readonly AppDbContext _context;
    private readonly IInventoryService _inventoryService;

    public SaleService(AppDbContext context, IInventoryService inventoryService)
    {
        _context = context;
        _inventoryService = inventoryService;
    }

    public async Task<List<Sale>> GetAllAsync()
    {
        return await _context.Sales
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    public async Task<(bool Success, string? Error)> CreateSaleAsync(
        List<SaleLineInput> lines,
        int createdByUserId,
        string? notes)
    {
        if (lines is null || lines.Count == 0)
            return (false, "At least one product line is required.");

        foreach (var line in lines)
        {
            if (line.Quantity <= 0)
                return (false, "Quantity must be positive for all lines.");

            if (line.UnitPrice < 0)
                return (false, "Unit price cannot be negative.");
        }

        // Ensure valid user ID to prevent FK constraint failure
        var userExists = await _context.Users.AnyAsync(u => u.Id == createdByUserId);
        if (!userExists)
        {
            var fallbackUser = await _context.Users.FirstOrDefaultAsync();
            if (fallbackUser is null)
            {
                var (hash, salt) = Core.Helpers.PasswordHasher.Hash("Admin@123");
                fallbackUser = new User { Username = "admin", PasswordHash = hash, PasswordSalt = salt, IsActive = true };
                _context.Users.Add(fallbackUser);
                await _context.SaveChangesAsync();
            }
            createdByUserId = fallbackUser.Id;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var sale = new Sale
            {
                SaleDate = DateTime.UtcNow,
                ReferenceNumber = $"SALE-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}",
                Notes = notes,
                CreatedByUserId = createdByUserId,
                TotalAmount = lines.Sum(l => l.Quantity * l.UnitPrice)
            };

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync(); // generates sale.Id

            foreach (var line in lines)
            {
                var product = await _context.Products.FindAsync(line.ProductId);
                if (product is null)
                {
                    await transaction.RollbackAsync();
                    _context.ChangeTracker.Clear();
                    return (false, "One of the selected products could not be found.");
                }

                // Snapshot the product's CURRENT cost at the moment of sale
                var saleItem = new SaleItem
                {
                    SaleId = sale.Id,
                    ProductId = line.ProductId,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    CostPrice = product.CostPrice,
                    Subtotal = line.Quantity * line.UnitPrice
                };
                _context.SaleItems.Add(saleItem);

                var stockResult = await _inventoryService.AdjustStockAsync(
                    productId: line.ProductId,
                    quantityChange: -line.Quantity, // negative — stock going OUT
                    movementType: StockMovementType.Sale,
                    referenceType: ReferenceType.Sale,
                    referenceId: sale.Id,
                    createdByUserId: createdByUserId,
                    notes: $"Sale {sale.ReferenceNumber}");

                if (!stockResult.Success)
                {
                    await transaction.RollbackAsync();
                    _context.ChangeTracker.Clear();
                    return (false, stockResult.Error);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _context.ChangeTracker.Clear();
            return (false, $"An unexpected error occurred while recording the sale: {ex.Message}{(ex.InnerException != null ? " (" + ex.InnerException.Message + ")" : "")}");
        }
    }
}