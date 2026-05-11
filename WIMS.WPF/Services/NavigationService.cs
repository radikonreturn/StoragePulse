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
        }
    }

    public void NavigateTo<TViewModel>(object parameter) where TViewModel : class
    {
        NavigateTo<TViewModel>();
        if (CurrentView is IParameterizedViewModel pvm)
            pvm.SetParameter(parameter);
    }

    public void GoBack()
    {
        if (_navigationStack.Count > 0)
        {
            var previousType = _navigationStack.Pop();
            var vm = _serviceProvider.GetRequiredService(previousType);
            CurrentView = vm as BaseViewModel;
            OnPropertyChanged(nameof(CanGoBack));
        }
    }
}

public interface IParameterizedViewModel
{
    void SetParameter(object parameter);
}
