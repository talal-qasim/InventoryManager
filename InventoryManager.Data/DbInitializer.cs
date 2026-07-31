using InventoryManager.Core.Entities;
using InventoryManager.Core.Helpers;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Users.AnyAsync())
        {
            var (hash, salt) = PasswordHasher.Hash("Admin@123");

            context.Users.Add(new User
            {
                Username = "admin",
                PasswordHash = hash,
                PasswordSalt = salt,
                IsActive = true
            });
        }

        if (!await context.AppSettings.AnyAsync())
        {
            context.AppSettings.Add(new AppSettings());
        }

        await context.SaveChangesAsync();
    }
}