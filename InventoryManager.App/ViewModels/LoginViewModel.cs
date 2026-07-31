using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManager.Core.Interfaces;

namespace InventoryManager.App.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthenticationService _authenticationService;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool rememberPassword;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public event EventHandler? LoginSucceeded;

    public LoginViewModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [RelayCommand]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter both username and password.";
            return;
        }

        var user = await _authenticationService.LoginAsync(Username, Password);

        if (user is null)
        {
            ErrorMessage = "Invalid username or password.";
            return;
        }

        ErrorMessage = string.Empty;
        LoginSucceeded?.Invoke(this, EventArgs.Empty);
    }
}