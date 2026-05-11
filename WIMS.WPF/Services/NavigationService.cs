using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using WIMS.Core.Interfaces;

namespace WIMS.WPF.Services;

public partial class NavigationService : ObservableObject, INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<NavigationEntry> _navigationStack = new();

    [ObservableProperty]
    private BaseViewModel? _currentView;

    public bool CanGoBack => _navigationStack.Count > 0;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo<TViewModel>() where TViewModel : class
        => NavigateToCore<TViewModel>(null, isTopLevel: false, clearHistory: false);

    public void NavigateTo<TViewModel>(object parameter) where TViewModel : class
        => NavigateToCore<TViewModel>(parameter, isTopLevel: false, clearHistory: false);

    public void NavigateToRoot<TViewModel>() where TViewModel : class
        => NavigateToCore<TViewModel>(null, isTopLevel: true, clearHistory: true);

    private void NavigateToCore<TViewModel>(object? parameter, bool isTopLevel, bool clearHistory) where TViewModel : class
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        if (viewModel is BaseViewModel baseVm)
        {
            if (clearHistory)
            {
                _navigationStack.Clear();
                OnPropertyChanged(nameof(CanGoBack));
            }

            if (CurrentView?.GetType() == baseVm.GetType())
            {
                if (parameter is not null && CurrentView is not null)
                {
                    _ = InitializeViewAsync(CurrentView, parameter);
                }

                return;
            }

            if (CurrentView != null && !clearHistory)
                _navigationStack.Push(new NavigationEntry(CurrentView.GetType(), GetCurrentParameter(CurrentView), IsTopLevel: false));

            CurrentView = baseVm;
            OnPropertyChanged(nameof(CanGoBack));
            _ = InitializeViewAsync(baseVm, parameter);
        }
    }

    public void GoBack()
    {
        if (_navigationStack.Count > 0)
        {
            var previousEntry = _navigationStack.Pop();
            var vm = _serviceProvider.GetRequiredService(previousEntry.ViewModelType);
            CurrentView = vm as BaseViewModel;
            OnPropertyChanged(nameof(CanGoBack));
            if (CurrentView is not null)
            {
                _ = InitializeViewAsync(CurrentView, previousEntry.Parameter);
            }
        }
    }

    private static object? GetCurrentParameter(BaseViewModel viewModel)
        => viewModel is IParameterState parameterState ? parameterState.Parameter : null;

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

public interface IParameterState
{
    object? Parameter { get; }
}

public sealed record NavigationEntry(Type ViewModelType, object? Parameter, bool IsTopLevel);
