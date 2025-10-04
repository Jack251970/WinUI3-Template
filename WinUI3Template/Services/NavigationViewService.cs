using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace WinUI3Template.Services;

internal class NavigationViewService(IPageService pageService) : INavigationViewService
{
    private INavigationService _navigationService = null!;
    private readonly IPageService _pageService = pageService;

    private NavigationView? _navigationView;

    public IList<object> MenuItems => _navigationView?.MenuItems ?? [];

    public IList<object> FooterMenuItems => _navigationView?.FooterMenuItems ?? [];

    public object? SettingsItem => _navigationView?.SettingsItem;

    [MemberNotNull(nameof(_navigationView))]
    public void Initialize(NavigationView navigationView)
    {
        _navigationService = Ioc.Default.GetRequiredService<INavigationService>();
        _navigationView = navigationView;
        _navigationView.BackRequested += OnBackRequested;
        _navigationView.ItemInvoked += OnItemInvoked;
    }

    public void UnregisterEvents()
    {
        if (_navigationView != null)
        {
            _navigationView.BackRequested -= OnBackRequested;
            _navigationView.ItemInvoked -= OnItemInvoked;
        }
    }

    public NavigationViewItem? GetSelectedItem()
    {
        return _navigationView?.SelectedItem as NavigationViewItem;
    }

    public void SetNavigateTo(string navigateTo)
    {
        _navigationView!.ItemInvoked -= OnItemInvoked;
        if (navigateTo == _pageService.SettingPageKey)
        {
            _navigationView.SelectedItem = _navigationView.SettingsItem;
        }
        else
        {
            foreach (var item in MenuItems.Concat(FooterMenuItems).OfType<NavigationViewItem>())
            {
                if (NavigationHelper.GetNavigateTo(item) is string pageKey && pageKey == navigateTo)
                {
                    _navigationView.SelectedItem = item;
                    break;
                }
            }
        }
        _navigationView.ItemInvoked += OnItemInvoked;
    }

    public NavigationViewItem? GetItem(Type pageType)
    {
        if (_navigationView != null)
        {
            if (pageType == _pageService.SettingPageType)
            {
                return _navigationView.SettingsItem as NavigationViewItem;
            }

            foreach (var item in MenuItems.Concat(FooterMenuItems).OfType<NavigationViewItem>())
            {
                if (NavigationHelper.GetNavigateTo(item) is string pageKey &&
                    _pageService.GetPageType(pageKey) == pageType)
                {
                    return item;
                }
            }
        }

        return null;
    }

    private void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        _navigationService.GoBack();
    }

    private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked)
        {
            _navigationService.NavigateTo(typeof(SettingsPageViewModel).FullName!);
        }
        else
        {
            var selectedItem = args.InvokedItemContainer as NavigationViewItem;

            if (selectedItem?.GetValue(NavigationHelper.NavigateToProperty) is string pageKey)
            {
                _navigationService.NavigateTo(pageKey);
            }
        }
    }
}
