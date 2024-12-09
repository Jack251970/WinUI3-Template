using System.Runtime.InteropServices;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;
using Windows.UI.ViewManagement;
using Windows.Win32;

namespace WinUI3Template.Views.Windows;

public sealed partial class MainWindow : WindowEx
{
    private readonly DispatcherQueue dispatcherQueue;

    private readonly UISettings settings;

    #region Position & Size

    public PointInt32 Position
    {
        get => AppWindow.Position;
        set => this.Move(value.X, value.Y);
    }

    public SizeInt32 Size
    {
        get => new((int)(AppWindow.Size.Width * 96f / this.GetDpiForWindow()), (int)(AppWindow.Size.Height * 96f / this.GetDpiForWindow()));
        set => this.SetWindowSize(value.Width, value.Height);
    }

    #endregion

    #region UI Elements

    public UIElement? TitleBar { get; set; }

    public UIElement? TitleBarText { get; set; }

    public UIElement? Shell { get; set; }

    #endregion

    #region Manager & Handle

    public WindowManager WindowManager => _manager;
    public IntPtr WindowHandle => _handle;

    private readonly WindowManager _manager;
    private readonly IntPtr _handle;

    #endregion

    public MainWindow()
    {
        InitializeComponent();

        _manager = WindowManager.Get(this);
        _handle = this.GetWindowHandle();

        AppWindow.SetIcon(Constants.AppIconPath);
        Title = ConstantHelper.AppDisplayName;
        Content = null;

        // Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
        dispatcherQueue = DispatcherQueue;
        settings = new UISettings();
        settings.ColorValuesChanged += Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event
    }

    #region Hide & Show & Activate

    private bool activated = false;

    public void Hide()
    {
        WindowExtensions.Hide(this);
    }

    public void Show()
    {
        if (!activated)
        {
            Activate();
        }
        else
        {
            AppWindow.Show();
        }

        if (PInvoke.IsIconic(new(WindowHandle)))
        {
            this.Restore();
        }

        // This handles updating the caption button colors correctly when windows system theme is changed while the app is shown
        TitleBarHelper.ApplySystemThemeToCaptionButtons(this, TitleBarText);
    }

    public new void Activate()
    {
        base.Activate();
        activated = true;
    }

    #endregion

    #region Events

    // This handles updating the caption button colors correctly when windows system theme is changed while the app is open
    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        // This calls comes off-thread, hence we will need to dispatch it to current app's thread
        dispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => TitleBarHelper.ApplySystemThemeToCaptionButtons(this, TitleBarText));
    }

    // This handles updating the caption button colors correctly when windows system theme is changed while the visibility of the app is changed
    private void MainWindow_VisibilityChanged(object sender, WindowVisibilityChangedEventArgs args)
    {
        if (args.Visible)
        {
            dispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => TitleBarHelper.ApplySystemThemeToCaptionButtons(this, TitleBarText));
        }
    }

    // this enables the app to continue running in background after clicking close button
    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
#if TRAY_ICON
        if (!App.CanCloseWindow)
        {
            Hide();
            args.Handled = true;
            return;
        }
#endif
        settings.ColorValuesChanged -= Settings_ColorValuesChanged;
        VisibilityChanged -= MainWindow_VisibilityChanged;
        Closed -= MainWindow_Closed;
        App.Exit();
    }

#if SPLASH_SCREEN
    public void ShowSplashScreen()
    {
        var rootFrame = EnsureWindowIsInitialized(true);

        rootFrame?.Navigate(typeof(SplashScreenPage));
    }
#endif

    public async Task InitializeApplicationAsync(object activatedEventArgs)
    {
        var rootFrame = EnsureWindowIsInitialized(false);

        if (rootFrame is null)
        {
            return;
        }

        // Show window
        if (!Visible)
        {
            // When resuming the cached instance
            AppWindow.Show();
            Activate();

            // Bring to front
            BringToFront();
        }

        // Restore window if minimized
        if (PInvoke.IsIconic(new(WindowHandle)))
        {
            this.Restore();
        }

        await Task.CompletedTask;
    }

    private Frame? EnsureWindowIsInitialized(bool splash)
    {
        try
        {
            if (splash)
            {
                if (Content is not Frame splashFrame)
                {
                    // Create a Frame to act as the navigation context and navigate to the first page
                    splashFrame = new() { CacheSize = 1 };
                    splashFrame.NavigationFailed += (s, e) =>
                    {
                        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
                    };

                    // Place the frame in the current Window
                    Content = splashFrame;
                }

                return splashFrame;
            }
            else
            {
                if (Content is not NavShellPage shell)
                {
                    shell = DependencyExtensions.GetRequiredService<NavShellPage>();
                    if (shell == null)
                    {
                        var frame = new Frame();
                        Content = frame;
                        return frame;
                    }
                    else
                    {
                        Shell = shell;
                        Content = shell;
                        return shell.ShellFrame;
                    }
                }
                else
                {
                    return shell.ShellFrame;
                }
            }
        }
        catch (COMException)
        {
            return null;
        }
    }

    #endregion
}
