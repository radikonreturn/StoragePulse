using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using WIMS.Core.Enums;
using WIMS.Core.Interfaces;
using WIMS.Data;

namespace WIMS.WPF.ViewModels.Reports;

public partial class ReportsViewModel : BaseViewModel
{
    private readonly WIMSDbContext _db;
    private readonly IReportService _reportService;

    private List<ProductDto> _stockValuationRows = new();
    private List<ProductDto> _lowStockRows = new();
    private List<StockMovementDto> _movementRows = new();

    public ObservableCollection<ProductDto> StockValuationRows { get; } = new();
    public ObservableCollection<ProductDto> LowStockRows { get; } = new();
    public ObservableCollection<StockMovementDto> MovementRows { get; } = new();

    [ObservableProperty]
    private string _reportTitle = "Rapor seçilmedi";

    [ObservableProperty]
    private string _reportSummary = "Bir rapor kartından işlem başlatın.";

    [ObservableProperty]
    private ReportKind _selectedReport = ReportKind.None;

    public bool IsStockValuationSelected => SelectedReport == ReportKind.StockValuation;
    public bool IsMovementHistorySelected => SelectedReport == ReportKind.MovementHistory;
    public bool IsLowStockSelected => SelectedReport == ReportKind.LowStock;
    public bool HasReport => SelectedReport != ReportKind.None;
    public bool IsReportEmpty => HasReport
        && ((IsStockValuationSelected && StockValuationRows.Count == 0)
            || (IsMovementHistorySelected && MovementRows.Count == 0)
            || (IsLowStockSelected && LowStockRows.Count == 0));

    public ReportsViewModel(WIMSDbContext db, IReportService reportService)
    {
        _db = db;
        _reportService = reportService;
        Title = "Raporlama";
        StockValuationRows.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsReportEmpty));
        MovementRows.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsReportEmpty));
        LowStockRows.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsReportEmpty));
    }

    [RelayCommand]
    private async Task LoadStockValuationAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();
            SelectedReport = ReportKind.StockValuation;
            ReportTitle = "Stok değerleme";

            _stockValuationRows = await QueryProductsAsync(includeOnlyLowStock: false);
            StockValuationRows.Clear();
            foreach (var row in _stockValuationRows)
            {
                StockValuationRows.Add(row);
            }

            var totalValue = _stockValuationRows.Sum(p => p.CurrentStock * p.UnitPrice);
            ReportSummary = $"{_stockValuationRows.Count} ürün listelendi. Toplam stok değeri: {totalValue:N2}.";
        }
        catch (Exception ex)
        {
            SetError($"Stok değerleme raporu yüklenemedi: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(IsReportEmpty));
        }
    }

    [RelayCommand]
    private async Task LoadMovementHistoryAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();
            SelectedReport = ReportKind.MovementHistory;
            ReportTitle = "Hareket geçmişi";

            _movementRows = await _db.StockMovements
                .AsNoTracking()
                .Where(m => m.IsActive)
                .OrderByDescending(m => m.MovementDate)
                .ThenByDescending(m => m.Id)
                .Select(m => new StockMovementDto
                {
                    Id = m.Id,
                    DocumentNumber = m.DocumentNumber,
                    Type = m.Type,
                    MovementDate = m.MovementDate,
                    ProductId = m.ProductId,
                    ProductCode = m.Product.Code,
                    ProductName = m.Product.Name,
                    Quantity = m.Quantity,
                    UnitPrice = m.UnitPrice,
                    TotalPrice = m.Quantity * m.UnitPrice,
                    ReferenceNumber = m.ReferenceNumber,
                    Notes = m.Notes
                })
                .ToListAsync();

            MovementRows.Clear();
            foreach (var row in _movementRows)
            {
                MovementRows.Add(row);
            }

            var inbound = _movementRows.Where(m => m.Type == MovementType.Inbound).Sum(m => m.Quantity);
            var outbound = _movementRows.Where(m => m.Type == MovementType.Outbound).Sum(m => m.Quantity);
            ReportSummary = $"{_movementRows.Count} hareket listelendi. Giriş: {inbound:N2}, çıkış: {outbound:N2}.";
        }
        catch (Exception ex)
        {
            SetError($"Hareket geçmişi raporu yüklenemedi: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(IsReportEmpty));
        }
    }

    [RelayCommand]
    private async Task LoadLowStockAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();
            SelectedReport = ReportKind.LowStock;
            ReportTitle = "Düşük stok";

            _lowStockRows = await QueryProductsAsync(includeOnlyLowStock: true);
            LowStockRows.Clear();
            foreach (var row in _lowStockRows)
            {
                LowStockRows.Add(row);
            }

            var outOfStock = _lowStockRows.Count(p => p.CurrentStock <= 0);
            ReportSummary = $"{_lowStockRows.Count} ürün kritik seviyede. Stokta olmayan ürün: {outOfStock}.";
        }
        catch (Exception ex)
        {
            SetError($"Düşük stok raporu yüklenemedi: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(IsReportEmpty));
        }
    }

    [RelayCommand]
    private void ExportToExcel(string? type)
    {
        try
        {
            ClearError();
            var exportType = ResolveExportType(type);
            if (exportType == ReportKind.None)
            {
                SetError("Dışa aktarmak için önce bir rapor yükleyin.");
                return;
            }

            var filePath = CreateExportPath(exportType);
            switch (exportType)
            {
                case ReportKind.StockValuation:
                    if (_stockValuationRows.Count == 0)
                    {
                        SetError("Stok değerleme raporunda dışa aktarılacak veri yok.");
                        return;
                    }
                    _reportService.ExportStockValuation(filePath, _stockValuationRows);
                    break;
                case ReportKind.MovementHistory:
                    if (_movementRows.Count == 0)
                    {
                        SetError("Hareket geçmişi raporunda dışa aktarılacak veri yok.");
                        return;
                    }
                    _reportService.ExportMovementHistory(filePath, _movementRows);
                    break;
                case ReportKind.LowStock:
                    if (_lowStockRows.Count == 0)
                    {
                        SetError("Düşük stok raporunda dışa aktarılacak veri yok.");
                        return;
                    }
                    _reportService.ExportLowStock(filePath, _lowStockRows);
                    break;
            }

            ReportSummary = $"{ReportTitle} dışa aktarıldı: {filePath}";
        }
        catch (Exception ex)
        {
            SetError($"Excel dışa aktarım başarısız: {ex.Message}");
        }
    }

    partial void OnSelectedReportChanged(ReportKind value)
    {
        OnPropertyChanged(nameof(IsStockValuationSelected));
        OnPropertyChanged(nameof(IsMovementHistorySelected));
        OnPropertyChanged(nameof(IsLowStockSelected));
        OnPropertyChanged(nameof(HasReport));
        OnPropertyChanged(nameof(IsReportEmpty));
    }

    private async Task<List<ProductDto>> QueryProductsAsync(bool includeOnlyLowStock)
    {
        var rows = await _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                UnitId = p.UnitId,
                UnitSymbol = p.Unit.Symbol,
                ReorderPoint = p.ReorderPoint,
                UnitPrice = p.UnitPrice,
                CurrentStock = p.StockMovements
                    .Where(m => m.IsActive)
                    .Sum(m => m.Type == MovementType.Inbound ? m.Quantity : -m.Quantity)
            })
            .ToListAsync();

        foreach (var row in rows)
        {
            row.StockStatus = GetStockStatus(row.CurrentStock, row.ReorderPoint);
        }

        var filteredRows = rows
            .Where(p => !includeOnlyLowStock || p.CurrentStock <= p.ReorderPoint)
            .ToList();

        return includeOnlyLowStock
            ? filteredRows.OrderBy(p => p.CurrentStock).ThenBy(p => p.Name).ToList()
            : filteredRows.OrderBy(p => p.Code).ThenBy(p => p.Name).ToList();
    }

    private static StockStatus GetStockStatus(decimal currentStock, decimal reorderPoint)
    {
        if (currentStock <= 0)
        {
            return StockStatus.OutOfStock;
        }

        if (currentStock <= reorderPoint / 2)
        {
            return StockStatus.Critical;
        }

        return currentStock <= reorderPoint ? StockStatus.Low : StockStatus.Normal;
    }

    private ReportKind ResolveExportType(string? type)
    {
        return type?.ToLowerInvariant() switch
        {
            "stock" or "valuation" => ReportKind.StockValuation,
            "movement" or "history" => ReportKind.MovementHistory,
            "low" or "lowstock" => ReportKind.LowStock,
            _ => SelectedReport
        };
    }

    private static string CreateExportPath(ReportKind reportKind)
    {
        var folder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
        {
            folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        var name = reportKind switch
        {
            ReportKind.StockValuation => "stok-degerleme",
            ReportKind.MovementHistory => "hareket-gecmisi",
            ReportKind.LowStock => "dusuk-stok",
            _ => "rapor"
        };

        return Path.Combine(folder, $"{name}-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx");
    }
}

public enum ReportKind
{
    None = 0,
    StockValuation = 1,
    MovementHistory = 2,
    LowStock = 3
}
