using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using WIMS.Core.Entities;
using WIMS.Core.Enums;
using WIMS.Core.Interfaces;
using WIMS.Data;
using WIMS.WPF.Services;

namespace WIMS.WPF.ViewModels.StockMovements;

public partial class StockMovementsViewModel : BaseViewModel
{
    private readonly WIMSDbContext _db;
    private readonly INavigationService _navigation;

    public ObservableCollection<StockMovementRow> Movements { get; } = new();

    public StockMovementsViewModel(WIMSDbContext db, INavigationService navigation)
    {
        _db = db;
        _navigation = navigation;
        Title = "Stok İşlemleri";
        _ = LoadMovementsAsync();
    }

    [RelayCommand]
    private async Task LoadMovementsAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            var rows = await _db.StockMovements
                .AsNoTracking()
                .Where(m => m.IsActive)
                .OrderByDescending(m => m.MovementDate)
                .ThenByDescending(m => m.Id)
                .Select(m => new StockMovementRow
                {
                    Id = m.Id,
                    MovementDate = m.MovementDate,
                    DocumentNumber = m.DocumentNumber,
                    Type = m.Type,
                    ProductName = m.Product.Name,
                    Quantity = m.Quantity,
                    UnitPrice = m.UnitPrice,
                    ReferenceNumber = m.ReferenceNumber ?? string.Empty
                })
                .ToListAsync();

            Movements.Clear();
            foreach (var row in rows)
            {
                Movements.Add(row);
            }
        }
        catch (Exception ex)
        {
            SetError($"Stok hareketleri yüklenemedi: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void NewInbound() => _navigation.NavigateTo<StockEntryViewModel>(MovementType.Inbound);

    [RelayCommand]
    private void NewOutbound() => _navigation.NavigateTo<StockEntryViewModel>(MovementType.Outbound);

    [RelayCommand]
    private void ExportHistory() => SetError("Excel aktarımı için raporlama servisi sonraki adımda bağlanacak.");
}

public partial class StockEntryViewModel : BaseViewModel, IParameterizedViewModel
{
    private readonly WIMSDbContext _db;
    private readonly INavigationService _navigation;

    public ObservableCollection<ProductLookup> Products { get; } = new();

    [ObservableProperty]
    private string _documentNumber = string.Empty;

    [ObservableProperty]
    private MovementType _movementType = MovementType.Inbound;

    [ObservableProperty]
    private DateTime _movementDate = DateTime.Today;

    [ObservableProperty]
    private int _productId;

    [ObservableProperty]
    private decimal _quantity;

    [ObservableProperty]
    private decimal _unitPrice;

    [ObservableProperty]
    private string? _referenceNumber;

    [ObservableProperty]
    private string? _notes;

    public StockEntryViewModel(WIMSDbContext db, INavigationService navigation)
    {
        _db = db;
        _navigation = navigation;
        Title = "Stok Girişi / Çıkışı";
        _ = LoadProductsAsync();
    }

    public void SetParameter(object parameter)
    {
        if (parameter is MovementType movementType)
        {
            MovementType = movementType;
            Title = movementType == MovementType.Inbound ? "Stok Girişi" : "Stok Çıkışı";
            DocumentNumber = $"{(movementType == MovementType.Inbound ? "GR" : "CK")}-{DateTime.Now:yyyyMMdd-HHmm}";
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            ClearError();

            if (string.IsNullOrWhiteSpace(DocumentNumber))
            {
                SetError("Belge numarası zorunludur.");
                return;
            }

            if (ProductId <= 0)
            {
                SetError("Ürün seçin.");
                return;
            }

            if (Quantity <= 0)
            {
                SetError("Miktar sıfırdan büyük olmalıdır.");
                return;
            }

            var movement = new StockMovement
            {
                DocumentNumber = DocumentNumber.Trim(),
                Type = MovementType,
                MovementDate = MovementDate.Date,
                ProductId = ProductId,
                Quantity = Quantity,
                UnitPrice = UnitPrice,
                ReferenceNumber = Normalize(ReferenceNumber),
                Notes = Normalize(Notes),
                CreatedAt = DateTime.UtcNow
            };

            _db.StockMovements.Add(movement);
            await _db.SaveChangesAsync();
            Close();
        }
        catch (Exception ex)
        {
            SetError($"Stok hareketi kaydedilemedi: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Cancel() => Close();

    partial void OnProductIdChanged(int value)
    {
        var product = Products.FirstOrDefault(p => p.Id == value);
        if (product is not null && UnitPrice == 0)
        {
            UnitPrice = product.UnitPrice;
        }
    }

    private async Task LoadProductsAsync()
    {
        var rows = await _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Code)
            .Select(p => new ProductLookup(p.Id, $"{p.Code} - {p.Name}", p.UnitPrice))
            .ToListAsync();

        Products.Clear();
        foreach (var row in rows)
        {
            Products.Add(row);
        }

        ProductId = ProductId == 0 ? Products.FirstOrDefault()?.Id ?? 0 : ProductId;
    }

    private void Close()
    {
        if (_navigation.CanGoBack)
        {
            _navigation.GoBack();
        }
        else
        {
            _navigation.NavigateTo<StockMovementsViewModel>();
        }
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class StockMovementRow
{
    public int Id { get; set; }
    public DateTime MovementDate { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public MovementType Type { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
}

public sealed record ProductLookup(int Id, string Name, decimal UnitPrice);
