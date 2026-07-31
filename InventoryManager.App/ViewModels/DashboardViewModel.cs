using CommunityToolkit.Mvvm.ComponentModel;
using InventoryManager.Core.Interfaces;

namespace InventoryManager.App.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IDashboardService _dashboardService;

    [ObservableProperty]
    private int totalProducts;

    [ObservableProperty]
    private int lowStockCount;

    [ObservableProperty]
    private int outOfStockCount;

    [ObservableProperty]
    private int totalSuppliers;

    [ObservableProperty]
    private decimal inventoryValue;

    [ObservableProperty]
    private decimal todaysSales;

    [ObservableProperty]
    private decimal todaysProfit;

    public DashboardViewModel(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var summary = await _dashboardService.GetSummaryAsync();

        TotalProducts = summary.TotalProducts;
        LowStockCount = summary.LowStockCount;
        OutOfStockCount = summary.OutOfStockCount;
        TotalSuppliers = summary.TotalSuppliers;
        InventoryValue = summary.InventoryValue;
        TodaysSales = summary.TodaysSales;
        TodaysProfit = summary.TodaysProfit;
    }
}