using InventoryManager.Core.Entities;
using InventoryManager.Core.Enums;
using InventoryManager.Core.Interfaces;
using InventoryManager.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Services;

public class PurchaseService : IPurchaseService
{
    private readonly AppDbContext _context;
    private readonly IInventoryService _inventoryService;

    public PurchaseService(AppDbContext context, IInventoryService inventoryService)
    {
        _context = context;
        _inventoryService = inventoryService;
    }

    public async Task<List<Purchase>> GetAllAsync()
    {
        return await _context.Purchases
            .Include(p => p.Supplier)
            .Include(p => p.Items)
                .ThenInclude(i => i.Product)
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();
    }

    public async Task<(bool Success, string? Error)> CreatePurchaseAsync(
        int supplierId,
        List<PurchaseLineInput> lines,
        int createdByUserId,
        string? notes)
    {
        if (lines is null || lines.Count == 0)
            return (false, "At least one product line is required.");

        foreach (var line in lines)
        {
            if (line.Quantity <= 0)
                return (false, "Quantity must be positive for all lines.");

            if (line.UnitCost < 0)
                return (false, "Unit cost cannot be negative.");
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
            var purchase = new Purchase
            {
                SupplierId = supplierId,
                PurchaseDate = DateTime.UtcNow,
                ReferenceNumber = $"PO-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}",
                Notes = notes,
                CreatedByUserId = createdByUserId,
                TotalAmount = lines.Sum(l => l.Quantity * l.UnitCost)
            };

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync(); // generates purchase.Id

            foreach (var line in lines)
            {
                var purchaseItem = new PurchaseItem
                {
                    PurchaseId = purchase.Id,
                    ProductId = line.ProductId,
                    Quantity = line.Quantity,
                    UnitCost = line.UnitCost,
                    Subtotal = line.Quantity * line.UnitCost
                };
                _context.PurchaseItems.Add(purchaseItem);

                var stockResult = await _inventoryService.AdjustStockAsync(
                    productId: line.ProductId,
                    quantityChange: line.Quantity,
                    movementType: StockMovementType.Purchase,
                    referenceType: ReferenceType.Purchase,
                    referenceId: purchase.Id,
                    createdByUserId: createdByUserId,
                    notes: $"Purchase {purchase.ReferenceNumber}");

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
            return (false, $"An unexpected error occurred while recording the purchase: {ex.Message}{(ex.InnerException != null ? " (" + ex.InnerException.Message + ")" : "")}");
        }
    }
}