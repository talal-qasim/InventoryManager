using InventoryManager.Core.Entities;

namespace InventoryManager.Core.Interfaces;

public class PurchaseLineInput
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
}

public interface IPurchaseService
{
    Task<List<Purchase>> GetAllAsync();
    Task<(bool Success, string? Error)> CreatePurchaseAsync(
        int supplierId,
        List<PurchaseLineInput> lines,
        int createdByUserId,
        string? notes);
}