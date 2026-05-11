using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using WIMS.Core.Entities;
using WIMS.Core.Interfaces;
using WIMS.Data;
using WIMS.WPF.Services;

namespace WIMS.WPF.ViewModels.Products;

public partial class ProductsViewModel : BaseViewModel
{
    private readonly WIMSDbContext _db;
    private readonly INavigationService _navigation;
    private List<ProductRow> _allProducts = new();

    public ObservableCollection<ProductRow> Products { get; } = new();

    [ObservableProperty]
    private ProductRow? _selectedProduct;

    [ObservableProperty]
    private string? _searchText;

    public ProductsViewModel(WIMSDbContext db, INavigationService navigation)
    {
        _db = db;
        _navigation = navigation;
        Title = "Ürün Kartları";
        _ = LoadProductsAsync();
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            _allProducts = await _db.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderBy(p => p.Code)
                .Select(p => new ProductRow
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    CategoryName = p.Category.Name,
                    UnitSymbol = p.Unit.Symbol,
                    ReorderPoint = p.ReorderPoint,
                    UnitPrice = p.UnitPrice
                })
                .ToListAsync();

            ApplyFilter();
        }
        catch (Exception ex)
        {
            SetError($"Ürünler yüklenemedi: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void AddProduct() => _navigation.NavigateTo<ProductsDetailViewModel>();

    [RelayCommand]
    private void EditProduct(object? product)
    {
        var row = product as ProductRow ?? SelectedProduct;
        if (row is null)
        {
            SetError("Düzenlemek için bir ürün seçin.");
            return;
        }

        _navigation.NavigateTo<ProductsDetailViewModel>(row.Id);
    }

    [RelayCommand]
    private async Task DeleteProductAsync(object? product)
    {
        var row = product as ProductRow ?? SelectedProduct;
        if (row is null)
        {
            SetError("Silmek için bir ürün seçin.");
            return;
        }

        try
        {
            ClearError();
            var entity = await _db.Products.FirstOrDefaultAsync(p => p.Id == row.Id && p.IsActive);
            if (entity is null)
            {
                SetError("Ürün bulunamadı.");
                return;
            }

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            SetError($"Ürün silinemedi: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Search(string? query)
    {
        SearchText = query;
        ApplyFilter();
    }

    partial void OnSearchTextChanged(string? value) => ApplyFilter();

    private void ApplyFilter()
    {
        var query = SearchText?.Trim();
        var rows = string.IsNullOrWhiteSpace(query)
            ? _allProducts
            : _allProducts
                .Where(p => p.Code.Contains(query, StringComparison.CurrentCultureIgnoreCase)
                    || p.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase)
                    || p.CategoryName.Contains(query, StringComparison.CurrentCultureIgnoreCase))
                .ToList();

        Products.Clear();
        foreach (var row in rows)
        {
            Products.Add(row);
        }
    }
}

public partial class ProductsDetailViewModel : BaseViewModel, IParameterizedViewModel
{
    private readonly WIMSDbContext _db;
    private readonly INavigationService _navigation;

    public ObservableCollection<LookupItem> Categories { get; } = new();
    public ObservableCollection<LookupItem> Units { get; } = new();

    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _code = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private int _categoryId;

    [ObservableProperty]
    private int _unitId;

    [ObservableProperty]
    private decimal _reorderPoint;

    [ObservableProperty]
    private decimal _unitPrice;

    public ProductsDetailViewModel(WIMSDbContext db, INavigationService navigation)
    {
        _db = db;
        _navigation = navigation;
        Title = "Ürün Detayı";
        _ = LoadLookupsAsync();
    }

    public void SetParameter(object parameter)
    {
        if (parameter is int id)
        {
            _ = LoadProductAsync(id);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            ClearError();

            if (string.IsNullOrWhiteSpace(Code) || string.IsNullOrWhiteSpace(Name))
            {
                SetError("Ürün kodu ve adı zorunludur.");
                return;
            }

            if (CategoryId <= 0 || UnitId <= 0)
            {
                SetError("Kategori ve birim seçin.");
                return;
            }

            Product entity;
            if (Id == 0)
            {
                entity = new Product { CreatedAt = DateTime.UtcNow };
                _db.Products.Add(entity);
            }
            else
            {
                entity = await _db.Products.FirstOrDefaultAsync(p => p.Id == Id && p.IsActive)
                    ?? throw new InvalidOperationException("Ürün bulunamadı.");
                entity.UpdatedAt = DateTime.UtcNow;
            }

            entity.Code = Code.Trim();
            entity.Name = Name.Trim();
            entity.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            entity.CategoryId = CategoryId;
            entity.UnitId = UnitId;
            entity.ReorderPoint = ReorderPoint;
            entity.UnitPrice = UnitPrice;

            await _db.SaveChangesAsync();
            Close();
        }
        catch (DbUpdateException ex)
        {
            SetError($"Ürün kaydedilemedi. Kod benzersiz olmalı. {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            SetError($"Ürün kaydedilemedi: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Cancel() => Close();

    private async Task LoadLookupsAsync()
    {
        Categories.Clear();
        foreach (var item in await _db.Categories.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.Name).Select(c => new LookupItem(c.Id, c.Name)).ToListAsync())
        {
            Categories.Add(item);
        }

        Units.Clear();
        foreach (var item in await _db.Units.AsNoTracking().Where(u => u.IsActive).OrderBy(u => u.Name).Select(u => new LookupItem(u.Id, $"{u.Name} ({u.Symbol})")).ToListAsync())
        {
            Units.Add(item);
        }

        CategoryId = CategoryId == 0 ? Categories.FirstOrDefault()?.Id ?? 0 : CategoryId;
        UnitId = UnitId == 0 ? Units.FirstOrDefault()?.Id ?? 0 : UnitId;
    }

    private async Task LoadProductAsync(int id)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (product is null)
        {
            SetError("Ürün bulunamadı.");
            return;
        }

        Id = product.Id;
        Code = product.Code;
        Name = product.Name;
        Description = product.Description;
        CategoryId = product.CategoryId;
        UnitId = product.UnitId;
        ReorderPoint = product.ReorderPoint;
        UnitPrice = product.UnitPrice;
        Title = "Ürün Düzenle";
    }

    private void Close()
    {
        if (_navigation.CanGoBack)
        {
            _navigation.GoBack();
        }
        else
        {
            _navigation.NavigateTo<ProductsViewModel>();
        }
    }
}

public sealed class ProductRow
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string UnitSymbol { get; set; } = string.Empty;
    public decimal ReorderPoint { get; set; }
    public decimal UnitPrice { get; set; }
}

public sealed record LookupItem(int Id, string Name);
