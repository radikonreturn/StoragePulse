using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using WIMS.Core.Interfaces;

namespace WIMS.WPF.Services;

public partial class NavigationService : ObservableObject, INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<Type> _navigationStack = new();

    [ObservableProperty]
    private BaseViewModel? _currentView;

    public bool CanGoBack => _navigationStack.Count > 0;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo<TViewModel>() where TViewModel : class
        => NavigateToCore<TViewModel>(null);

    public void NavigateTo<TViewModel>(object parameter) where TViewModel : class
        => NavigateToCore<TViewModel>(parameter);

    private void NavigateToCore<TViewModel>(object? parameter) where TViewModel : class
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        if (viewModel is BaseViewModel baseVm)
        {
            if (CurrentView?.GetType() == baseVm.GetType())
                return;

            if (CurrentView != null)
                _navigationStack.Push(CurrentView.GetType());

            CurrentView = baseVm;
            OnPropertyChanged(nameof(CanGoBack));
            _ = InitializeViewAsync(baseVm, parameter);
        }
    }

    public void GoBack()
    {
        if (_navigationStack.Count > 0)
        {
            var previousType = _navigationStack.Pop();
            var vm = _serviceProvider.GetRequiredService(previousType);
            CurrentView = vm as BaseViewModel;
            OnPropertyChanged(nameof(CanGoBack));
            if (CurrentView is not null)
            {
                _ = InitializeViewAsync(CurrentView, null);
            }
        }
    }

    private static async Task InitializeViewAsync(BaseViewModel viewModel, object? parameter)
    {
        try
        {
            if (parameter is not null)
            {
                if (viewModel is IAsyncParameterizedViewModel asyncParameterizedViewModel)
                {
                    await asyncParameterizedViewModel.SetParameterAsync(parameter);
                }
                else if (viewModel is IParameterizedViewModel parameterizedViewModel)
                {
                    parameterizedViewModel.SetParameter(parameter);
                }
            }

            if (viewModel is IAsyncInitializable initializable)
            {
                await initializable.InitializeAsync();
            }
        }
        catch (Exception ex)
        {
            viewModel.ErrorMessage = $"Sayfa yüklenemedi: {ex.Message}";
        }
    }
}

public interface IParameterizedViewModel
{
    void SetParameter(object parameter);
}

public interface IAsyncParameterizedViewModel
{
    Task SetParameterAsync(object parameter, CancellationToken cancellationToken = default);
}
