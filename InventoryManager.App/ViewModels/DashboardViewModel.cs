using CommunityToolkit.Mvvm.ComponentModel;
using InventoryManager.Core.Interfaces;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace InventoryManager.App.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IDashboardService _dashboardService;
    private readonly IReportService _reportService;

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

    [ObservableProperty]
    private ISeries[] salesTrendSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] salesTrendXAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private ISeries[] topProductsSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] topProductsXAxes = Array.Empty<Axis>();

    public DashboardViewModel(IDashboardService dashboardService, IReportService reportService)
    {
        _dashboardService = dashboardService;
        _reportService = reportService;
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

        var dailySales = await _reportService.GetLast7DaysSalesAsync();

        SalesTrendSeries = new ISeries[]
        {
            new LineSeries<decimal>
            {
                Values = dailySales.Select(d => d.TotalRevenue).ToArray(),
                Name = "Revenue",
                Fill = null
            }
        };

        SalesTrendXAxes = new Axis[]
        {
            new Axis
            {
                Labels = dailySales.Select(d => d.Date.ToString("MM/dd")).ToArray()
            }
        };

        var topProducts = await _reportService.GetTopSellingProductsAsync(5);

        TopProductsSeries = new ISeries[]
        {
            new ColumnSeries<int>
            {
                Values = topProducts.Select(p => p.QuantitySold).ToArray(),
                Name = "Quantity Sold"
            }
        };

        TopProductsXAxes = new Axis[]
        {
            new Axis
            {
                Labels = topProducts.Select(p => p.ProductName).ToArray()
            }
        };
    }
}