using Microsoft.UI.Xaml.Controls;

namespace WinUI3Template.Views.Pages;

public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel { get; }

    public HomePage()
    {
        ViewModel = DependencyExtensions.GetRequiredService<HomeViewModel>();
        InitializeComponent();
    }
}
