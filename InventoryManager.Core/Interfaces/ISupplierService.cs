using InventoryManager.Core.Entities;

namespace InventoryManager.Core.Interfaces;

public interface ISupplierService
{
    Task<List<Supplier>> GetAllAsync(bool includeInactive = false);
    Task<Supplier?> GetByIdAsync(int id);
    Task<(bool Success, string? Error)> AddAsync(Supplier supplier);
    Task<(bool Success, string? Error)> UpdateAsync(Supplier supplier);
    Task<(bool Success, string? Error)> DeactivateAsync(int id);
}