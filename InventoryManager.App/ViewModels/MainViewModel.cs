using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManager.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableObject currentView;

    public MainViewModel()
    {
        currentView = App.Services.GetRequiredService<DashboardViewModel>();
    }

    [RelayCommand]
    private void GoToDashboard() => CurrentView = App.Services.GetRequiredService<DashboardViewModel>();

    [RelayCommand]
    private void GoToProducts() => CurrentView = App.Services.GetRequiredService<ProductsViewModel>();

    [RelayCommand]
    private void GoToCategories() => CurrentView = App.Services.GetRequiredService<CategoriesViewModel>();

    [RelayCommand]
    private void GoToSuppliers() => CurrentView = App.Services.GetRequiredService<SuppliersViewModel>();

    [RelayCommand]
    private void GoToPurchases() => CurrentView = App.Services.GetRequiredService<PurchasesViewModel>();

    [RelayCommand]
    private void GoToSales() => CurrentView = App.Services.GetRequiredService<SalesViewModel>();

    [RelayCommand]
    private void GoToReports() => CurrentView = App.Services.GetRequiredService<ReportsViewModel>();

    [RelayCommand]
    private void GoToSettings() => CurrentView = App.Services.GetRequiredService<SettingsViewModel>();
}