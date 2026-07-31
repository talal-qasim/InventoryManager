using InventoryManager.Core.Entities;
using InventoryManager.Core.Helpers;
using InventoryManager.Core.Interfaces;
using InventoryManager.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Services;

public class SettingsService : ISettingsService
{
    private readonly AppDbContext _context;

    public SettingsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppSettings> GetSettingsAsync()
    {
        var settings = await _context.AppSettings.FirstOrDefaultAsync();
        return settings ?? new AppSettings();
    }

    public async Task<(bool Success, string? Error)> UpdateSettingsAsync(AppSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.BusinessName))
            return (false, "Business name is required.");

        if (string.IsNullOrWhiteSpace(settings.CurrencySymbol))
            return (false, "Currency symbol is required.");

        if (settings.DefaultReorderLevel < 0)
            return (false, "Default reorder level cannot be negative.");

        _context.AppSettings.Update(settings);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user is null)
            return (false, "User not found.");

        if (!PasswordHasher.Verify(currentPassword, user.PasswordHash, user.PasswordSalt))
            return (false, "Current password is incorrect.");

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            return (false, "New password must be at least 6 characters.");

        var (hash, salt) = PasswordHasher.Hash(newPassword);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;

        await _context.SaveChangesAsync();
        return (true, null);
    }
}