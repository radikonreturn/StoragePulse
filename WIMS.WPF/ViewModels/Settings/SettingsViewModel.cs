using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.IO;
using WIMS.Core.Entities;
using WIMS.Core.Interfaces;
using WIMS.Data;
using WIMS.WPF.Services;

namespace WIMS.WPF.ViewModels.Settings;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly WIMSDbContext _db;
    private readonly IConfirmationService _confirmationService;

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

    public bool HasSettings => Id > 0;

    public SettingsViewModel(WIMSDbContext db, IConfirmationService confirmationService)
    {
        _db = db;
        _confirmationService = confirmationService;
        Title = "Ayarlar";
        _ = LoadSettingsAsync();
    }

    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();
            var settings = await _db.AppSettings.AsNoTracking().Where(s => s.IsActive).OrderBy(s => s.Id).FirstOrDefaultAsync();
            if (settings is null)
            {
                CompanyName = "StoragePulse";
                return;
            }

            Id = settings.Id;
            OnPropertyChanged(nameof(HasSettings));
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
        finally
        {
            IsBusy = false;
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
            OnPropertyChanged(nameof(HasSettings));
            SetError("Ayarlar kaydedildi.");
        }
        catch (Exception ex)
        {
            SetError($"Ayarlar kaydedilemedi: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task BackupDatabaseAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            var dbPath = GetDatabasePath();
            if (!File.Exists(dbPath))
            {
                SetError("Yedeklenecek veritabanı dosyası bulunamadı.");
                return;
            }

            var defaultName = $"storagepulse-backup-{DateTime.Now:yyyyMMdd-HHmmss}.db";
            var dialog = new SaveFileDialog
            {
                Title = "Veritabanı yedeği kaydet",
                FileName = defaultName,
                DefaultExt = ".db",
                Filter = "SQLite veritabanı (*.db)|*.db|Tüm dosyalar (*.*)|*.*",
                AddExtension = true,
                OverwritePrompt = true
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            await _db.Database.CloseConnectionAsync();
            File.Copy(dbPath, dialog.FileName, overwrite: true);
            SetError($"Yedek oluşturuldu: {dialog.FileName}");
        }
        catch (Exception ex)
        {
            SetError($"Yedekleme başarısız: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RestoreDatabaseAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            ClearError();

            var dialog = new OpenFileDialog
            {
                Title = "Geri yüklenecek veritabanı yedeğini seç",
                DefaultExt = ".db",
                Filter = "SQLite veritabanı (*.db)|*.db|Tüm dosyalar (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            var confirmed = await _confirmationService.ConfirmAsync(
                "Yedekten dönülsün mü?",
                "Mevcut veritabanı seçilen yedekle değiştirilecek. Devam etmeden önce otomatik güvenlik kopyası alınacak ve işlemden sonra uygulamayı yeniden başlatmanız gerekecek.",
                "Geri yükle",
                "Vazgeç");
            if (!confirmed)
            {
                return;
            }

            IsBusy = true;
            var dbPath = GetDatabasePath();
            var dbDirectory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrWhiteSpace(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }

            await _db.Database.CloseConnectionAsync();
            _db.ChangeTracker.Clear();

            if (File.Exists(dbPath))
            {
                var safetyCopyPath = Path.Combine(
                    dbDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    $"wims-before-restore-{DateTime.Now:yyyyMMdd-HHmmss}.db");
                File.Copy(dbPath, safetyCopyPath, overwrite: false);
            }

            File.Copy(dialog.FileName, dbPath, overwrite: true);
            SetError("Yedek geri yüklendi. Değişikliklerin tamamen uygulanması için uygulamayı kapatıp yeniden açın.");
        }
        catch (Exception ex)
        {
            SetError($"Geri yükleme başarısız: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private string GetDatabasePath()
    {
        var dataSource = _db.Database.GetDbConnection().DataSource;
        if (!string.IsNullOrWhiteSpace(dataSource))
        {
            return dataSource;
        }

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WIMS",
            "wims.db");
    }
}
