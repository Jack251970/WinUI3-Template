using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Controls;

namespace WinUI3Template.Services;

internal class PageService : IPageService
{
    public Type SettingPageType => typeof(SettingsPage);

    public string SettingPageKey => typeof(SettingsPageViewModel).FullName!;

    private readonly Dictionary<string, Type> _pages = [];
    private readonly Dictionary<Type, Type> _subpages = [];

    public PageService()
    {
        // Main Window Pages
        Configure<HomePageViewModel, HomePage>();
        Configure<SettingsPageViewModel, SettingsPage>();
        // TODO: Add pages here

        // Main Window Subpages
        // TODO: Add subpages here
    }

    public Type GetPageType(string viewModel)
    {
        Type? page;
        lock (_pages)
        {
            if (!_pages.TryGetValue(viewModel, out page))
            {
                throw new ArgumentException($"Page not found: {viewModel}. Did you forget to call PageService.Configure?");
            }
        }

        return page;
    }

    public string GetPageKey(Type pageType)
    {
        lock (_pages)
        {
            if (!_pages.ContainsValue(pageType))
            {
                throw new ArgumentException($"Page not found: {pageType}. Did you forget to call PageService.Configure?");
            }

            return _pages.FirstOrDefault(p => p.Value == pageType).Key;
        }
    }

    private void Configure<VM, V>()
        where VM : ObservableObject
        where V : Page
    {
        lock (_pages)
        {
            var viewModel = typeof(VM).FullName!;
            if (_pages.ContainsKey(viewModel))
            {
                throw new ArgumentException($"The key {viewModel} is already configured in PageService!");
            }

            var view = typeof(V);
            if (_pages.ContainsValue(view))
            {
                throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == view).Key}!");
            }

            _pages.Add(viewModel, view);
        }
    }

    public string? GetSubpageKey(Type pageType)
    {
        Type? subpageType;
        lock (_subpages)
        {
            if (!_subpages.TryGetValue(pageType, out subpageType))
            {
                return null;
            }
        }

        return GetPageKey(subpageType);
    }

    private void ConfigureSubpage<V, SV>()
        where V : Page
        where SV : Page
    {
        lock (_subpages)
        {
            var pageType = typeof(V);
            if (_subpages.ContainsKey(pageType))
            {
                throw new ArgumentException($"The type {pageType} is already configured in PageService!");
            }

            var subpageType = typeof(SV);
            _subpages.Add(pageType, subpageType);
        }
    }
}
