using Microsoft.UI.Xaml.Controls;

namespace WinUI3Template.Views.Pages;

public sealed partial class SettingsPage : Page
{
    public SettingsPageViewModel ViewModel { get; }

    public SettingsPage()
    {
        ViewModel = DependencyExtensions.GetRequiredService<SettingsPageViewModel>();
        InitializeComponent();
    }
}
