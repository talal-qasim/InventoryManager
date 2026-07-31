namespace InventoryManager.Core.Entities;

public class AppSettings
{
    public int Id { get; set; }
    public string BusinessName { get; set; } = "My Business";
    public string CurrencySymbol { get; set; } = "$";
    public int DefaultReorderLevel { get; set; } = 5;
}