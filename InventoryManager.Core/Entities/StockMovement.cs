using InventoryManager.Core.Enums;

namespace InventoryManager.Core.Entities;

public class StockMovement
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public StockMovementType MovementType { get; set; }

    // Signed: positive = stock in, negative = stock out
    public int Quantity { get; set; }

    public int PreviousStock { get; set; }
    public int NewStock { get; set; }

    public ReferenceType ReferenceType { get; set; }
    public int? ReferenceId { get; set; }

    public DateTime MovementDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
}