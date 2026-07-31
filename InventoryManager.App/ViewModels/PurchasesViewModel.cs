using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManager.Core.Entities;
using InventoryManager.Core.Interfaces;

namespace InventoryManager.App.ViewModels;

public partial class PurchasesViewModel : ObservableObject
{
    private readonly IPurchaseService _purchaseService;
    private readonly ISupplierService _supplierService;
    private readonly IProductService _productService;
    private readonly IAuthenticationService _authService;

    public ObservableCollection<Purchase> Purchases { get; } = new();
    public ObservableCollection<Supplier> Suppliers { get; } = new();
    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<PurchaseLineInput> PendingLines { get; } = new();

    [ObservableProperty]
    private Supplier? selectedSupplier;

    [ObservableProperty]
    private Product? selectedProductForLine;

    partial void OnSelectedProductForLineChanged(Product? value)
    {
        if (value is not null)
        {
            LineUnitCost = value.CostPrice;
            if (LineQuantity <= 0)
            {
                LineQuantity = 1;
            }
        }
    }

    [ObservableProperty]
    private int lineQuantity;

    [ObservableProperty]
    private decimal lineUnitCost;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private string notes = string.Empty;

    public PurchasesViewModel(
        IPurchaseService purchaseService,
        ISupplierService supplierService,
        IProductService productService,
        IAuthenticationService authService)
    {
        _purchaseService = purchaseService;
        _supplierService = supplierService;
        _productService = productService;
        _authService = authService;

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        var suppliers = await _supplierService.GetAllAsync();
        foreach (var supplier in suppliers)
            Suppliers.Add(supplier);

        var products = await _productService.GetAllAsync();
        foreach (var product in products)
            Products.Add(product);

        await LoadPurchasesAsync();
    }

    private async Task LoadPurchasesAsync()
    {
        Purchases.Clear();
        var purchases = await _purchaseService.GetAllAsync();
        foreach (var purchase in purchases)
            Purchases.Add(purchase);
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

        if (LineQuantity <= 0)
        {
            ErrorMessage = "Quantity must be positive.";
            return;
        }

        if (LineUnitCost < 0)
        {
            ErrorMessage = "Unit cost cannot be negative.";
            return;
        }

        PendingLines.Add(new PurchaseLineInput
        {
            ProductId = SelectedProductForLine.Id,
            ProductName = SelectedProductForLine.Name,
            Quantity = LineQuantity,
            UnitCost = LineUnitCost
        });

        SelectedProductForLine = null;
        LineQuantity = 0;
        LineUnitCost = 0;
    }

    [RelayCommand]
    private async Task SavePurchase()
    {
        ErrorMessage = string.Empty;

        if (SelectedSupplier is null)
        {
            ErrorMessage = "Please select a supplier.";
            return;
        }

        if (PendingLines.Count == 0)
        {
            ErrorMessage = "Add at least one product line before saving.";
            return;
        }

        var userId = _authService.CurrentUser?.Id ?? 1;

        var result = await _purchaseService.CreatePurchaseAsync(
            SelectedSupplier.Id,
            PendingLines.ToList(),
            createdByUserId: userId,
            Notes);

        if (!result.Success)
        {
            ErrorMessage = result.Error ?? "Unable to save purchase.";
            return;
        }

        PendingLines.Clear();
        SelectedSupplier = null;
        Notes = string.Empty;

        await LoadPurchasesAsync();

        // Refresh product list too, since CurrentStock changed
        Products.Clear();
        var products = await _productService.GetAllAsync();
        foreach (var product in products)
            Products.Add(product);
    }

    [RelayCommand]
    private void RemoveLine(PurchaseLineInput line)
    {
        PendingLines.Remove(line);
    }
}