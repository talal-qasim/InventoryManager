using System;
using System.Linq;
using System.Threading.Tasks;
using InventoryManager.App.ViewModels;
using InventoryManager.Core.Entities;
using InventoryManager.Data;
using InventoryManager.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryManager.Tests;

public class ProductsViewModelTests
{
    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        return context;
    }

    [Fact]
    public async Task ProductsViewModel_LoadsCategoriesSuppliersAndProducts()
    {
        // Arrange
        using var context = CreateDbContext();
        var category = new Category { Name = "Electronics", Description = "Gadgets" };
        var supplier = new Supplier { Name = "Tech Corp", ContactPerson = "John" };
        context.Categories.Add(category);
        context.Suppliers.Add(supplier);
        await context.SaveChangesAsync();

        var productService = new ProductService(context);
        var categoryService = new CategoryService(context);
        var supplierService = new SupplierService(context);

        var product = new Product
        {
            SKU = "LAP-001",
            Name = "Laptop Pro",
            CategoryId = category.Id,
            SupplierId = supplier.Id,
            CostPrice = 800,
            SellingPrice = 1200,
            Unit = "pcs"
        };
        await productService.AddAsync(product);

        // Act
        var vm = new ProductsViewModel(productService, categoryService, supplierService);
        await vm.LoadDropdownsAsync();
        await vm.LoadProductsAsync();

        // Assert
        Assert.Single(vm.Categories);
        Assert.Equal("Electronics", vm.Categories[0].Name);

        Assert.Single(vm.Suppliers);
        Assert.Equal("Tech Corp", vm.Suppliers[0].Name);

        Assert.Single(vm.Products);
        Assert.Equal("Laptop Pro", vm.Products[0].Name);
    }

    [Fact]
    public async Task ProductsViewModel_AddProduct_SuccessfullyAddsProduct()
    {
        // Arrange
        using var context = CreateDbContext();
        var category = new Category { Name = "Hardware", Description = "Tools" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var productService = new ProductService(context);
        var categoryService = new CategoryService(context);
        var supplierService = new SupplierService(context);

        var vm = new ProductsViewModel(productService, categoryService, supplierService);
        await vm.LoadDropdownsAsync();

        vm.NewSku = "HAM-001";
        vm.NewName = "Hammer";
        vm.NewDescription = "Heavy duty hammer";
        vm.NewSelectedCategory = vm.Categories.First();
        vm.NewCostPrice = 10;
        vm.NewSellingPrice = 25;
        vm.NewReorderLevel = 5;
        vm.NewUnit = "pcs";

        // Act
        await vm.AddProductCommand.ExecuteAsync(null);

        // Assert
        Assert.Empty(vm.ErrorMessage);
        Assert.Single(vm.Products);
        Assert.Equal("Hammer", vm.Products[0].Name);
        Assert.Equal("HAM-001", vm.Products[0].SKU);
    }

    [Fact]
    public async Task ProductsViewModel_SearchFilter_FiltersByNameAndSKU()
    {
        // Arrange
        using var context = CreateDbContext();
        var category = new Category { Name = "Stationery", Description = "Office supplies" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var productService = new ProductService(context);
        var categoryService = new CategoryService(context);
        var supplierService = new SupplierService(context);

        await productService.AddAsync(new Product { SKU = "PEN-001", Name = "Blue Pen", CategoryId = category.Id });
        await productService.AddAsync(new Product { SKU = "PNC-001", Name = "Pencil HB", CategoryId = category.Id });

        var vm = new ProductsViewModel(productService, categoryService, supplierService);
        await vm.LoadProductsAsync();
        Assert.Equal(2, vm.Products.Count);

        // Act - Search by "Pen"
        vm.SearchText = "Blue";

        // Assert
        Assert.Single(vm.Products);
        Assert.Equal("Blue Pen", vm.Products[0].Name);

        // Act - Clear search
        vm.SearchText = string.Empty;
        Assert.Equal(2, vm.Products.Count);
    }

    [Fact]
    public async Task ProductsViewModel_DeactivateProduct_DeactivatesProduct()
    {
        // Arrange
        using var context = CreateDbContext();
        var category = new Category { Name = "General", Description = "Items" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var productService = new ProductService(context);
        var categoryService = new CategoryService(context);
        var supplierService = new SupplierService(context);

        var p = new Product { SKU = "ITEM-1", Name = "Old Item", CategoryId = category.Id };
        await productService.AddAsync(p);

        var vm = new ProductsViewModel(productService, categoryService, supplierService);
        await vm.LoadProductsAsync();
        vm.SelectedProduct = vm.Products.First();

        // Act
        await vm.DeactivateProductCommand.ExecuteAsync(null);

        // Assert
        Assert.Empty(vm.Products); // Deactivated products are excluded by default in GetAllAsync
    }
}
