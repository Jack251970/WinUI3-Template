using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Navigation;

namespace WinUI3Demo.ViewModels.Pages;

public partial class NavShellViewModel : ObservableRecipient
{
    [ObservableProperty]
    private bool isBackEnabled;

    [ObservableProperty]
    private object? selected;

    public INavigationService NavigationService { get; }

    public INavigationViewService ShellService { get; }

    public NavShellViewModel(INavigationService navigationService, INavigationViewService shellService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        ShellService = shellService;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            Selected = ShellService.SettingsItem;
            return;
        }

        var selectedItem = ShellService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }
}
