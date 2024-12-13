using Microsoft.UI.Xaml;

namespace WinUI3Demo.Core.Helpers;

/// <summary>
/// Helper for theme related operations.
/// </summary>
public class ThemeHelper
{
    public static void SetRequestedThemeAsync(Window window, ElementTheme theme)
    {
        if (window.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = theme;

            TitleBarHelper.UpdateTitleBar(window, theme);
        }
    }
}
