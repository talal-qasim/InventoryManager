using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManager.Core.Entities;
using InventoryManager.Core.Interfaces;

namespace InventoryManager.App.ViewModels;

public partial class SalesViewModel : ObservableObject
{
    private readonly ISaleService _saleService;
    private readonly IProductService _productService;
    private readonly IAuthenticationService _authService;

    public ObservableCollection<Sale> Sales { get; } = new();
    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<SaleLineInput> PendingLines { get; } = new();

    [ObservableProperty]
    private Product? selectedProductForLine;

    partial void OnSelectedProductForLineChanged(Product? value)
    {
        if (value is not null)
        {
            LineQuantity = null;
            LineUnitPrice = null;
        }
    }

    [ObservableProperty]
    private int? lineQuantity;

    [ObservableProperty]
    private decimal? lineUnitPrice;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private string notes = string.Empty;

    public SalesViewModel(ISaleService saleService, IProductService productService, IAuthenticationService authService)
    {
        _saleService = saleService;
        _productService = productService;
        _authService = authService;

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadProductsAsync();
        await LoadSalesAsync();
    }

    private async Task LoadProductsAsync()
    {
        Products.Clear();
        var products = await _productService.GetAllAsync();
        foreach (var product in products)
            Products.Add(product);
    }

    private async Task LoadSalesAsync()
    {
        Sales.Clear();
        var sales = await _saleService.GetAllAsync();
        foreach (var sale in sales)
            Sales.Add(sale);
    }

    [RelayCommand]
    private void AddLine()
    {
        ErrorMessage = string.Empty;

        if (SelectedProductForLine is null)
        {
            ErrorMessage = "Please select a product for this line.";
            return;
        }

        if (LineQuantity is null or <= 0)
        {
            ErrorMessage = "Quantity must be positive.";
            return;
        }

        if (LineUnitPrice is null or < 0)
        {
            ErrorMessage = "Unit price cannot be negative.";
            return;
        }

        PendingLines.Add(new SaleLineInput
        {
            ProductId = SelectedProductForLine.Id,
            ProductName = SelectedProductForLine.Name,
            Quantity = LineQuantity.Value,
            UnitPrice = LineUnitPrice.Value
        });

        SelectedProductForLine = null;
        LineQuantity = null;
        LineUnitPrice = null;
    }

    [RelayCommand]
    private void RemoveLine(SaleLineInput line)
    {
        PendingLines.Remove(line);
    }

    [RelayCommand]
    private async Task SaveSale()
    {
        ErrorMessage = string.Empty;

        if (PendingLines.Count == 0)
        {
            ErrorMessage = "Add at least one product line before saving.";
            return;
        }

        var userId = _authService.CurrentUser?.Id ?? 1;

        var result = await _saleService.CreateSaleAsync(
            PendingLines.ToList(),
            createdByUserId: userId,
            Notes);

        if (!result.Success)
        {
            ErrorMessage = result.Error ?? "Unable to save sale.";
            return;
        }

        PendingLines.Clear();
        Notes = string.Empty;

        await LoadSalesAsync();
        await LoadProductsAsync(); // refresh stock numbers
    }
}