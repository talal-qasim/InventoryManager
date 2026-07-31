using CommunityToolkit.Mvvm.ComponentModel;

namespace InventoryManager.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = "Settings (coming later)";
}