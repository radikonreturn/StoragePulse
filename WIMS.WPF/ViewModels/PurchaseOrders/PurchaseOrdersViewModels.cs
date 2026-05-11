using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using WIMS.Core.Entities;
using WIMS.Core.Enums;
using WIMS.Core.Interfaces;
using WIMS.Data;
using WIMS.WPF.Services;

namespace WIMS.WPF.ViewModels.PurchaseOrders;

public partial class PurchaseOrdersViewModel : BaseViewModel
{
    private readonly WIMSDbContext _db;
    private readonly INavigationService _navigation;

    public ObservableCollection<PurchaseOrderRow> Orders { get; } = new();

    [ObservableProperty]
    private PurchaseOrderRow? _selectedOrder;

    public bool HasOrders => Orders.Count > 0;
    public bool IsEmpty => !IsBusy && Orders.Count == 0;

    public PurchaseOrdersViewModel(WIMSDbContext db, INavigationService navigation)
    {
        _db = db;
        _navigation = navigation;
        Title = "Sipariş Yönetimi";
        Orders.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasOrders));
            OnPropertyChanged(nameof(IsEmpty));
        };
        _ = LoadOrdersAsync();
    }

    [RelayCommand]
    private async Task LoadOrdersAsync()
    {
        try
        {
            IsBusy = true;
            OnPropertyChanged(nameof(IsEmpty));
            ClearError();
            var rows = await _db.PurchaseOrders
                .AsNoTracking()
                .Where(o => o.IsActive)
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.Id)
                .Select(o => new PurchaseOrderRow
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    SupplierName = o.Supplier.Name,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    ExpectedDeliveryDate = o.ExpectedDeliveryDate,
                    TotalAmount = o.Lines.Where(l => l.IsActive).Sum(l => l.Quantity * l.UnitPrice)
                })
                .ToListAsync();

            Orders.Clear();
            foreach (var row in rows)
            {
                Orders.Add(row);
            }
        }
        catch (Exception ex)
        {
            SetError($"Siparişler yüklenemedi: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(IsEmpty));
        }
    }

    [RelayCommand]
    private void CreateOrder() => _navigation.NavigateTo<PurchaseOrderWizardViewModel>();

    [RelayCommand]
    private void ViewOrder(object? order)
    {
        var row = order as PurchaseOrderRow ?? SelectedOrder;
        if (row is null)
        {
            SetError("Görüntülemek için bir sipariş seçin.");
            return;
        }

        _navigation.NavigateTo<PurchaseOrderDetailViewModel>(row.Id);
    }

    [RelayCommand]
    private void FilterByStatus(object? status) => ClearError();
}

public partial class PurchaseOrderDetailViewModel : BaseViewModel, IParameterizedViewModel, IParameterState
{
    private readonly WIMSDbContext _db;
    private readonly INavigationService _navigation;
    private object? _parameter;

    public ObservableCollection<PurchaseOrderLineRow> Lines { get; } = new();

    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _orderNumber = string.Empty;

    [ObservableProperty]
    private string _supplierName = string.Empty;

    [ObservableProperty]
    private DateTime _orderDate = DateTime.Today;

    [ObservableProperty]
    private DateTime? _expectedDeliveryDate;

    [ObservableProperty]
    private PurchaseOrderStatus _status;

    public PurchaseOrderDetailViewModel(WIMSDbContext db, INavigationService navigation)
    {
        _db = db;
        _navigation = navigation;
        Title = "Sipariş Detayı";
    }

    public object? Parameter => _parameter;

    public void SetParameter(object parameter)
    {
        _parameter = parameter;
        if (parameter is int id)
        {
            _ = LoadOrderAsync(id);
        }
    }

    [RelayCommand]
    private Task SaveAsync()
    {
        Close();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ConfirmOrderAsync()
    {
        await SetStatusAsync(PurchaseOrderStatus.Confirmed);
    }

    [RelayCommand]
    private async Task CancelOrderAsync()
    {
        await SetStatusAsync(PurchaseOrderStatus.Cancelled);
    }

    private async Task LoadOrderAsync(int id)
    {
        var order = await _db.PurchaseOrders
            .AsNoTracking()
            .Include(o => o.Supplier)
            .Include(o => o.Lines).ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(o => o.Id == id && o.IsActive);

        if (order is null)
        {
            SetError("Sipariş bulunamadı.");
            return;
        }

        Id = order.Id;
        OrderNumber = order.OrderNumber;
        SupplierName = order.Supplier.Name;
        OrderDate = order.OrderDate;
        ExpectedDeliveryDate = order.ExpectedDeliveryDate;
        Status = order.Status;

        Lines.Clear();
        foreach (var line in order.Lines.Where(l => l.IsActive))
        {
            Lines.Add(new PurchaseOrderLineRow
            {
                ProductName = line.Product.Name,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                LineTotal = line.Quantity * line.UnitPrice
            });
        }
    }

    private async Task SetStatusAsync(PurchaseOrderStatus status)
    {
        try
        {
            var order = await _db.PurchaseOrders.FirstOrDefaultAsync(o => o.Id == Id && o.IsActive);
            if (order is null)
            {
                SetError("Sipariş bulunamadı.");
                return;
            }

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            Close();
        }
        catch (Exception ex)
        {
            SetError($"Sipariş güncellenemedi: {ex.Message}");
        }
    }

    private void Close()
    {
        if (_navigation.CanGoBack)
        {
            _navigation.GoBack();
        }
        else
        {
            _navigation.NavigateTo<PurchaseOrdersViewModel>();
        }
    }
}

public partial class PurchaseOrderWizardViewModel : BaseViewModel, IParameterizedViewModel, IParameterState
{
    private readonly WIMSDbContext _db;
    private readonly INavigationService _navigation;
    private object? _parameter;

    public ObservableCollection<SupplierLookup> Suppliers { get; } = new();

    [ObservableProperty]
    private int _currentStep = 1;

    [ObservableProperty]
    private int _supplierId;

    [ObservableProperty]
    private DateTime _orderDate = DateTime.Today;

    [ObservableProperty]
    private DateTime? _expectedDeliveryDate;

    [ObservableProperty]
    private string? _notes;

    public PurchaseOrderWizardViewModel(WIMSDbContext db, INavigationService navigation)
    {
        _db = db;
        _navigation = navigation;
        Title = "Sipariş Oluşturma";
        _ = LoadSuppliersAsync();
    }

    public object? Parameter => _parameter;

    public void SetParameter(object parameter) => _parameter = parameter;

    [RelayCommand]
    private void NextStep()
    {
        if (CurrentStep < 3)
        {
            CurrentStep++;
        }
        else
        {
            _ = FinishAsync();
        }
    }

    [RelayCommand]
    private void PreviousStep()
    {
        if (CurrentStep > 1)
        {
            CurrentStep--;
        }
    }

    [RelayCommand]
    private async Task FinishAsync()
    {
        try
        {
            ClearError();
            if (SupplierId <= 0)
            {
                SetError("Sipariş için tedarikçi seçin.");
                return;
            }

            var order = new PurchaseOrder
            {
                OrderNumber = $"PO-{DateTime.Now:yyyyMMdd-HHmmss}",
                SupplierId = SupplierId,
                OrderDate = OrderDate.Date,
                ExpectedDeliveryDate = ExpectedDeliveryDate?.Date,
                Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
                Status = PurchaseOrderStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            _db.PurchaseOrders.Add(order);
            await _db.SaveChangesAsync();
            _navigation.NavigateTo<PurchaseOrderDetailViewModel>(order.Id);
        }
        catch (Exception ex)
        {
            SetError($"Sipariş oluşturulamadı: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Cancel() => Close();

    private async Task LoadSuppliersAsync()
    {
        var rows = await _db.Suppliers
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new SupplierLookup(s.Id, $"{s.Code} - {s.Name}"))
            .ToListAsync();

        Suppliers.Clear();
        foreach (var row in rows)
        {
            Suppliers.Add(row);
        }

        SupplierId = SupplierId == 0 ? Suppliers.FirstOrDefault()?.Id ?? 0 : SupplierId;
    }

    private void Close()
    {
        if (_navigation.CanGoBack)
        {
            _navigation.GoBack();
        }
        else
        {
            _navigation.NavigateTo<PurchaseOrdersViewModel>();
        }
    }
}

public sealed class PurchaseOrderRow
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public decimal TotalAmount { get; set; }
}

public sealed class PurchaseOrderLineRow
{
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed record SupplierLookup(int Id, string Name);
