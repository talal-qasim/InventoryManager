using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManager.Core.Entities;
using InventoryManager.Core.Interfaces;

namespace InventoryManager.App.ViewModels;

public partial class ProductsViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ISupplierService _supplierService;

    private List<Product> _allProducts = new();

    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<Supplier> Suppliers { get; } = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string newSku = string.Empty;

    [ObservableProperty]
    private string newName = string.Empty;

    [ObservableProperty]
    private string newDescription = string.Empty;

    [ObservableProperty]
    private Category? newSelectedCategory;

    [ObservableProperty]
    private Supplier? newSelectedSupplier;

    [ObservableProperty]
    private decimal newCostPrice;

    [ObservableProperty]
    private decimal newSellingPrice;

    [ObservableProperty]
    private int newReorderLevel;

    [ObservableProperty]
    private string newUnit = "pcs";

    [ObservableProperty]
    private Product? selectedProduct;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public ProductsViewModel(
        IProductService productService,
        ICategoryService categoryService,
        ISupplierService supplierService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _supplierService = supplierService;

        _ = InitializeAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplySearchFilter();
    }

    private async Task InitializeAsync()
    {
        await LoadDropdownsAsync();
        await LoadProductsAsync();
    }

    public async Task LoadDropdownsAsync()
    {
        Categories.Clear();
        var categories = await _categoryService.GetAllAsync();
        foreach (var category in categories)
            Categories.Add(category);

        Suppliers.Clear();
        var suppliers = await _supplierService.GetAllAsync();
        foreach (var supplier in suppliers)
            Suppliers.Add(supplier);
    }

    public async Task LoadProductsAsync()
    {
        _allProducts = await _productService.GetAllAsync();
        ApplySearchFilter();
    }

    private void ApplySearchFilter()
    {
        Products.Clear();
        var query = _allProducts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            string term = SearchText.Trim().ToLowerInvariant();
            query = query.Where(p =>
                (p.Name != null && p.Name.ToLowerInvariant().Contains(term)) ||
                (p.SKU != null && p.SKU.ToLowerInvariant().Contains(term)) ||
                (p.Category != null && p.Category.Name != null && p.Category.Name.ToLowerInvariant().Contains(term)));
        }

        foreach (var product in query)
        {
            Products.Add(product);
        }
    }

    [RelayCommand]
    private async Task AddProduct()
    {
        ErrorMessage = string.Empty;

        if (NewSelectedCategory is null)
        {
            ErrorMessage = "Please select a category.";
            return;
        }

        var product = new Product
        {
            SKU = NewSku?.Trim() ?? string.Empty,
            Name = NewName?.Trim() ?? string.Empty,
            Description = string.IsNullOrWhiteSpace(NewDescription) ? null : NewDescription.Trim(),
            CategoryId = NewSelectedCategory.Id,
            SupplierId = NewSelectedSupplier?.Id,
            CostPrice = NewCostPrice,
            SellingPrice = NewSellingPrice,
            ReorderLevel = NewReorderLevel,
            Unit = string.IsNullOrWhiteSpace(NewUnit) ? "pcs" : NewUnit.Trim(),
            IsActive = true
        };

        var result = await _productService.AddAsync(product);

        if (!result.Success)
        {
            ErrorMessage = result.Error ?? "Unable to add product.";
            return;
        }

        NewSku = string.Empty;
        NewName = string.Empty;
        NewDescription = string.Empty;
        NewSelectedCategory = null;
        NewSelectedSupplier = null;
        NewCostPrice = 0;
        NewSellingPrice = 0;
        NewReorderLevel = 0;
        NewUnit = "pcs";

        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task DeactivateProduct()
    {
        if (SelectedProduct is null)
        {
            ErrorMessage = "Please select a product to deactivate.";
            return;
        }

        ErrorMessage = string.Empty;
        var result = await _productService.DeactivateAsync(SelectedProduct.Id);

        if (!result.Success)
        {
            ErrorMessage = result.Error ?? "Unable to deactivate product.";
            return;
        }

        await LoadProductsAsync();
    }
}