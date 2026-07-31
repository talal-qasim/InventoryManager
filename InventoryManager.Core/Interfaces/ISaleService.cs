using InventoryManager.Core.Entities;

namespace InventoryManager.Core.Interfaces;

public class SaleLineInput
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public interface ISaleService
{
    Task<List<Sale>> GetAllAsync();
    Task<(bool Success, string? Error)> CreateSaleAsync(
        List<SaleLineInput> lines,
        int createdByUserId,
        string? notes);
}