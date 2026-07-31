namespace InventoryManager.Core.Interfaces;

public interface IExportService
{
    string ToCsv<T>(IEnumerable<T> rows);
}