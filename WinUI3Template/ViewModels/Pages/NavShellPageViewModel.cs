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

    public INavigationViewService NavigationViewService { get; }

    public NavShellPageViewModel(INavigationService navigationService, INavigationViewService shellService)
    {
        NavigationService = navigationService;
        NavigationViewService = shellService;
        NavigationService.Navigated += OnNavigated;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        // Update the back button status
        IsBackEnabled = NavigationService.CanGoBack;

        // Update the selected NavigationViewItem based on the page type
        var selectedItem = NavigationViewService.GetItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }
}
