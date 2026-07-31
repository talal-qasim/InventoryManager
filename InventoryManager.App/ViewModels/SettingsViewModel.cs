using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManager.Core.Interfaces;

namespace InventoryManager.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;

    private int _settingsId;

    [ObservableProperty]
    private string businessName = string.Empty;

    [ObservableProperty]
    private string currencySymbol = string.Empty;

    [ObservableProperty]
    private int defaultReorderLevel;

    [ObservableProperty]
    private string settingsMessage = string.Empty;

    [ObservableProperty]
    private string currentPassword = string.Empty;

    [ObservableProperty]
    private string newPassword = string.Empty;

    [ObservableProperty]
    private string passwordMessage = string.Empty;

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var settings = await _settingsService.GetSettingsAsync();
        _settingsId = settings.Id;
        BusinessName = settings.BusinessName;
        CurrencySymbol = settings.CurrencySymbol;
        DefaultReorderLevel = settings.DefaultReorderLevel;
    }

    [RelayCommand]
    private async Task SaveSettings()
    {
        SettingsMessage = string.Empty;

        var settings = new Core.Entities.AppSettings
        {
            Id = _settingsId,
            BusinessName = BusinessName,
            CurrencySymbol = CurrencySymbol,
            DefaultReorderLevel = DefaultReorderLevel
        };

        var result = await _settingsService.UpdateSettingsAsync(settings);

        SettingsMessage = result.Success
            ? "Settings saved successfully."
            : result.Error ?? "Unable to save settings.";
    }

    [RelayCommand]
    private async Task ChangePassword()
    {
        PasswordMessage = string.Empty;

        var result = await _settingsService.ChangePasswordAsync(1, CurrentPassword, NewPassword);

        PasswordMessage = result.Success
            ? "Password changed successfully."
            : result.Error ?? "Unable to change password.";

        if (result.Success)
        {
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
        }
    }
}