using System.Windows;
using InventoryManager.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManager.App.Views;

public partial class LoginView : Window
{
    private readonly LoginViewModel _viewModel;

    public LoginView(LoginViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;
        _viewModel.LoginSucceeded += OnLoginSucceeded;
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Password = PasswordBox.Password;
        _viewModel.LoginCommand.Execute(null);
    }

    private void OnLoginSucceeded(object? sender, EventArgs e)
    {
        var mainWindow = App.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
        Close();
    }

    private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = App.Services.GetRequiredService<MainWindow>();
        
        if (mainWindow.DataContext is MainViewModel mainViewModel)
        {
            var settingsViewModel = App.Services.GetRequiredService<SettingsViewModel>();
            mainViewModel.CurrentView = settingsViewModel;
        }

        mainWindow.Show();
        Close();
    }
}