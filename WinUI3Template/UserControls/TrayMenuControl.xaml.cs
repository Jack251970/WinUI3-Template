using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace WinUI3Template.UserControls;

public sealed partial class TrayMenuControl : UserControl
{
    public TrayMenuControlViewModel ViewModel { get; } = new();

    public TrayMenuControl()
    {
        InitializeComponent();
    }

    #region Commands

#pragma warning disable CA1822 // Mark members as static
    [RelayCommand]
    private void ShowWindow()
    {
        App.MainWindow.Show();
        App.MainWindow.BringToFront();
    }
#pragma warning restore CA1822 // Mark members as static

    [RelayCommand]
    private void ExitApp()
    {
        App.MainWindow.Hide();
        DisposeTrayIconControl();
#if TRAY_ICON
        App.CanCloseWindow = true;
#endif
        App.MainWindow.Close();
    }

    private void DisposeTrayIconControl()
    {
        try
        {
            TrayIconControl.Dispose();
        }
        catch { }
    }

    #endregion
}

public partial class TrayMenuControlViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string TrayIconToolTip { get; set; } = ConstantHelper.AppDisplayName;
}
