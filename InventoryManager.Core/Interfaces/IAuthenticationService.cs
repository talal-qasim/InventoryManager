using InventoryManager.Core.Entities;

namespace InventoryManager.Core.Interfaces;

public interface IAuthenticationService
{
    User? CurrentUser { get; }
    Task<User?> LoginAsync(string username, string password);
}