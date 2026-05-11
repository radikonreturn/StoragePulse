using CommunityToolkit.Mvvm.ComponentModel;

namespace WIMS.Core.Interfaces;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    protected void ClearError() => ErrorMessage = null;
    protected void SetError(string message) => ErrorMessage = message;
}

public interface IAsyncInitializable
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
