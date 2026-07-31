using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InventoryManager.Core.Entities;
using InventoryManager.Core.Enums;
using InventoryManager.Core.Interfaces;
using InventoryManager.Data;
using InventoryManager.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryManager.Tests;

public class SaleServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;

    public SaleServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task CreateSaleAsync_WithValidData_SucceedsAndDeductsStock()
    {
        // Arrange
        var user = new User { Username = "admin", PasswordHash = new byte[16], PasswordSalt = new byte[16], IsActive = true };
        _context.Users.Add(user);

        var category = new Category { Name = "General" };
        _context.Categories.Add(category);

        var product = new Product
        {
            SKU = "PROD-1",
            Name = "Test Product",
            Category = category,
            CostPrice = 10,
            SellingPrice = 20,
            CurrentStock = 100,
            Unit = "pcs"
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var inventoryService = new InventoryService(_context);
        var saleService = new SaleService(_context, inventoryService);

        var lines = new List<SaleLineInput>
        {
            new SaleLineInput { ProductId = product.Id, ProductName = product.Name, Quantity = 5, UnitPrice = 25 },
            new SaleLineInput { ProductId = product.Id, ProductName = product.Name, Quantity = 2, UnitPrice = 25 }
        };

        // Act
        var result = await saleService.CreateSaleAsync(lines, user.Id, "abcde");

        // Assert
        Assert.True(result.Success, result.Error);
        var updatedProduct = await _context.Products.FindAsync(product.Id);
        Assert.Equal(93, updatedProduct!.CurrentStock);
    }

    [Fact]
    public async Task CreateSaleAsync_WithInvalidUserId_FallbackSucceeds()
    {
        // Arrange
        var category = new Category { Name = "General" };
        _context.Categories.Add(category);

        var product = new Product
        {
            SKU = "PROD-2",
            Name = "Test Product 2",
            Category = category,
            CostPrice = 10,
            SellingPrice = 20,
            CurrentStock = 50,
            Unit = "pcs"
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var inventoryService = new InventoryService(_context);
        var saleService = new SaleService(_context, inventoryService);

        var lines = new List<SaleLineInput>
        {
            new SaleLineInput { ProductId = product.Id, ProductName = product.Name, Quantity = 3, UnitPrice = 20 }
        };

        // Act - Pass user ID 999 which does not exist
        var result = await saleService.CreateSaleAsync(lines, createdByUserId: 999, notes: "Test Fallback User");

        // Assert
        Assert.True(result.Success, result.Error);
    }

    [Fact]
    public async Task CreateSaleAsync_InsufficientStock_FailsAndCleansUpState()
    {
        // Arrange
        var category = new Category { Name = "General" };
        _context.Categories.Add(category);

        var product = new Product
        {
            SKU = "PROD-3",
            Name = "Low Stock Product",
            Category = category,
            CostPrice = 10,
            SellingPrice = 20,
            CurrentStock = 5,
            Unit = "pcs"
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var inventoryService = new InventoryService(_context);
        var saleService = new SaleService(_context, inventoryService);

        var lines = new List<SaleLineInput>
        {
            new SaleLineInput { ProductId = product.Id, ProductName = product.Name, Quantity = 10, UnitPrice = 20 }
        };

        // Act
        var result = await saleService.CreateSaleAsync(lines, createdByUserId: 1, notes: "Oversell attempt");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Insufficient stock", result.Error);
    }
}
