namespace InventoryManager.Core.Entities;

public class SaleItem
{
    public int Id { get; set; }

    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    // Snapshot of the product's cost AT THE TIME OF SALE — not looked up later,
    // because CostPrice on the Product can change and would corrupt historical profit math.
    public decimal CostPrice { get; set; }

    public decimal Subtotal { get; set; }
}