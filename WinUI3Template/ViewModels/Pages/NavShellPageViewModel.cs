using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Navigation;

namespace WinUI3Template.ViewModels.Pages;

public partial class NavShellPageViewModel : ObservableRecipient
{
    [ObservableProperty]
    public partial bool IsBackEnabled { get; set; }

    [ObservableProperty]
    public partial object? Selected { get; set; }

    public INavigationService NavigationService { get; }

    public INavigationViewService ShellService { get; }

    public NavShellPageViewModel(INavigationService navigationService, INavigationViewService shellService)
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
