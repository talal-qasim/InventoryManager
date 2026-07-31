using InventoryManager.Core.Entities;
using InventoryManager.Core.Interfaces;
using InventoryManager.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Services;

public class SupplierService : ISupplierService
{
    private readonly AppDbContext _context;

    public SupplierService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Supplier>> GetAllAsync(bool includeInactive = false)
    {
        var query = _context.Suppliers.AsQueryable();

        if (!includeInactive)
            query = query.Where(s => s.IsActive);

        return await query.OrderBy(s => s.Name).ToListAsync();
    }

    public async Task<Supplier?> GetByIdAsync(int id)
    {
        return await _context.Suppliers.FindAsync(id);
    }

    public async Task<(bool Success, string? Error)> AddAsync(Supplier supplier)
    {
        if (string.IsNullOrWhiteSpace(supplier.Name))
            return (false, "Supplier name is required.");

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(Supplier supplier)
    {
        if (string.IsNullOrWhiteSpace(supplier.Name))
            return (false, "Supplier name is required.");

        _context.Suppliers.Update(supplier);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeactivateAsync(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier is null)
            return (false, "Supplier not found.");

        supplier.IsActive = false;
        await _context.SaveChangesAsync();
        return (true, null);
    }
}