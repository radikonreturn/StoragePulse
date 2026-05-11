using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using WIMS.Core.Entities;
using WIMS.Core.Interfaces;
using WIMS.Data;
using WIMS.WPF.Services;

namespace WIMS.WPF.ViewModels.Suppliers;

public partial class SuppliersViewModel : BaseViewModel
{
    private readonly WIMSDbContext _db;
    private readonly INavigationService _navigation;
    private List<SupplierRow> _allSuppliers = new();

    public ObservableCollection<SupplierRow> Suppliers { get; } = new();

    [ObservableProperty]
    private SupplierRow? _selectedSupplier;

    [ObservableProperty]
    private string? _searchText;

    public SuppliersViewModel(WIMSDbContext db, INavigationService navigation)
    {
        _db = db;
        _navigation = navigation;
        Title = "Tedarikçiler";
        _ = LoadSuppliersAsync();
    }

    [RelayCommand]
    private async Task LoadSuppliersAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            _allSuppliers = await _db.Suppliers
                .AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.Code)
                .Select(s => new SupplierRow
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.Name,
                    ContactName = s.ContactName ?? string.Empty,
                    Phone = s.Phone ?? string.Empty,
                    Email = s.Email ?? string.Empty,
                    TaxNumber = s.TaxNumber ?? string.Empty
                })
                .ToListAsync();

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
            var entity = await _db.Suppliers.FirstOrDefaultAsync(s => s.Id == row.Id && s.IsActive);
            if (entity is null)
            {
                SetError("Tedarikçi bulunamadı.");
                return;
            }

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
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

public partial class SuppliersDetailViewModel : BaseViewModel, IParameterizedViewModel
{
    private readonly WIMSDbContext _db;
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

    public SuppliersDetailViewModel(WIMSDbContext db, INavigationService navigation)
    {
        _db = db;
        _navigation = navigation;
        Title = "Tedarikçi Detayı";
    }

    public void SetParameter(object parameter)
    {
        if (parameter is int id)
        {
            _ = LoadSupplierAsync(id);
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
                SetError("Tedarikçi kodu ve adı zorunludur.");
                return;
            }

            Supplier entity;
            if (Id == 0)
            {
                entity = new Supplier { CreatedAt = DateTime.UtcNow };
                _db.Suppliers.Add(entity);
            }
            else
            {
                entity = await _db.Suppliers.FirstOrDefaultAsync(s => s.Id == Id && s.IsActive)
                    ?? throw new InvalidOperationException("Tedarikçi bulunamadı.");
                entity.UpdatedAt = DateTime.UtcNow;
            }

            entity.Code = Code.Trim();
            entity.Name = Name.Trim();
            entity.ContactName = Normalize(ContactName);
            entity.Phone = Normalize(Phone);
            entity.Email = Normalize(Email);
            entity.Address = Normalize(Address);
            entity.TaxNumber = Normalize(TaxNumber);

            await _db.SaveChangesAsync();
            Close();
        }
        catch (DbUpdateException ex)
        {
            SetError($"Tedarikçi kaydedilemedi. Kod benzersiz olmalı. {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            SetError($"Tedarikçi kaydedilemedi: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Cancel() => Close();

    private async Task LoadSupplierAsync(int id)
    {
        var supplier = await _db.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
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

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
