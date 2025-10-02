using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinUI3Template.Views.Pages;

public sealed partial class NavShellPage : Page
{
    public NavShellPageViewModel ViewModel { get; }

    public Frame ShellFrame => NavigationFrame;

    public NavShellPage()
    {
        ViewModel = Ioc.Default.GetRequiredService<NavShellPageViewModel>();
        InitializeComponent();

#if TRAY_ICON
        var trayIcon = new TrayMenuControl();
        ContentArea.Children.Add(trayIcon);

        App.TrayIcon = trayIcon;
#endif

        ViewModel.NavigationService.Frame = ShellFrame;
        ViewModel.ShellService.Initialize(NavigationViewControl);

        // A custom title bar is required for full window theme and Mica support.
        // https://docs.microsoft.com/windows/apps/develop/title-bar?tabs=winui3#full-customization
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
        App.MainWindow.TitleBar = AppTitleBar;

        AppTitleBarText.Text = ConstantHelper.AppDisplayName;

        App.MainWindow.Activated += MainWindow_Activated;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(App.MainWindow, RequestedTheme);
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        App.MainWindow.Activated -= MainWindow_Activated;
        App.MainWindow.TitleBarText = AppTitleBarText;
    }

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        AppTitleBar.Margin = new Thickness()
        {
            Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
            Top = AppTitleBar.Margin.Top,
            Right = AppTitleBar.Margin.Right,
            Bottom = AppTitleBar.Margin.Bottom
        };
    }
}
