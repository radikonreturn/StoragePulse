using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using WIMS.Application.Mapping;
using WIMS.Application.Services;
using WIMS.Core.Interfaces;
using WIMS.Data;
using WIMS.Data.Repositories;
using WIMS.Data.Services;
using WIMS.WPF.Services;
using WIMS.WPF.ViewModels;
using WIMS.WPF.ViewModels.Dashboard;
using WIMS.WPF.ViewModels.Products;
using WIMS.WPF.ViewModels.StockMovements;
using WIMS.WPF.ViewModels.Suppliers;
using WIMS.WPF.ViewModels.PurchaseOrders;
using WIMS.WPF.ViewModels.Reports;
using WIMS.WPF.ViewModels.Settings;
using Microsoft.EntityFrameworkCore;

namespace WIMS.WPF;

public partial class App : System.Windows.Application
{
    public static IServiceProvider? ServiceProvider { get; private set; }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        var dbPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WIMS", "wims.db");

        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dbPath)!);

        services.AddDbContext<WIMSDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IInventoryDashboardService, InventoryDashboardService>();
        services.AddScoped<IProductCatalogService, ProductCatalogService>();
        services.AddScoped<ISupplierCatalogService, SupplierCatalogService>();
        services.AddSingleton<IMessengerService, MessengerService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IConfirmationService, ConfirmationService>();
        services.AddSingleton<IMapperService, MapperService>();
        services.AddSingleton<IReportService, ReportService>();

        services.AddSingleton<ShellViewModel>();

        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ProductsViewModel>();
        services.AddTransient<ProductsDetailViewModel>();
        services.AddTransient<StockMovementsViewModel>();
        services.AddTransient<StockEntryViewModel>();
        services.AddTransient<SuppliersViewModel>();
        services.AddTransient<SuppliersDetailViewModel>();
        services.AddTransient<PurchaseOrdersViewModel>();
        services.AddTransient<PurchaseOrderDetailViewModel>();
        services.AddTransient<PurchaseOrderWizardViewModel>();
        services.AddTransient<ReportsViewModel>();
        services.AddTransient<SettingsViewModel>();

        services.AddSingleton<MainWindow>(s => new MainWindow
        {
            DataContext = s.GetRequiredService<ShellViewModel>()
        });

        ServiceProvider = services.BuildServiceProvider();

        try
        {
            await InitializeDatabaseAsync(ServiceProvider);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Veritabanı başlatılamadı: {ex.Message}",
                "StoragePulse",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
            return;
        }

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WIMSDbContext>();
        await StampExistingEnsureCreatedDatabaseAsync(db);
        await db.Database.MigrateAsync();
        await DatabaseSeeder.SeedAsync(db);
    }

    private static async Task StampExistingEnsureCreatedDatabaseAsync(WIMSDbContext db)
    {
        if (!await db.Database.CanConnectAsync())
        {
            return;
        }

        var connection = db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync();

        try
        {
            if (!await TableExistsAsync(connection, "Products") || await TableExistsAsync(connection, "__EFMigrationsHistory"))
            {
                return;
            }

            await ExecuteNonQueryAsync(connection, """
                CREATE TABLE "__EFMigrationsHistory" (
                    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
                    "ProductVersion" TEXT NOT NULL
                );
                """);

            await ExecuteNonQueryAsync(connection, """
                INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                VALUES ('20260512000000_InitialCreate', '8.0.11');
                """);
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }

    private static async Task<bool> TableExistsAsync(System.Data.Common.DbConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $tableName;";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "$tableName";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    private static async Task ExecuteNonQueryAsync(System.Data.Common.DbConnection connection, string commandText)
    {
        using var command = connection.CreateCommand();
        command.CommandText = commandText;
        await command.ExecuteNonQueryAsync();
    }
}
