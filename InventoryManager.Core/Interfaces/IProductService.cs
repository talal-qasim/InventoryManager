using InventoryManager.Core.Entities;

namespace InventoryManager.Core.Interfaces;

public interface IProductService
{
    Task<List<Product>> GetAllAsync(bool includeInactive = false);
    Task<Product?> GetByIdAsync(int id);
    Task<(bool Success, string? Error)> AddAsync(Product product);
    Task<(bool Success, string? Error)> UpdateAsync(Product product);
    Task<(bool Success, string? Error)> DeactivateAsync(int id);
}