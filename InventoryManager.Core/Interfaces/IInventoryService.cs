using InventoryManager.Core.Entities;
using InventoryManager.Core.Enums;

namespace InventoryManager.Core.Interfaces;

public interface IInventoryService
{
    Task<(bool Success, string? Error)> AdjustStockAsync(
        int productId,
        int quantityChange,
        StockMovementType movementType,
        ReferenceType referenceType,
        int? referenceId,
        int createdByUserId,
        string? notes = null);

    Task<List<StockMovement>> GetMovementsForProductAsync(int productId);
    Task<List<StockMovement>> GetAllMovementsAsync();
}