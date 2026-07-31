using InventoryManager.Core.Entities;
using InventoryManager.Core.Interfaces;
using InventoryManager.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllAsync(bool includeInactive = false)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        return await query.OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<(bool Success, string? Error)> AddAsync(Product product)
    {
        var error = ValidateProduct(product);
        if (error != null)
            return (false, error);

        var skuExists = await _context.Products
            .AnyAsync(p => p.SKU.ToLower() == product.SKU.ToLower());

        if (skuExists)
            return (false, $"A product with SKU '{product.SKU}' already exists.");

        product.CreatedAt = DateTime.UtcNow;
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(Product product)
    {
        var error = ValidateProduct(product);
        if (error != null)
            return (false, error);

        var skuExists = await _context.Products
            .AnyAsync(p => p.Id != product.Id && p.SKU.ToLower() == product.SKU.ToLower());

        if (skuExists)
            return (false, $"A product with SKU '{product.SKU}' already exists.");

        product.UpdatedAt = DateTime.UtcNow;
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeactivateAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null)
            return (false, "Product not found.");

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return (true, null);
    }

    private static string? ValidateProduct(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
            return "Product name is required.";

        if (string.IsNullOrWhiteSpace(product.SKU))
            return "SKU is required.";

        if (product.CategoryId <= 0)
            return "Category is required.";

        if (product.CostPrice < 0)
            return "Cost price cannot be negative.";

        if (product.SellingPrice < 0)
            return "Selling price cannot be negative.";

        if (product.ReorderLevel < 0)
            return "Reorder level cannot be negative.";

        return null;
    }
}