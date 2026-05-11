using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WIMS.Core.Interfaces;

namespace WIMS.WPF.ViewModels.Dashboard;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly IInventoryDashboardService _dashboardService;

    [ObservableProperty]
    private int _productCount;

    [ObservableProperty]
    private int _supplierCount;

    [ObservableProperty]
    private int _pendingPurchaseOrderCount;

    [ObservableProperty]
    private decimal _totalStockValue;

    [ObservableProperty]
    private int _lowStockProductCount;

    [ObservableProperty]
    private int _outOfStockProductCount;

    [ObservableProperty]
    private DateTime? _lastRefreshedAt;

    public ObservableCollection<DashboardLowStockItem> LowStockItems { get; } = new();
    public ObservableCollection<DashboardMovementItem> RecentMovements { get; } = new();

    public bool HasLowStockItems => LowStockItems.Count > 0;
    public bool HasRecentMovements => RecentMovements.Count > 0;

    public string TotalStockValueText => TotalStockValue.ToString("C2", CultureInfo.CurrentCulture);
    public string LastRefreshedText => LastRefreshedAt is null ? "Henüz yenilenmedi" : LastRefreshedAt.Value.ToString("dd.MM.yyyy HH:mm");

    public DashboardViewModel(IInventoryDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
        Title = "Gösterge Paneli";
        LowStockItems.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasLowStockItems));
        RecentMovements.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasRecentMovements));
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            var summary = await _dashboardService.GetSummaryAsync();

            ProductCount = summary.ProductCount;
            SupplierCount = summary.SupplierCount;
            PendingPurchaseOrderCount = summary.PendingPurchaseOrderCount;
            TotalStockValue = summary.TotalStockValue;
            LowStockProductCount = summary.LowStockProductCount;
            OutOfStockProductCount = summary.OutOfStockProductCount;

            LowStockItems.Clear();
            foreach (var item in summary.LowStockItems)
            {
                LowStockItems.Add(item);
            }

            RecentMovements.Clear();
            foreach (var item in summary.RecentMovements)
            {
                RecentMovements.Add(item);
            }

            LastRefreshedAt = DateTime.Now;
        }
        catch (Exception ex)
        {
            SetError($"Gösterge paneli yüklenemedi: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnTotalStockValueChanged(decimal value) => OnPropertyChanged(nameof(TotalStockValueText));
    partial void OnLastRefreshedAtChanged(DateTime? value) => OnPropertyChanged(nameof(LastRefreshedText));
}
