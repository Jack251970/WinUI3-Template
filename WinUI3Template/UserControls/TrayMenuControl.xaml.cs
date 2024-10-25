using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace WinUI3Template.UserControls;

[ObservableObject]
public sealed partial class TrayMenuControl : UserControl
{
    [ObservableProperty]
    private string _appDisplayName = ConstantHelper.AppDisplayName;

    public TrayMenuControl()
    {
        InitializeComponent();
    }

    #region Commands

#pragma warning disable CA1822 // Mark members as static
    [RelayCommand]
    private void ShowWindow()
    {
        App.ShowMainWindow(false);
    }
#pragma warning restore CA1822 // Mark members as static

    [RelayCommand]
    private void ExitApp()
    {
        DisposeTrayIconControl();
        App.CanCloseWindow = true;
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
