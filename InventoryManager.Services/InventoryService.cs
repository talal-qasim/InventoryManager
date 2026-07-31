using InventoryManager.Core.Entities;
using InventoryManager.Core.Enums;
using InventoryManager.Core.Interfaces;
using InventoryManager.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Services;

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _context;

    public InventoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string? Error)> AdjustStockAsync(
        int productId,
        int quantityChange,
        StockMovementType movementType,
        ReferenceType referenceType,
        int? referenceId,
        int createdByUserId,
        string? notes = null)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product is null)
            return (false, "Product not found.");

        var previousStock = product.CurrentStock;
        var newStock = previousStock + quantityChange;

        if (newStock < 0)
        {
            return (false,
                $"Insufficient stock. Available: {previousStock}, Requested: {-quantityChange}.");
        }

        product.CurrentStock = newStock;
        product.UpdatedAt = DateTime.UtcNow;

        var movement = new StockMovement
        {
            ProductId = productId,
            MovementType = movementType,
            Quantity = quantityChange,
            PreviousStock = previousStock,
            NewStock = newStock,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            MovementDate = DateTime.UtcNow,
            Notes = notes,
            CreatedByUserId = createdByUserId
        };

        _context.StockMovements.Add(movement);

        // Both the Product update and the new StockMovement row are saved
        // together in one SaveChangesAsync call — EF Core wraps this in a
        // single implicit transaction, so either both succeed or neither does.
        await _context.SaveChangesAsync();

        return (true, null);
    }

    public async Task<List<StockMovement>> GetMovementsForProductAsync(int productId)
    {
        return await _context.StockMovements
            .Where(m => m.ProductId == productId)
            .OrderByDescending(m => m.MovementDate)
            .ToListAsync();
    }

    public async Task<List<StockMovement>> GetAllMovementsAsync()
    {
        return await _context.StockMovements
            .Include(m => m.Product)
            .Include(m => m.CreatedByUser)
            .OrderByDescending(m => m.MovementDate)
            .ToListAsync();
    }
}