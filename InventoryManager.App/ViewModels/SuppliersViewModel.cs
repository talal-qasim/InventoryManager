using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManager.Core.Entities;
using InventoryManager.Core.Interfaces;

namespace InventoryManager.App.ViewModels;

public partial class SuppliersViewModel : ObservableObject
{
    private readonly ISupplierService _supplierService;

    public ObservableCollection<Supplier> Suppliers { get; } = new();

    [ObservableProperty]
    private string newSupplierName = string.Empty;

    [ObservableProperty]
    private string newContactPerson = string.Empty;

    [ObservableProperty]
    private string newPhone = string.Empty;

    [ObservableProperty]
    private string newEmail = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private Supplier? selectedSupplier;

    public SuppliersViewModel(ISupplierService supplierService)
    {
        _supplierService = supplierService;
        _ = LoadSuppliersAsync();
    }

    private async Task LoadSuppliersAsync()
    {
        Suppliers.Clear();
        var suppliers = await _supplierService.GetAllAsync();
        foreach (var supplier in suppliers)
            Suppliers.Add(supplier);
    }

    [RelayCommand]
    private async Task AddSupplier()
    {
        ErrorMessage = string.Empty;

        var supplier = new Supplier
        {
            Name = NewSupplierName,
            ContactPerson = NewContactPerson,
            Phone = NewPhone,
            Email = NewEmail
        };

        var result = await _supplierService.AddAsync(supplier);

        if (!result.Success)
        {
            ErrorMessage = result.Error ?? "Unable to add supplier.";
            return;
        }

        NewSupplierName = string.Empty;
        NewContactPerson = string.Empty;
        NewPhone = string.Empty;
        NewEmail = string.Empty;
        await LoadSuppliersAsync();
    }

    [RelayCommand]
    private async Task DeactivateSupplier()
    {
        if (SelectedSupplier is null)
            return;

        ErrorMessage = string.Empty;
        var result = await _supplierService.DeactivateAsync(SelectedSupplier.Id);

        if (!result.Success)
        {
            ErrorMessage = result.Error ?? "Unable to deactivate supplier.";
            return;
        }

        await LoadSuppliersAsync();
    }
}