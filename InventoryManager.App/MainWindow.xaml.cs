using System.Windows;
using InventoryManager.App.ViewModels;

namespace InventoryManager.App;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}