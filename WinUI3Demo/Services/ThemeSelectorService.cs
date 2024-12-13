using Microsoft.UI.Xaml;

namespace WinUI3Demo.Services;

internal class ThemeSelectorService(IAppSettingsService appSettingsService) : IThemeSelectorService
{
    public ElementTheme Theme => _appSettingsService.Theme;

    public event EventHandler<ElementTheme>? ThemeChanged;

    private readonly IAppSettingsService _appSettingsService = appSettingsService;

    public async Task SetThemeAsync(ElementTheme theme)
    {
        await SetRequestedThemeAsync(App.MainWindow, theme);

        await WindowsExtensions.GetAllWindows().EnqueueOrInvokeAsync(
            async (window) => await SetRequestedThemeAsync(window, theme), 
            Microsoft.UI.Dispatching.DispatcherQueuePriority.High);

        ThemeChanged?.Invoke(this, Theme);

        await _appSettingsService.SaveThemeInSettingsAsync(theme);
    }

    public async Task SetRequestedThemeAsync(Window window)
    {
        await SetRequestedThemeAsync(window, Theme);
    }

    private static async Task SetRequestedThemeAsync(Window window, ElementTheme theme)
    {
        ThemeHelper.SetRequestedThemeAsync(window, theme);

        await Task.CompletedTask;
    }

    public bool IsDarkTheme()
    {
        // If theme is Default, use the Application.RequestedTheme value
        // https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.elementtheme?view=windows-app-sdk-1.2#fields
        return Theme == ElementTheme.Dark ||
            (Theme == ElementTheme.Default && Application.Current.RequestedTheme == ApplicationTheme.Dark);
    }

    public ElementTheme GetActualTheme()
    {
        return IsDarkTheme() ? ElementTheme.Dark : ElementTheme.Light;
    }
}
