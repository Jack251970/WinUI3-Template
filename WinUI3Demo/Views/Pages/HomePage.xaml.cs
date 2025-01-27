using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace WinUI3Demo.Views.Pages;

public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel { get; }

    public HomePage()
    {
        ViewModel = DependencyExtensions.GetRequiredService<HomeViewModel>();
        InitializeComponent();

        // Slider control is handling the pointer input internally and marking it as handled
        // So we need to explicitly register the event with AddHandler and set the handledEventsToo parameter to true
        Slider.AddHandler(PointerReleasedEvent, new PointerEventHandler(Slider_PointerReleased), true);

        ContentArea.Loaded += ContentArea_Loaded;
        ContentArea.Unloaded += ContentArea_Unloaded;

        Loaded += HomePage_Loaded;
        Unloaded += HomePage_Unloaded;

        Console.WriteLine("HomePage Constructor");
    }

    private void HomePage_Loaded(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("HomePage_Loaded");
    }

    private void HomePage_Unloaded(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("HomePage_Unloaded");
    }

    private void ContentArea_Loaded(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("ContentArea_Loaded");
    }

    private void ContentArea_Unloaded(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("ContentArea_Unloaded");
    }

    private void Slider_KeyUp(object sender, KeyRoutedEventArgs e)
    {

    }

    private void Slider_PointerReleased(object sender, PointerRoutedEventArgs e)
    {

    }
}
