using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WIMS.Application.Services;
using WIMS.Core.Interfaces;
using WIMS.WPF.Services;

namespace WIMS.WPF.ViewModels.Products;

public partial class ProductsViewModel : BaseViewModel, IAsyncInitializable
{
    private readonly IProductCatalogService _productService;
    private readonly INavigationService _navigation;
    private readonly IConfirmationService _confirmationService;
    private List<ProductRow> _allProducts = new();

    public ObservableCollection<ProductRow> Products { get; } = new();

    [ObservableProperty]
    private ProductRow? _selectedProduct;

    [ObservableProperty]
    private string? _searchText;

    public bool HasProducts => Products.Count > 0;
    public bool IsEmpty => !IsBusy && Products.Count == 0;

    public ProductsViewModel(IProductCatalogService productService, INavigationService navigation, IConfirmationService confirmationService)
    {
        _productService = productService;
        _navigation = navigation;
        _confirmationService = confirmationService;
        Title = "Ürün Kartları";
        Products.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasProducts));
            OnPropertyChanged(nameof(IsEmpty));
        };
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
        => LoadProductsAsync(cancellationToken);

    [RelayCommand]
    private async Task LoadProductsAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            OnPropertyChanged(nameof(IsEmpty));
            ClearError();

            var products = await _productService.GetProductsAsync(cancellationToken);
            _allProducts = products.Select(p => new ProductRow
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                CategoryName = p.CategoryName,
                UnitSymbol = p.UnitSymbol,
                ReorderPoint = p.ReorderPoint,
                UnitPrice = p.UnitPrice
            }).ToList();

            ApplyFilter();
        }
        catch (Exception ex)
        {
            SetError($"Ürünler yüklenemedi: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(IsEmpty));
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
        if (IsBusy)
        {
            return;
        }

        var row = product as ProductRow ?? SelectedProduct;
        if (row is null)
        {
            SetError("Silmek için bir ürün seçin.");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();
            var confirmed = await _confirmationService.ConfirmAsync(
                "Ürün silinsin mi?",
                $"{row.Code} - {row.Name} pasif hale getirilecek. Geçmiş hareketler korunur.",
                "Sil",
                "Vazgeç");
            if (!confirmed)
            {
                return;
            }

            var result = await _productService.DeleteProductAsync(row.Id);
            if (!result.Success)
            {
                SetError(result.ErrorMessage ?? "Ürün silinemedi.");
                return;
            }

            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            SetError($"Ürün silinemedi: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(IsEmpty));
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

public partial class ProductsDetailViewModel : BaseViewModel, IAsyncInitializable, IAsyncParameterizedViewModel, IParameterState
{
    private readonly IProductCatalogService _productService;
    private readonly INavigationService _navigation;
    private object? _parameter;

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

    public ProductsDetailViewModel(IProductCatalogService productService, INavigationService navigation)
    {
        _productService = productService;
        _navigation = navigation;
        Title = "Ürün Detayı";
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
        => LoadLookupsAsync(cancellationToken);

    public object? Parameter => _parameter;

    public async Task SetParameterAsync(object parameter, CancellationToken cancellationToken = default)
    {
        _parameter = parameter;
        if (parameter is int id)
        {
            await LoadProductAsync(id, cancellationToken);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            var result = await _productService.SaveProductAsync(new ProductEditModel
            {
                Id = Id,
                Code = Code,
                Name = Name,
                Description = Description,
                CategoryId = CategoryId,
                UnitId = UnitId,
                ReorderPoint = ReorderPoint,
                UnitPrice = UnitPrice
            });
            if (!result.Success)
            {
                SetError(result.ErrorMessage ?? "Ürün kaydedilemedi.");
                return;
            }

            Close();
        }
        catch (Exception ex)
        {
            SetError($"Ürün kaydedilemedi: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel() => Close();

    private async Task LoadLookupsAsync(CancellationToken cancellationToken = default)
    {
        var lookups = await _productService.GetProductLookupsAsync(cancellationToken);

        Categories.Clear();
        foreach (var item in lookups.Categories)
        {
            Categories.Add(new LookupItem(item.Id, item.Name));
        }

        Units.Clear();
        foreach (var item in lookups.Units)
        {
            Units.Add(new LookupItem(item.Id, item.Name));
        }

        CategoryId = CategoryId == 0 ? Categories.FirstOrDefault()?.Id ?? 0 : CategoryId;
        UnitId = UnitId == 0 ? Units.FirstOrDefault()?.Id ?? 0 : UnitId;
    }

    private async Task LoadProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _productService.GetProductAsync(id, cancellationToken);
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
