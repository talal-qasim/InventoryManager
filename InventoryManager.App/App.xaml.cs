using System.Windows;
using InventoryManager.App.Views;
using InventoryManager.Core.Interfaces;
using InventoryManager.Data;
using InventoryManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InventoryManager.App;

public partial class App : Application
{
    private IHost? _host;

    public static IServiceProvider Services { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
            })
            .ConfigureServices((context, services) =>
            {
                var connectionString = context.Configuration.GetConnectionString("DefaultConnection");

                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite(connectionString));

                services.AddScoped<IAuthenticationService, AuthenticationService>();
                services.AddScoped<ICategoryService, CategoryService>();
                services.AddScoped<ISupplierService, SupplierService>();

                services.AddTransient<Views.LoginView>();
                services.AddTransient<ViewModels.LoginViewModel>();
                services.AddTransient<MainWindow>();
                services.AddTransient<ViewModels.MainViewModel>();

                services.AddTransient<ViewModels.DashboardViewModel>();
                services.AddTransient<ViewModels.ProductsViewModel>();
                services.AddTransient<ViewModels.CategoriesViewModel>();
                services.AddTransient<ViewModels.SuppliersViewModel>();
                services.AddTransient<ViewModels.PurchasesViewModel>();
                services.AddTransient<ViewModels.SalesViewModel>();
                services.AddTransient<ViewModels.ReportsViewModel>();
                services.AddTransient<ViewModels.SettingsViewModel>();

                services.AddScoped<IProductService, ProductService>();

                services.AddScoped<IInventoryService, InventoryService>();

                services.AddScoped<IPurchaseService, PurchaseService>();

                services.AddScoped<ISaleService, SaleService>();

                services.AddScoped<IDashboardService, DashboardService>();
            })
            .Build();

        Services = _host.Services;

        base.OnStartup(e);

        using (var scope = _host.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbInitializer.SeedAsync(context);
        }

        var loginView = _host.Services.GetRequiredService<LoginView>();
        loginView.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        base.OnExit(e);
    }
}