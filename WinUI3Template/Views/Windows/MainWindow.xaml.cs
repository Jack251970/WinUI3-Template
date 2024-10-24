using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace WinUI3Template.Views.Windows;

public sealed partial class MainWindow : WindowEx
{
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

    public new bool Visible { get; set; } = false;

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

        Closed += WindowEx_Closed;
    }

    #region Hide & Show & Activate

    private bool activated = false;

    public void Hide()
    {
        WindowExtensions.Hide(this);
        Visible = false;
    }

    public void Show()
    {
        if (!activated)
        {
            Activate();
        }
        else
        {
            WindowExtensions.Show(this);
        }
        Visible = true;
    }

    public new void Activate()
    {
        base.Activate();
        activated = true;
    }

    #endregion

    #region Events

    // this enables the app to continue running in background after clicking close button
    private void WindowEx_Closed(object sender, WindowEventArgs args)
    {
        if (App.CanCloseWindow)
        {
            App.Exit();
        }
        else
        {
            args.Handled = true;
            Hide();
            Visible = false;
        }
    }

    #endregion
}
