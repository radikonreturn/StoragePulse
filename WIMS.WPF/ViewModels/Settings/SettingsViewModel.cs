using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using WIMS.Core.Entities;
using WIMS.Core.Interfaces;
using WIMS.Data;

namespace WIMS.WPF.ViewModels.Settings;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly WIMSDbContext _db;

    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _companyName = string.Empty;

    [ObservableProperty]
    private string? _companyPhone;

    [ObservableProperty]
    private string? _companyEmail;

    [ObservableProperty]
    private string? _companyAddress;

    [ObservableProperty]
    private string? _taxNumber;

    public SettingsViewModel(WIMSDbContext db)
    {
        _db = db;
        Title = "Ayarlar";
        _ = LoadSettingsAsync();
    }

    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        try
        {
            ClearError();
            var settings = await _db.AppSettings.AsNoTracking().Where(s => s.IsActive).OrderBy(s => s.Id).FirstOrDefaultAsync();
            if (settings is null)
            {
                CompanyName = "StoragePulse";
                return;
            }

            Id = settings.Id;
            CompanyName = settings.CompanyName;
            CompanyPhone = settings.CompanyPhone;
            CompanyEmail = settings.CompanyEmail;
            CompanyAddress = settings.CompanyAddress;
            TaxNumber = settings.TaxNumber;
        }
        catch (Exception ex)
        {
            SetError($"Ayarlar yüklenemedi: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        try
        {
            ClearError();

            if (string.IsNullOrWhiteSpace(CompanyName))
            {
                SetError("Firma adı zorunludur.");
                return;
            }

            AppSettings entity;
            if (Id == 0)
            {
                entity = new AppSettings { CreatedAt = DateTime.UtcNow };
                _db.AppSettings.Add(entity);
            }
            else
            {
                entity = await _db.AppSettings.FirstOrDefaultAsync(s => s.Id == Id && s.IsActive)
                    ?? throw new InvalidOperationException("Ayar kaydı bulunamadı.");
                entity.UpdatedAt = DateTime.UtcNow;
            }

            entity.CompanyName = CompanyName.Trim();
            entity.CompanyPhone = Normalize(CompanyPhone);
            entity.CompanyEmail = Normalize(CompanyEmail);
            entity.CompanyAddress = Normalize(CompanyAddress);
            entity.TaxNumber = Normalize(TaxNumber);

            await _db.SaveChangesAsync();
            Id = entity.Id;
            SetError("Ayarlar kaydedildi.");
        }
        catch (Exception ex)
        {
            SetError($"Ayarlar kaydedilemedi: {ex.Message}");
        }
    }

    [RelayCommand]
    private void BackupDatabase() => SetError("Yedekleme dosya seçici sonraki adımda bağlanacak.");

    [RelayCommand]
    private void RestoreDatabase() => SetError("Geri yükleme dosya seçici sonraki adımda bağlanacak.");

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
