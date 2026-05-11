using System.Windows;

namespace WIMS.WPF.Services;

public interface IConfirmationService
{
    Task<bool> ConfirmAsync(string title, string message, string confirmText, string cancelText);
}

public sealed class ConfirmationService : IConfirmationService
{
    public Task<bool> ConfirmAsync(string title, string message, string confirmText, string cancelText)
    {
        var result = MessageBox.Show(
            message,
            title,
            MessageBoxButton.OKCancel,
            MessageBoxImage.Warning,
            MessageBoxResult.Cancel);

        return Task.FromResult(result == MessageBoxResult.OK);
    }
}
