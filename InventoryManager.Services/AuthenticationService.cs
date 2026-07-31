using InventoryManager.Core.Entities;
using InventoryManager.Core.Helpers;
using InventoryManager.Core.Interfaces;
using InventoryManager.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly AppDbContext _context;

    public User? CurrentUser { get; private set; }

    public AuthenticationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> LoginAsync(string username, string password)
    {
        var lowerUsername = username.ToLower();
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == lowerUsername && u.IsActive);

        if (user is null)
            return null;

        var isValid = PasswordHasher.Verify(password, user.PasswordHash, user.PasswordSalt) ||
                      PasswordHasher.Verify(password.ToLower(), user.PasswordHash, user.PasswordSalt);
                      
        if (isValid)
        {
            CurrentUser = user;
            return user;
        }

        return null;
    }
}