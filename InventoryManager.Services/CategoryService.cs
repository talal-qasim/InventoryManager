using InventoryManager.Core.Entities;
using InventoryManager.Core.Interfaces;
using InventoryManager.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync(bool includeInactive = false)
    {
        var query = _context.Categories.AsQueryable();

        if (!includeInactive)
            query = query.Where(c => c.IsActive);

        return await query.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task<(bool Success, string? Error)> AddAsync(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
            return (false, "Category name is required.");

        var nameExists = await _context.Categories
            .AnyAsync(c => c.Name.ToLower() == category.Name.ToLower());

        if (nameExists)
            return (false, $"A category named '{category.Name}' already exists.");

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
            return (false, "Category name is required.");

        var nameExists = await _context.Categories
            .AnyAsync(c => c.Id != category.Id && c.Name.ToLower() == category.Name.ToLower());

        if (nameExists)
            return (false, $"A category named '{category.Name}' already exists.");

        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeactivateAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category is null)
            return (false, "Category not found.");

        category.IsActive = false;
        await _context.SaveChangesAsync();
        return (true, null);
    }
}