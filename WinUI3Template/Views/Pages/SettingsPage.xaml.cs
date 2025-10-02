using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace WinUI3Template.Views.Pages;

public sealed partial class SettingsPage : Page
{
    public SettingsPageViewModel ViewModel { get; }

    public SettingsPage()
    {
        ViewModel = Ioc.Default.GetRequiredService<SettingsPageViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }
}
