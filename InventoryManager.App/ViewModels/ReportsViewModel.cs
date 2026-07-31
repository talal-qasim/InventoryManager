using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManager.Core.Entities;
using InventoryManager.Core.Interfaces;
using Microsoft.Win32;

namespace InventoryManager.App.ViewModels;

public enum ReportType
{
    Inventory,
    Sales,
    LowStock,
    StockMovement
}

public partial class ReportsViewModel : ObservableObject
{
    private readonly IReportService _reportService;
    private readonly IExportService _exportService;

    public ObservableCollection<InventoryReportRow> InventoryRows { get; } = new();
    public ObservableCollection<SalesReportRow> SalesRows { get; } = new();
    public ObservableCollection<LowStockReportRow> LowStockRows { get; } = new();
    public ObservableCollection<StockMovement> MovementRows { get; } = new();

    [ObservableProperty]
    private ReportType activeReport = ReportType.Inventory;

    [ObservableProperty]
    private DateTime? fromDate;

    [ObservableProperty]
    private DateTime? toDate;

    [ObservableProperty]
    private string exportMessage = string.Empty;

    public bool IsInventoryActive => ActiveReport == ReportType.Inventory;
    public bool IsSalesActive => ActiveReport == ReportType.Sales;
    public bool IsLowStockActive => ActiveReport == ReportType.LowStock;
    public bool IsStockMovementActive => ActiveReport == ReportType.StockMovement;

    public ReportsViewModel(IReportService reportService, IExportService exportService)
    {
        _reportService = reportService;
        _exportService = exportService;
        _ = LoadActiveReportAsync();
    }

    partial void OnActiveReportChanged(ReportType value)
    {
        OnPropertyChanged(nameof(IsInventoryActive));
        OnPropertyChanged(nameof(IsSalesActive));
        OnPropertyChanged(nameof(IsLowStockActive));
        OnPropertyChanged(nameof(IsStockMovementActive));
        ExportMessage = string.Empty;
        _ = LoadActiveReportAsync();
    }

    [RelayCommand]
    private void ShowInventoryReport() => ActiveReport = ReportType.Inventory;

    [RelayCommand]
    private void ShowSalesReport() => ActiveReport = ReportType.Sales;

    [RelayCommand]
    private void ShowLowStockReport() => ActiveReport = ReportType.LowStock;

    [RelayCommand]
    private void ShowStockMovementReport() => ActiveReport = ReportType.StockMovement;

    [RelayCommand]
    private async Task ApplyDateFilter()
    {
        await LoadActiveReportAsync();
    }

    [RelayCommand]
    private void ExportCurrentReport()
    {
        ExportMessage = string.Empty;

        string csv;
        string defaultFileName;

        switch (ActiveReport)
        {
            case ReportType.Inventory:
                csv = _exportService.ToCsv(InventoryRows);
                defaultFileName = "InventoryReport.csv";
                break;
            case ReportType.Sales:
                csv = _exportService.ToCsv(SalesRows);
                defaultFileName = "SalesReport.csv";
                break;
            case ReportType.LowStock:
                csv = _exportService.ToCsv(LowStockRows);
                defaultFileName = "LowStockReport.csv";
                break;
            case ReportType.StockMovement:
                csv = _exportService.ToCsv(MovementRows);
                defaultFileName = "StockMovementReport.csv";
                break;
            default:
                return;
        }

        var dialog = new SaveFileDialog
        {
            FileName = defaultFileName,
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            File.WriteAllText(dialog.FileName, csv);
            ExportMessage = $"Exported successfully to {dialog.FileName}";
        }
    }

    private async Task LoadActiveReportAsync()
    {
        switch (ActiveReport)
        {
            case ReportType.Inventory:
                InventoryRows.Clear();
                var invRows = await _reportService.GetInventoryReportAsync();
                foreach (var row in invRows)
                    InventoryRows.Add(row);
                break;

            case ReportType.Sales:
                SalesRows.Clear();
                var salesRows = await _reportService.GetSalesReportAsync(FromDate, ToDate);
                foreach (var row in salesRows)
                    SalesRows.Add(row);
                break;

            case ReportType.LowStock:
                LowStockRows.Clear();
                var lowStockRows = await _reportService.GetLowStockReportAsync();
                foreach (var row in lowStockRows)
                    LowStockRows.Add(row);
                break;

            case ReportType.StockMovement:
                MovementRows.Clear();
                var movementRows = await _reportService.GetStockMovementReportAsync(FromDate, ToDate);
                foreach (var row in movementRows)
                    MovementRows.Add(row);
                break;
        }
    }
}