using System.Windows;
using System.Windows.Controls;
using InventoryManager.App.ViewModels;

namespace InventoryManager.App.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
        {
            vm.CurrentPassword = CurrentPasswordBox.Password;
            vm.NewPassword = NewPasswordBox.Password;
            vm.ChangePasswordCommand.Execute(null);
        }
    }
}