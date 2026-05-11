using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WIMS.Core.Interfaces;
using WIMS.WPF.ViewModels.Dashboard;
using WIMS.WPF.ViewModels.Products;
using WIMS.WPF.ViewModels.StockMovements;
using WIMS.WPF.ViewModels.Suppliers;
using WIMS.WPF.ViewModels.PurchaseOrders;
using WIMS.WPF.ViewModels.Reports;
using WIMS.WPF.ViewModels.Settings;

namespace WIMS.WPF.ViewModels;

public partial class ShellViewModel : BaseViewModel
{
    private bool _isSynchronizingSelection;

    public INavigationService Navigation { get; }
    public IMessengerService Messenger { get; }
    public ObservableCollection<ShellNavigationItem> NavigationItems { get; } = new();

    [ObservableProperty]
    private ShellNavigationItem? _selectedNavigationItem;

    public ShellViewModel(INavigationService navigation, IMessengerService messenger)
    {
        Navigation = navigation;
        Messenger = messenger;
        Title = "WIMS - Depo Envanter Yönetim Sistemi";

        NavigationItems.Add(new ShellNavigationItem("Gösterge Paneli", typeof(DashboardViewModel)));
        NavigationItems.Add(new ShellNavigationItem("Ürün Kartları", typeof(ProductsViewModel)));
        NavigationItems.Add(new ShellNavigationItem("Stok İşlemleri", typeof(StockMovementsViewModel)));
        NavigationItems.Add(new ShellNavigationItem("Tedarikçiler", typeof(SuppliersViewModel)));
        NavigationItems.Add(new ShellNavigationItem("Sipariş Yönetimi", typeof(PurchaseOrdersViewModel)));
        NavigationItems.Add(new ShellNavigationItem("Raporlama", typeof(ReportsViewModel)));
        NavigationItems.Add(new ShellNavigationItem("Ayarlar", typeof(SettingsViewModel)));

        if (Navigation is INotifyPropertyChanged observableNavigation)
        {
            observableNavigation.PropertyChanged += OnNavigationPropertyChanged;
        }

        Navigation.NavigateTo<DashboardViewModel>();
        SyncSelectedNavigationItem();
    }

    [RelayCommand] private void NavigateDashboard() => Navigation.NavigateTo<DashboardViewModel>();
    [RelayCommand] private void NavigateProducts() => Navigation.NavigateTo<ProductsViewModel>();
    [RelayCommand] private void NavigateStockMovements() => Navigation.NavigateTo<StockMovementsViewModel>();
    [RelayCommand] private void NavigateSuppliers() => Navigation.NavigateTo<SuppliersViewModel>();
    [RelayCommand] private void NavigatePurchaseOrders() => Navigation.NavigateTo<PurchaseOrdersViewModel>();
    [RelayCommand] private void NavigateReports() => Navigation.NavigateTo<ReportsViewModel>();
    [RelayCommand] private void NavigateSettings() => Navigation.NavigateTo<SettingsViewModel>();
    [RelayCommand] private void GoBack() => Navigation.GoBack();

    partial void OnSelectedNavigationItemChanged(ShellNavigationItem? value)
    {
        if (_isSynchronizingSelection || value is null)
        {
            return;
        }

        NavigateToItem(value);
    }

    private void OnNavigationPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(INavigationService.CurrentView))
        {
            SyncSelectedNavigationItem();
        }
    }

    private void SyncSelectedNavigationItem()
    {
        var currentViewType = Navigation.CurrentView?.GetType();
        if (currentViewType is null)
        {
            return;
        }

        var selectedItem = NavigationItems.FirstOrDefault(item => item.ViewModelType == currentViewType);
        if (selectedItem is null || Equals(SelectedNavigationItem, selectedItem))
        {
            return;
        }

        _isSynchronizingSelection = true;
        SelectedNavigationItem = selectedItem;
        _isSynchronizingSelection = false;
    }

    private void NavigateToItem(ShellNavigationItem item)
    {
        if (item.ViewModelType == typeof(DashboardViewModel))
        {
            Navigation.NavigateTo<DashboardViewModel>();
        }
        else if (item.ViewModelType == typeof(ProductsViewModel))
        {
            Navigation.NavigateTo<ProductsViewModel>();
        }
        else if (item.ViewModelType == typeof(StockMovementsViewModel))
        {
            Navigation.NavigateTo<StockMovementsViewModel>();
        }
        else if (item.ViewModelType == typeof(SuppliersViewModel))
        {
            Navigation.NavigateTo<SuppliersViewModel>();
        }
        else if (item.ViewModelType == typeof(PurchaseOrdersViewModel))
        {
            Navigation.NavigateTo<PurchaseOrdersViewModel>();
        }
        else if (item.ViewModelType == typeof(ReportsViewModel))
        {
            Navigation.NavigateTo<ReportsViewModel>();
        }
        else if (item.ViewModelType == typeof(SettingsViewModel))
        {
            Navigation.NavigateTo<SettingsViewModel>();
        }
    }
}

public sealed class ShellNavigationItem
{
    public ShellNavigationItem(string title, Type viewModelType)
    {
        Title = title;
        ViewModelType = viewModelType;
    }

    public string Title { get; }
    public Type ViewModelType { get; }
}
