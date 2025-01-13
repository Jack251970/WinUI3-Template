using Microsoft.UI.Xaml.Controls;

namespace WinUI3Template.Views.Pages;

public sealed partial class HomePage : Page
{
    public HomePageViewModel ViewModel { get; }

    public HomePage()
    {
        ViewModel = DependencyExtensions.GetRequiredService<HomePageViewModel>();
        InitializeComponent();
    }
}
