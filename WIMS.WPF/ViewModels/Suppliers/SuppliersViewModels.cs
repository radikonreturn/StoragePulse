using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WIMS.Application.Services;
using WIMS.Core.Interfaces;
using WIMS.WPF.Services;

namespace WIMS.WPF.ViewModels.Suppliers;

public partial class SuppliersViewModel : BaseViewModel, IAsyncInitializable
{
    private readonly ISupplierCatalogService _supplierService;
    private readonly INavigationService _navigation;
    private List<SupplierRow> _allSuppliers = new();

    public ObservableCollection<SupplierRow> Suppliers { get; } = new();

    [ObservableProperty]
    private SupplierRow? _selectedSupplier;

    [ObservableProperty]
    private string? _searchText;

    public SuppliersViewModel(ISupplierCatalogService supplierService, INavigationService navigation)
    {
        _supplierService = supplierService;
        _navigation = navigation;
        Title = "Tedarikçiler";
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
        => LoadSuppliersAsync(cancellationToken);

    [RelayCommand]
    private async Task LoadSuppliersAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            var suppliers = await _supplierService.GetSuppliersAsync(cancellationToken);
            _allSuppliers = suppliers.Select(s => new SupplierRow
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                ContactName = s.ContactName,
                Phone = s.Phone,
                Email = s.Email,
                TaxNumber = s.TaxNumber
            }).ToList();

            ApplyFilter();
        }
        catch (Exception ex)
        {
            SetError($"Tedarikçiler yüklenemedi: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void AddSupplier() => _navigation.NavigateTo<SuppliersDetailViewModel>();

    [RelayCommand]
    private void EditSupplier(object? supplier)
    {
        var row = supplier as SupplierRow ?? SelectedSupplier;
        if (row is null)
        {
            SetError("Düzenlemek için bir tedarikçi seçin.");
            return;
        }

        _navigation.NavigateTo<SuppliersDetailViewModel>(row.Id);
    }

    [RelayCommand]
    private async Task DeleteSupplierAsync(object? supplier)
    {
        var row = supplier as SupplierRow ?? SelectedSupplier;
        if (row is null)
        {
            SetError("Silmek için bir tedarikçi seçin.");
            return;
        }

        try
        {
            ClearError();
            var result = await _supplierService.DeleteSupplierAsync(row.Id);
            if (!result.Success)
            {
                SetError(result.ErrorMessage ?? "Tedarikçi silinemedi.");
                return;
            }

            await LoadSuppliersAsync();
        }
        catch (Exception ex)
        {
            SetError($"Tedarikçi silinemedi: {ex.Message}");
        }
    }

    partial void OnSearchTextChanged(string? value) => ApplyFilter();

    private void ApplyFilter()
    {
        var query = SearchText?.Trim();
        var rows = string.IsNullOrWhiteSpace(query)
            ? _allSuppliers
            : _allSuppliers
                .Where(s => s.Code.Contains(query, StringComparison.CurrentCultureIgnoreCase)
                    || s.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase)
                    || s.ContactName.Contains(query, StringComparison.CurrentCultureIgnoreCase)
                    || s.Email.Contains(query, StringComparison.CurrentCultureIgnoreCase))
                .ToList();

        Suppliers.Clear();
        foreach (var row in rows)
        {
            Suppliers.Add(row);
        }
    }
}

public partial class SuppliersDetailViewModel : BaseViewModel, IAsyncParameterizedViewModel
{
    private readonly ISupplierCatalogService _supplierService;
    private readonly INavigationService _navigation;

    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _code = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _contactName;

    [ObservableProperty]
    private string? _phone;

    [ObservableProperty]
    private string? _email;

    [ObservableProperty]
    private string? _address;

    [ObservableProperty]
    private string? _taxNumber;

    public SuppliersDetailViewModel(ISupplierCatalogService supplierService, INavigationService navigation)
    {
        _supplierService = supplierService;
        _navigation = navigation;
        Title = "Tedarikçi Detayı";
    }

    public async Task SetParameterAsync(object parameter, CancellationToken cancellationToken = default)
    {
        if (parameter is int id)
        {
            await LoadSupplierAsync(id, cancellationToken);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            ClearError();

            var result = await _supplierService.SaveSupplierAsync(new SupplierEditModel
            {
                Id = Id,
                Code = Code,
                Name = Name,
                ContactName = ContactName,
                Phone = Phone,
                Email = Email,
                Address = Address,
                TaxNumber = TaxNumber
            });
            if (!result.Success)
            {
                SetError(result.ErrorMessage ?? "Tedarikçi kaydedilemedi.");
                return;
            }

            Close();
        }
        catch (Exception ex)
        {
            SetError($"Tedarikçi kaydedilemedi: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Cancel() => Close();

    private async Task LoadSupplierAsync(int id, CancellationToken cancellationToken = default)
    {
        var supplier = await _supplierService.GetSupplierAsync(id, cancellationToken);
        if (supplier is null)
        {
            SetError("Tedarikçi bulunamadı.");
            return;
        }

        Id = supplier.Id;
        Code = supplier.Code;
        Name = supplier.Name;
        ContactName = supplier.ContactName;
        Phone = supplier.Phone;
        Email = supplier.Email;
        Address = supplier.Address;
        TaxNumber = supplier.TaxNumber;
        Title = "Tedarikçi Düzenle";
    }

    private void Close()
    {
        if (_navigation.CanGoBack)
        {
            _navigation.GoBack();
        }
        else
        {
            _navigation.NavigateTo<SuppliersViewModel>();
        }
    }
}

public sealed class SupplierRow
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
}
