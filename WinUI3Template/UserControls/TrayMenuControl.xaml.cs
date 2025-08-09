using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace WinUI3Template.UserControls;

[ObservableObject]
#pragma warning disable MVVMTK0050 // Using [ObservableObject] is not AOT compatible for WinRT
public sealed partial class TrayMenuControl : UserControl
#pragma warning restore MVVMTK0050 // Using [ObservableObject] is not AOT compatible for WinRT
{
    [ObservableProperty]
#pragma warning disable MVVMTK0045 // Using [ObservableProperty] on fields is not AOT compatible for WinRT
    private string _trayIconToolTip = string.Empty;
#pragma warning restore MVVMTK0045 // Using [ObservableProperty] on fields is not AOT compatible for WinRT

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
