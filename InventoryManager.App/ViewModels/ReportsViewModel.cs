using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using InventoryManager.Core.Entities;
using InventoryManager.Core.Interfaces;

namespace InventoryManager.App.ViewModels;

public partial class ReportsViewModel : ObservableObject
{
    private readonly IInventoryService _inventoryService;

    public ObservableCollection<StockMovement> Movements { get; } = new();

    public ReportsViewModel(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
        _ = LoadMovementsAsync();
    }

    private async Task LoadMovementsAsync()
    {
        Movements.Clear();
        var movements = await _inventoryService.GetAllMovementsAsync();
        foreach (var movement in movements)
            Movements.Add(movement);
    }
}