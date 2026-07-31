using InventoryManager.Core.Entities;

namespace InventoryManager.Core.Interfaces;

public interface ISettingsService
{
    Task<AppSettings> GetSettingsAsync();
    Task<(bool Success, string? Error)> UpdateSettingsAsync(AppSettings settings);
    Task<(bool Success, string? Error)> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
}