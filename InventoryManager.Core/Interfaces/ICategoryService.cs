using InventoryManager.Core.Entities;

namespace InventoryManager.Core.Interfaces;

public interface ICategoryService
{
    Task<List<Category>> GetAllAsync(bool includeInactive = false);
    Task<Category?> GetByIdAsync(int id);
    Task<(bool Success, string? Error)> AddAsync(Category category);
    Task<(bool Success, string? Error)> UpdateAsync(Category category);
    Task<(bool Success, string? Error)> DeactivateAsync(int id);
}